using System.Security.Cryptography;
using System.Text;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.EntityFrameworkCore;
using MyThuat.Api.Contracts;
using MyThuat.Api.Data;
using MyThuat.Api.Models;
using MyThuat.Api.Security;
using MyThuat.Api.Services;
namespace MyThuat.Api.Controllers;
[ApiController,Route("api/tu-van")]
public sealed class TuVanController(MyThuatDbContext db,AuditService audit):ControllerBase
{
    public static string Hash(string value)=>Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(value)));
    [AllowAnonymous,EnableRateLimiting("consultation"),HttpPost]
    public async Task<object> Send(ConsultationRequest r,CancellationToken ct){
        if(r.KhoaHocId.HasValue)ApiError.Require(await db.KhoaHoc.AnyAsync(x=>x.Id==r.KhoaHocId&&x.TrangThai=="HOAT_DONG",ct),"Khóa học không được công bố.");
        var secret=Convert.ToHexString(RandomNumberGenerator.GetBytes(32));
        var row=new TuVan{MaTraCuu="TV"+Guid.NewGuid().ToString("N")[..20].ToUpperInvariant(),KhoaTraCuuHash=Hash(secret),ThanhVienId=User.MemberId(),KhoaHocId=r.KhoaHocId,HoTen=r.HoTen.Trim(),Email=r.Email.Trim(),DienThoai=r.DienThoai,ChuDe=r.ChuDe.Trim(),GuiLuc=BusinessClock.Now,TrangThai="MOI_GUI"};
        await using var tx=await db.Database.BeginTransactionAsync(ct);db.TuVan.Add(row);await db.SaveChangesAsync(ct);
        db.TinNhanTuVan.Add(new(){TuVanId=row.Id,NoiDung=r.NoiDung.Trim(),GuiLuc=BusinessClock.Now,TuNhanVien=false,TaiKhoanId=User.Identity?.IsAuthenticated==true?User.AccountId():null});await db.SaveChangesAsync(ct);await tx.CommitAsync(ct);
        return new {row.MaTraCuu,KhoaTraCuu=secret,row.TrangThai,row.GuiLuc};
    }
    async Task<TuVan> BySecret(string code,string secret,CancellationToken ct){
        var row=await db.TuVan.SingleOrDefaultAsync(x=>x.MaTraCuu==code,ct);
        ApiError.Require(row!=null&&CryptographicOperations.FixedTimeEquals(Encoding.UTF8.GetBytes(row.KhoaTraCuuHash),Encoding.UTF8.GetBytes(Hash(secret))),"Mã hoặc khóa tra cứu không đúng.",404);return row!;
    }
    async Task<object> Detail(TuVan row,CancellationToken ct)=>new {row.MaTraCuu,row.ChuDe,row.TrangThai,row.GuiLuc,row.DocLuc,row.RowVersion,TinNhan=await db.TinNhanTuVan.AsNoTracking().Where(x=>x.TuVanId==row.Id).OrderBy(x=>x.GuiLuc).ThenBy(x=>x.Id).Select(x=>new{x.Id,x.NoiDung,x.TuNhanVien,x.GuiLuc}).ToListAsync(ct)};
    [AllowAnonymous,EnableRateLimiting("consultation"),HttpPost("tra-cuu")]
    public async Task<object> Lookup(ConsultationAccess r,CancellationToken ct)=>await Detail(await BySecret(r.MaTraCuu,r.KhoaTraCuu,ct),ct);
    [AllowAnonymous,EnableRateLimiting("consultation"),HttpPost("bo-sung")]
    public async Task<IActionResult> Followup(ConsultationFollowup r,CancellationToken ct){
        var row=await BySecret(r.MaTraCuu,r.KhoaTraCuu,ct);ApiError.Require(row.TrangThai!="DA_DONG","Yêu cầu đã đóng. Vui lòng tạo yêu cầu mới.",409);
        row.TrangThai="DANG_XU_LY";row.UpdatedAt=BusinessClock.Now;db.TinNhanTuVan.Add(new(){TuVanId=row.Id,NoiDung=r.NoiDung.Trim(),TuNhanVien=false,GuiLuc=BusinessClock.Now});await db.SaveChangesAsync(ct);return NoContent();
    }
    [Authorize,HttpGet("toi")]
    public async Task<object> Mine(CancellationToken ct){var member=User.MemberId();ApiError.Require(member.HasValue,"Tài khoản không liên kết thành viên.",403);return await db.TuVan.AsNoTracking().Where(x=>x.ThanhVienId==member).OrderByDescending(x=>x.GuiLuc).Select(x=>new{x.Id,x.MaTraCuu,x.ChuDe,x.TrangThai,x.GuiLuc}).Take(100).ToListAsync(ct);}
    [Authorize,HttpGet("toi/{id:long}")]
    public async Task<object> Own(long id,CancellationToken ct){var row=await db.TuVan.SingleOrDefaultAsync(x=>x.Id==id&&x.ThanhVienId==User.MemberId()&&x.ThanhVienId!=null,ct)??throw new ApiError(404,"Không có yêu cầu.");return await Detail(row,ct);}
    [Authorize,HttpPost("toi/{id:long}/bo-sung")]
    public async Task<IActionResult> OwnFollowup(long id,ConsultationMessage r,CancellationToken ct){var row=await db.TuVan.SingleOrDefaultAsync(x=>x.Id==id&&x.ThanhVienId==User.MemberId()&&x.ThanhVienId!=null,ct)??throw new ApiError(404,"Không có yêu cầu.");ApiError.Require(row.TrangThai!="DA_DONG"&&Convert.ToBase64String(row.RowVersion)==r.RowVersion,"Yêu cầu đã thay đổi hoặc đã đóng.",409);row.TrangThai="DANG_XU_LY";db.TinNhanTuVan.Add(new(){TuVanId=id,NoiDung=r.NoiDung.Trim(),TuNhanVien=false,TaiKhoanId=User.AccountId(),GuiLuc=BusinessClock.Now});await db.SaveChangesAsync(ct);return NoContent();}
    [Authorize(Policy="HoSo"),HttpGet]
    public async Task<object> List(string? trangThai,int trang=1,int kichThuoc=20,CancellationToken ct=default){
        ApiError.Require(trang>0&&kichThuoc is >0 and <=100,"Phân trang không hợp lệ.");var q=db.TuVan.AsNoTracking();if(!string.IsNullOrWhiteSpace(trangThai))q=q.Where(x=>x.TrangThai==trangThai);
        return new {TongSo=await q.CountAsync(ct),DuLieu=await q.OrderByDescending(x=>x.GuiLuc).Skip((trang-1)*kichThuoc).Take(kichThuoc).Select(x=>new{x.Id,x.MaTraCuu,x.HoTen,x.Email,x.DienThoai,x.ChuDe,x.KhoaHocId,x.TrangThai,x.GuiLuc,x.DocLuc,x.NguoiXuLyId,x.RowVersion}).ToListAsync(ct)};
    }
    [Authorize(Policy="HoSo"),HttpGet("{id:long}")]
    public async Task<object> Read(long id,CancellationToken ct){var row=await db.TuVan.SingleOrDefaultAsync(x=>x.Id==id,ct)??throw new ApiError(404,"Không có yêu cầu.");if(row.DocLuc==null){row.DocLuc=BusinessClock.Now;if(row.TrangThai=="MOI_GUI")row.TrangThai="DA_DOC";row.NguoiXuLyId=User.AccountId();audit.Add("TuVan",id,"DOC_TU_VAN");await db.SaveChangesAsync(ct);}return await Detail(row,ct);}
    [Authorize(Policy="HoSo"),HttpPost("{id:long}/tra-loi")]
    public async Task<IActionResult> Reply(long id,ConsultationMessage r,CancellationToken ct){var row=await db.TuVan.SingleOrDefaultAsync(x=>x.Id==id,ct)??throw new ApiError(404,"Không có yêu cầu.");ApiError.Require(row.TrangThai!="DA_DONG"&&Convert.ToBase64String(row.RowVersion)==r.RowVersion,"Yêu cầu đã thay đổi hoặc đã đóng.",409);row.TrangThai="DA_TRA_LOI";row.DocLuc??=BusinessClock.Now;row.NguoiXuLyId=User.AccountId();db.TinNhanTuVan.Add(new(){TuVanId=id,NoiDung=r.NoiDung.Trim(),TuNhanVien=true,TaiKhoanId=User.AccountId(),GuiLuc=BusinessClock.Now});audit.Add("TuVan",id,"TRA_LOI_TU_VAN");await db.SaveChangesAsync(ct);return NoContent();}
    [Authorize(Policy="HoSo"),HttpPut("{id:long}/trang-thai")]
    public async Task<IActionResult> State(long id,ConsultationState r,CancellationToken ct){ApiError.Require(r.TrangThai is "DANG_XU_LY" or "DA_DONG","Trạng thái không hợp lệ.");var row=await db.TuVan.SingleOrDefaultAsync(x=>x.Id==id,ct)??throw new ApiError(404,"Không có yêu cầu.");ApiError.Require(Convert.ToBase64String(row.RowVersion)==r.RowVersion,"Yêu cầu đã thay đổi.",409);row.TrangThai=r.TrangThai;row.NguoiXuLyId=User.AccountId();audit.Add("TuVan",id,"DOI_TRANG_THAI_TU_VAN");await db.SaveChangesAsync(ct);return NoContent();}
}
