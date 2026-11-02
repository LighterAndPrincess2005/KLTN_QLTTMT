using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MyThuat.Api.Data;
using MyThuat.Api.Models;
using MyThuat.Api.Security;
using MyThuat.Api.Services;
namespace MyThuat.Api.Controllers;
[ApiController,Authorize,Route("api/thong-bao")]
public sealed class ThongBaoController(MyThuatDbContext db,AuditService audit):ControllerBase
{
    [HttpGet("toi")]
    public async Task<object> Mine(CancellationToken ct){var member=User.MemberId();ApiError.Require(member.HasValue,"Tài khoản chưa liên kết thành viên.",403);return await db.Set<ThongBao>().AsNoTracking().Where(x=>x.ThanhVienId==member).OrderByDescending(x=>x.Id).Select(x=>new{x.Id,x.Loai,x.TieuDe,x.NoiDung,x.CreatedAt,x.DaDocLuc}).Take(100).ToListAsync(ct);}
    [HttpPost("{id:long}/da-doc")]
    public async Task<IActionResult> Read(long id,CancellationToken ct){var row=await db.Set<ThongBao>().SingleOrDefaultAsync(x=>x.Id==id&&x.ThanhVienId!=null&&x.ThanhVienId==User.MemberId(),ct)??throw new ApiError(404,"Không có thông báo.");row.DaDocLuc??=BusinessClock.Now;await db.SaveChangesAsync(ct);return NoContent();}
    [Authorize(Policy="HoSo"),HttpGet]
    public async Task<object> List(int trang=1,CancellationToken ct=default){ApiError.Require(trang>0,"Trang không hợp lệ.");var q=db.Set<ThongBao>().AsNoTracking();return new{TongSo=await q.CountAsync(ct),DuLieu=await q.OrderByDescending(x=>x.Id).Skip((trang-1)*20).Take(20).Select(x=>new{x.Id,x.TieuDe,x.NoiDung,x.Email,x.DienThoai,x.TrangThaiEmail,x.SoLanGui,x.GuiEmailLuc,x.DaGoiLuc,x.CreatedAt}).ToListAsync(ct)};}
    [Authorize(Policy="HoSo"),HttpPost("{id:long}/da-goi")]
    public async Task<IActionResult> Called(long id,CancellationToken ct){var row=await db.Set<ThongBao>().SingleOrDefaultAsync(x=>x.Id==id,ct)??throw new ApiError(404,"Không có thông báo.");ApiError.Require(!string.IsNullOrWhiteSpace(row.DienThoai),"Chưa có số điện thoại.");row.DaGoiLuc=BusinessClock.Now;row.NguoiGoiId=User.AccountId();audit.Add("ThongBao",id,"GHI_NHAN_DA_GOI");await db.SaveChangesAsync(ct);return NoContent();}
    [Authorize(Policy="HoSo"),HttpPost("{id:long}/gui-lai")]
    public async Task<IActionResult> Retry(long id,CancellationToken ct){var row=await db.Set<ThongBao>().SingleOrDefaultAsync(x=>x.Id==id,ct)??throw new ApiError(404,"Không có thông báo.");ApiError.Require(row.TrangThaiEmail=="GUI_LOI","Chỉ gửi lại email đã lỗi.",409);row.TrangThaiEmail="CHO_GUI";row.SoLanGui=0;row.ThuLaiLuc=null;audit.Add("ThongBao",id,"GUI_LAI_EMAIL");await db.SaveChangesAsync(ct);return NoContent();}
}
