using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MyThuat.Api.Contracts;
using MyThuat.Api.Data;
using MyThuat.Api.Models;
using MyThuat.Api.Security;
using MyThuat.Api.Services;
namespace MyThuat.Api.Controllers;
[ApiController,Authorize,Route("api/phan-hoi")]
public sealed class PhanHoiController(MyThuatDbContext db,AccessService access,AuditService audit):ControllerBase
{
    [HttpPost]
    public async Task<object> Send(FeedbackRequest r,CancellationToken ct)
    {
        ApiError.Require(!r.NoiDung.TrimStart().StartsWith("[HOC_BU:",StringComparison.Ordinal)&&!r.NoiDung.TrimStart().StartsWith("[DOI_LICH:",StringComparison.Ordinal),
            "Vui lòng dùng mục Học bù hoặc Đổi lịch để gửi đề nghị này.");
        var member=User.MemberId();ApiError.Require(member.HasValue,"Tài khoản cần liên kết thành viên.",403);
        var e=await db.DangKy.Include(x=>x.HoSoTheoHoc).SingleOrDefaultAsync(x=>x.Id==r.DangKyId,ct)??throw new ApiError(404,"Không có đăng ký.");
        await access.Student(e.HoSoTheoHoc!.HocVienId,false,ct);
        var rule=await db.QuyDinh.SingleAsync(x=>x.Id==e.HoSoTheoHoc!.QuyDinhId,ct);
        var settings=PolicySettings.Parse(rule.NoiDungJson);
        var f=new PhanHoi {DangKyId=e.Id,NguoiGuiId=member!.Value,NoiDung=r.NoiDung,GuiLuc=BusinessClock.Now,
            HanPhanHoi=BusinessClock.AddWorkingDays(BusinessClock.Now,2,settings.NgayNghi),TrangThai="CHO_XU_LY"};
        db.PhanHoi.Add(f);await db.SaveChangesAsync(ct);audit.Add("PhanHoi",f.Id,"GUI_PHAN_HOI");await db.SaveChangesAsync(ct);return ManagementService.Scalars(f);
    }
    [HttpGet]
    public async Task<object> List(CancellationToken ct)
    {
        var q=db.PhanHoi.AsNoTracking();if(!User.Has(Permission.HoSo))q=q.Where(x=>x.NguoiGuiId==User.MemberId());
        return (await q.OrderByDescending(x=>x.GuiLuc).Take(500).ToListAsync(ct)).Select(ManagementService.Scalars);
    }
    [Authorize(Policy="HoSo"),HttpPost("{id:long:min(1)}/tra-loi")]
    public async Task<object> Reply(long id,FeedbackReply r,CancellationToken ct)
    {
        var f=await db.PhanHoi.SingleOrDefaultAsync(x=>x.Id==id,ct)??throw new ApiError(404,"Không có phản hồi.");
        f.TraLoi=r.TraLoi;f.TraLoiLuc=BusinessClock.Now;f.NguoiXuLy=User.AccountId();f.TrangThai="DA_TRA_LOI";
        audit.Add("PhanHoi",id,"TRA_LOI");await db.SaveChangesAsync(ct);return ManagementService.Scalars(f);
    }
}
