using System.ComponentModel.DataAnnotations;
using System.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MyThuat.Api.Contracts;
using MyThuat.Api.Data;
using MyThuat.Api.Models;
using MyThuat.Api.Security;
using MyThuat.Api.Services;
namespace MyThuat.Api.Controllers;
public sealed record ClassNoticeRequest([Required,StringLength(150,MinimumLength=3)]string TieuDe,[Required,StringLength(3000,MinimumLength=3)]string NoiDung,[Required,StringLength(64,MinimumLength=16)]string MaYeuCau);
[ApiController,Authorize,Route("api/giao-vien")]
public sealed class GiaoVienLopController(MyThuatDbContext db,NotificationService notifications,ScheduleService schedules,AuditService audit):ControllerBase
{
    async Task<LopHoc> OwnClass(long id,CancellationToken ct){
        var teacher=User.TeacherId();ApiError.Require(teacher.HasValue,"Cần tài khoản giáo viên.",403);
        ApiError.Require(await db.GiangDayBuoi.AnyAsync(x=>x.GiaoVienId==teacher&&x.BuoiHoc!.LopHocId==id&&x.BuoiHoc.TrangThai!="DA_HUY",ct),"Bạn không được phân công lớp này.",403);
        var cls=await db.LopHoc.SingleOrDefaultAsync(x=>x.Id==id,ct)??throw new ApiError(404,"Không có lớp.");
        ApiError.Require(cls.TrangThai is "DANG_HOC" or "DANG_TUYEN_SINH","Lớp không còn hoạt động để gửi thông báo.",409);return cls;
    }
    async Task<List<ThanhVien>> Contacts(long id,CancellationToken ct){
        var students=db.DangKy.Where(x=>x.LopHocId==id&&x.TrangThai=="DA_XAC_NHAN").Select(x=>x.HoSoTheoHoc!.HocVienId);
        var now=BusinessClock.Now;return await db.DaiDienHocVien.Where(x=>students.Contains(x.HocVienId)&&x.XacNhanLuc!=null&&x.HieuLucTu<=now&&(x.HieuLucDen==null||x.HieuLucDen>now)&&(x.QuyenDaiDien=="DAY_DU"||x.QuyenDaiDien=="XEM_KET_QUA")&&x.ThanhVien!.TrangThai=="HOAT_DONG").Select(x=>x.ThanhVien!).Distinct().ToListAsync(ct);
    }
    static bool CanEmail(ThanhVien m)=>!string.IsNullOrWhiteSpace(m.Email)&&!m.Email.EndsWith(".invalid",StringComparison.OrdinalIgnoreCase);
    [HttpGet("lop/{id:long:min(1)}/nguoi-nhan")]
    public async Task<object> Preview(long id,CancellationToken ct){await OwnClass(id,ct);var contacts=await Contacts(id,ct);return new{SoHocVien=await db.DangKy.Where(x=>x.LopHocId==id&&x.TrangThai=="DA_XAC_NHAN").Select(x=>x.HoSoTheoHoc!.HocVienId).Distinct().CountAsync(ct),SoNguoiNhan=contacts.Count,SoEmail=contacts.Count(CanEmail)};}
    [HttpPost("lop/{id:long:min(1)}/thong-bao")]
    public async Task<object> Send(long id,ClassNoticeRequest r,CancellationToken ct){
        await using var tx=await db.Database.BeginTransactionAsync(IsolationLevel.Serializable,ct);
        var cls=await OwnClass(id,ct);var account=User.AccountId();var title=r.TieuDe.Trim();var text=r.NoiDung.Trim();ApiError.Require(title.Length>=3&&text.Length>=3,"Điền tiêu đề và nội dung thông báo.");
        var previous=await db.Set<ThongBaoLop>().SingleOrDefaultAsync(x=>x.TaiKhoanId==account&&x.MaYeuCau==r.MaYeuCau,ct);
        if(previous!=null){ApiError.Require(previous.LopHocId==id&&previous.TieuDe==title&&previous.NoiDung==text,"Yêu cầu gửi đã được dùng cho nội dung khác.",409);return new{previous.SoNguoiNhan,previous.SoEmail,DaTiepNhan=true};}
        var cooldown=BusinessClock.Now.AddSeconds(-60);
        ApiError.Require(!await db.Set<ThongBaoLop>().AnyAsync(x=>x.TaiKhoanId==account&&x.LopHocId==id&&x.CreatedAt>cooldown,ct),"Vui lòng chờ 60 giây trước khi gửi thông báo khác cho lớp.",429);
        var contacts=await Contacts(id,ct);ApiError.Require(contacts.Count>0,"Chưa có học viên xác nhận hoặc người liên hệ hợp lệ để nhận thông báo.",409);
        var batch=new ThongBaoLop{LopHocId=id,TaiKhoanId=account,MaYeuCau=r.MaYeuCau,TieuDe=title,NoiDung=text,SoNguoiNhan=contacts.Count,SoEmail=contacts.Count(CanEmail)};db.Add(batch);
        foreach(var m in contacts)notifications.Add(m.Id,m.Email,m.DienThoai,"THONG_BAO_LOP",title,"Lớp: "+cls.TenLop+"\n\n"+text+"\n\nThông báo từ giáo viên. Lịch chính thức được cập nhật trong Góc học tập; lời nhắn không tự thay đổi lịch.");
        audit.Add("LopHoc",id,"GIAO_VIEN_THONG_BAO",new{SoNguoiNhan=contacts.Count});await db.SaveChangesAsync(ct);await tx.CommitAsync(ct);return new{batch.SoNguoiNhan,batch.SoEmail,DaTiepNhan=true};
    }
    [HttpPost("buoi/{id:long:min(1)}/xep-bu")]
    public Task<object> Reschedule(long id,RescheduleRequest r,CancellationToken ct)=>schedules.TeacherMove(id,r,ct);
}
