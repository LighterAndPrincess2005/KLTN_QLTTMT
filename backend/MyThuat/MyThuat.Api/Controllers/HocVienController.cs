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
[ApiController,Authorize,Route("api/hoc-vien")]
public sealed class HocVienController(MyThuatDbContext db,AuditService audit):ControllerBase
{
    [HttpGet("toi")]
    public async Task<object> Mine(CancellationToken ct)
    {
        var member=User.MemberId();ApiError.Require(member.HasValue,"Tài khoản không liên kết thành viên.",403);
        var now=BusinessClock.Now;
        return await db.DaiDienHocVien.AsNoTracking().Where(x=>x.ThanhVienId==member&&x.HieuLucTu<=now&&(x.HieuLucDen==null||x.HieuLucDen>now))
            .Select(x=>new {x.Id,x.HocVienId,x.HocVien!.HoTen,x.HocVien.NgaySinh,x.HocVien.TrangThai,x.QuanHe,x.QuyenDaiDien,x.XacNhanLuc,x.HocVien.CapDoDeXuat,
                x.HocVien.LienHeKhanCapTen,x.HocVien.LienHeKhanCapSDT,x.HocVien.MucTieuHoc,x.HocVien.RowVersion,
                CanXoa=x.XacNhanLuc!=null&&x.QuyenDaiDien=="DAY_DU"&&!db.HoSoTheoHoc.Any(h=>h.HocVienId==x.HocVienId)&&db.DaiDienHocVien.Count(d=>d.HocVienId==x.HocVienId)==1,
                CanSua=x.XacNhanLuc!=null&&x.HocVien.TrangThai=="HOAT_DONG"&&(x.QuyenDaiDien=="DAY_DU"||x.QuyenDaiDien=="DANG_KY")}).ToListAsync(ct);
    }
    [HttpDelete("toi/{id:long:min(1)}")]
    public async Task<IActionResult> Delete(long id,CancellationToken ct){await using var tx=await db.Database.BeginTransactionAsync(IsolationLevel.Serializable,ct);var member=User.MemberId();var now=BusinessClock.Now;var link=await db.DaiDienHocVien.SingleOrDefaultAsync(x=>x.HocVienId==id&&x.ThanhVienId==member&&x.QuyenDaiDien=="DAY_DU"&&x.XacNhanLuc!=null&&x.HieuLucTu<=now&&(x.HieuLucDen==null||x.HieuLucDen>now),ct)??throw new ApiError(403,"Không có quyền xóa hồ sơ.");ApiError.Require(!await db.HoSoTheoHoc.AnyAsync(x=>x.HocVienId==id,ct)&&await db.DaiDienHocVien.CountAsync(x=>x.HocVienId==id,ct)==1,"Hồ sơ có lịch sử hoặc người đại diện khác; không được xóa.",409);var student=await db.HocVien.SingleAsync(x=>x.Id==id,ct);audit.Add("HocVien",id,"XOA_HO_SO_CHUA_SU_DUNG");db.DaiDienHocVien.Remove(link);db.HocVien.Remove(student);await db.SaveChangesAsync(ct);await tx.CommitAsync(ct);return NoContent();}
    [HttpPut("toi/{id:long:min(1)}")]
    public async Task<IActionResult> Update(long id,UpdateStudentRequest r,CancellationToken ct)
    {
        var member=User.MemberId();var now=BusinessClock.Now;
        var link=await db.DaiDienHocVien.SingleOrDefaultAsync(x=>x.ThanhVienId==member&&x.HocVienId==id&&x.HieuLucTu<=now&&(x.HieuLucDen==null||x.HieuLucDen>now),ct);
        ApiError.Require(member.HasValue&&link?.XacNhanLuc!=null&&link.QuyenDaiDien is "DAY_DU" or "DANG_KY","Không có quyền sửa hồ sơ này.",403);
        ApiError.Require(await db.ThanhVien.AnyAsync(x=>x.Id==member&&x.TrangThai=="HOAT_DONG",ct),"Thành viên không hoạt động.",403);
        ApiError.Require(r.NgaySinh.Year>=1900&&r.NgaySinh<=BusinessClock.Today,"Ngày sinh không hợp lệ.");
        var student=await db.HocVien.SingleAsync(x=>x.Id==id,ct);
        ApiError.Require(student.TrangThai=="HOAT_DONG","Hồ sơ không hoạt động.",403);
        ApiError.Require(Convert.ToBase64String(student.RowVersion)==r.RowVersion,"Hồ sơ đã thay đổi. Đóng và mở lại để tải bản mới nhất.",409);
        if(student.NgaySinh!=r.NgaySinh){
            var registrations=await db.DangKy.Where(x=>x.HoSoTheoHoc!.HocVienId==id&&(x.TrangThai=="DA_XAC_NHAN"||((x.TrangThai=="CHO_THANH_TOAN"||x.TrangThai=="CHO_CHUYEN")&&x.HanGiuCho>now)))
                .Select(x=>new {Date=x.LopHoc!.NgayKhaiGiangDuKien,MinAge=x.LopHoc.KhoaHoc!.TuoiToiThieu}).ToListAsync(ct);
            foreach(var registration in registrations){
                var age=registration.Date.Year-r.NgaySinh.Year-(registration.Date<r.NgaySinh.AddYears(registration.Date.Year-r.NgaySinh.Year)?1:0);
                ApiError.Require(age>=(registration.MinAge??0)&&(age>=18||link!.QuanHe!="TU_BAN_THAN"),"Ngày sinh mới không đáp ứng điều kiện tuổi của lớp đã đăng ký. Vui lòng liên hệ trung tâm để kiểm tra.");
            }
        }
        student.HoTen=r.HoTen.Trim();student.NgaySinh=r.NgaySinh;student.LienHeKhanCapTen=r.LienHeKhanCapTen.Trim();student.LienHeKhanCapSDT=r.LienHeKhanCapSDT.Trim();student.MucTieuHoc=r.MucTieuHoc;
        audit.Add("HocVien",id,"SUA_HO_SO_TU_PHUC_VU");await db.SaveChangesAsync(ct);return NoContent();
    }
    [HttpPost("toi")]
    public async Task<object> Add(StudentRequest r,CancellationToken ct)
    {
        var member=User.MemberId();ApiError.Require(member.HasValue,"Tài khoản không liên kết thành viên.",403);
        ApiError.Require(r.NgaySinh.Year>=1900&&r.NgaySinh<=BusinessClock.Today&&r.QuanHe is "CHA" or "ME" or "GIAM_HO" or "TU_BAN_THAN","Ngày sinh hoặc quan hệ không hợp lệ.");
        await using var tx=await db.Database.BeginTransactionAsync(IsolationLevel.Serializable,ct);
        var owner=await db.ThanhVien.SingleOrDefaultAsync(x=>x.Id==member,ct)??throw new ApiError(403,"Không có thành viên tương ứng.");
        ApiError.Require(owner.TrangThai is "HOAT_DONG" or "CHO_XAC_NHAN","Tài khoản thành viên không hoạt động.",403);
        // Self-service creates a new student owned by this member; it never links an existing student's ID.
        var now=BusinessClock.Now;
        if(owner.TrangThai=="CHO_XAC_NHAN"){owner.TrangThai="HOAT_DONG";owner.XacNhanThanhVienLuc=now;audit.Add("ThanhVien",owner.Id,"KICH_HOAT_TU_PHUC_VU");}
        if(r.QuanHe=="TU_BAN_THAN"&&owner.NgaySinh is null){owner.NgaySinh=r.NgaySinh;audit.Add("ThanhVien",owner.Id,"BO_SUNG_NGAY_SINH_BAN_THAN");}
        var student=new HocVien {MaHocVien="HV"+Guid.NewGuid().ToString("N")[..16],HoTen=r.HoTen,NgaySinh=r.NgaySinh,
            LienHeKhanCapTen=r.LienHeKhanCapTen,LienHeKhanCapSDT=r.LienHeKhanCapSDT,MucTieuHoc=r.MucTieuHoc,TrangThai="HOAT_DONG"};
        db.HocVien.Add(student);await db.SaveChangesAsync(ct);
        db.DaiDienHocVien.Add(new(){ThanhVienId=member!.Value,HocVienId=student.Id,QuanHe=r.QuanHe,QuyenDaiDien="DAY_DU",LaLienHeChinh=true,HieuLucTu=now,XacNhanLuc=now,NguoiXacNhan=User.AccountId()});
        audit.Add("HocVien",student.Id,"TAO_HO_SO_TU_PHUC_VU");await db.SaveChangesAsync(ct);await tx.CommitAsync(ct);return new {student.Id,student.HoTen,student.TrangThai};
    }
    [HttpPost("{id:long:min(1)}/danh-gia")]
    public async Task<IActionResult> Assess(long id,AssessRequest r,CancellationToken ct)
    {
        ApiError.Require(User.Has(Permission.HoSo)||User.TeacherId()==r.GiaoVienId,"Không có quyền đánh giá.",403);
        var student=await db.HocVien.SingleOrDefaultAsync(x=>x.Id==id,ct)??throw new ApiError(404,"Không có học viên.");
        if(!User.Has(Permission.HoSo))ApiError.Require(student.GiaoVienDanhGia==r.GiaoVienId,"Học vụ chưa chỉ định bạn đánh giá học viên này.",403);
        ApiError.Require(await db.GiaoVien.AnyAsync(x=>x.Id==r.GiaoVienId&&x.TrangThai=="HOAT_DONG",ct),"Giáo viên không hoạt động.");
        student.GiaoVienDanhGia=r.GiaoVienId;student.CapDoDeXuat=r.CapDoDeXuat;student.NhanXetDauVao=r.NhanXet;student.NgayDanhGiaDauVao=BusinessClock.Today;
        audit.Add("HocVien",id,"DANH_GIA_DAU_VAO",new {r.GiaoVienId,r.CapDoDeXuat});await db.SaveChangesAsync(ct);return NoContent();
    }
    [Authorize(Policy="HoSo"),HttpPost("{id:long:min(1)}/chi-dinh/{teacher:long:min(1)}")]
    public async Task<IActionResult> AssignAssessment(long id,long teacher,CancellationToken ct)
    {
        var student=await db.HocVien.SingleOrDefaultAsync(x=>x.Id==id,ct)??throw new ApiError(404,"Không có học viên.");
        ApiError.Require(await db.GiaoVien.AnyAsync(x=>x.Id==teacher&&x.TrangThai=="HOAT_DONG",ct),"Giáo viên không hoạt động.");
        student.GiaoVienDanhGia=teacher;student.NgayDanhGiaDauVao=null;student.CapDoDeXuat=null;student.NhanXetDauVao=null;
        audit.Add("HocVien",id,"CHI_DINH_DANH_GIA",new {GiaoVienId=teacher});await db.SaveChangesAsync(ct);return NoContent();
    }
    [HttpGet("danh-gia-duoc-giao")]
    public async Task<object> Assessments(CancellationToken ct)
    {
        ApiError.Require(User.TeacherId().HasValue,"Tài khoản không phải giáo viên.",403);
        return await db.HocVien.AsNoTracking().Where(x=>x.GiaoVienDanhGia==User.TeacherId())
            .Select(x=>new {x.Id,x.HoTen,x.NgaySinh,x.MucTieuHoc,x.NhanXetDauVao,x.CapDoDeXuat,x.NgayDanhGiaDauVao}).Take(500).ToListAsync(ct);
    }
}
