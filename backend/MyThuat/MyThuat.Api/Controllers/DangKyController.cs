using Microsoft.EntityFrameworkCore;
using MyThuat.Api.Data;
using MyThuat.Api.Security;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MyThuat.Api.Contracts;
using MyThuat.Api.Services;
namespace MyThuat.Api.Controllers;
[ApiController,Authorize,Route("api/dang-ky")]
public sealed class DangKyController(EnrollmentService service,FinanceService finance,MyThuatDbContext db):ControllerBase
{
    [HttpGet("toi")]
    public async Task<object> Mine(CancellationToken ct)
    {
        var member=User.MemberId();ApiError.Require(member.HasValue,"Tài khoản không liên kết thành viên.",403);
        var now=BusinessClock.Now;
        return await db.DangKy.AsNoTracking().Where(x=>db.DaiDienHocVien.Any(d=>d.ThanhVienId==member
            &&d.HocVienId==x.HoSoTheoHoc!.HocVienId&&d.XacNhanLuc!=null&&d.HieuLucTu<=now&&(d.HieuLucDen==null||d.HieuLucDen>now)))
            .OrderByDescending(x=>x.LapLuc).Select(x=>new {x.Id,x.MaDangKy,x.HoSoTheoHocId,x.LopHocId,x.TrangThai,x.HanGiuCho,
                HocVien=x.HoSoTheoHoc!.HocVien!.HoTen,TenKhoa=x.HoSoTheoHoc.KhoaHoc!.TenKhoa,
                SoBuoi=x.HoSoTheoHoc.KhoaHoc.SoBuoi,TenLop=x.LopHoc!.TenLop,LichHoc=x.LopHoc.LichHocDuKien}).Take(500).ToListAsync(ct);
    }
    [HttpPost]public Task<object> Create(EnrollRequest r,CancellationToken ct)=>service.Create(r,ct);
    [HttpGet("{id:long:min(1)}")]public Task<object> Detail(long id,CancellationToken ct)=>service.Detail(id,ct);
    [HttpGet("{id:long:min(1)}/cong-no")]public Task<object> Debt(long id,CancellationToken ct)=>finance.Debt(id,ct);
}
