using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MyThuat.Api.Contracts;
using MyThuat.Api.Data;
using MyThuat.Api.Services;
namespace MyThuat.Api.Controllers;
[ApiController,Authorize,Route("api/hoc-tap")]
public sealed class HocTapController(LearningService service,MyThuatDbContext db,AccessService access):ControllerBase
{
    [HttpGet("buoi/{id:long:min(1)}/danh-sach")]public Task<object> Roster(long id,CancellationToken ct)=>service.Roster(id,ct);
    [HttpPut("diem-danh/{id:long:min(1)}")]public Task<object> Attend(long id,AttendRequest r,CancellationToken ct)=>service.Attend(id,r,ct);
    [HttpPost("diem-danh/{id:long:min(1)}/bao-nghi")]public Task<object> Absence(long id,AbsenceRequest r,CancellationToken ct)=>service.Absence(id,r,ct);
    [HttpPost("hoc-bu")]public Task<object> Makeup(MakeupRequest r,CancellationToken ct)=>service.Makeup(r,ct);
    [HttpPost("buoi/{id:long:min(1)}/hoan-thanh")]public async Task<IActionResult> Close(long id,CancellationToken ct){await service.CloseSession(id,ct);return NoContent();}
    [HttpPost("ket-qua/de-xuat")]public Task<object> ProposeResult(ResultRequest r,CancellationToken ct)=>service.ProposeResult(r,ct);
    [HttpPost("ket-qua")]public Task<object> Result(ResultRequest r,CancellationToken ct)=>service.Result(r,ct);
    [HttpGet("ho-so/{id:long:min(1)}/tien-do")]
    public async Task<object> Progress(long id,CancellationToken ct)
    {
        var profile=await db.HoSoTheoHoc.AsNoTracking().SingleOrDefaultAsync(x=>x.Id==id,ct)??throw new ApiError(404,"Không có hồ sơ.");
        await access.Student(profile.HocVienId,false,ct);
        var rows=await db.DiemDanh.AsNoTracking().Where(x=>x.DangKy!.HoSoTheoHocId==id).OrderBy(x=>x.BuoiHoc!.BatDau)
            .Select(x=>new {x.Id,x.BuoiHocId,x.NoiDungDuocTinhId,x.LoaiThamGia,x.TrangThai,x.NhanXet,x.DiemSanPham,x.SanPhamUrl,NgayHoc=x.BuoiHoc!.BatDau}).ToListAsync(ct);
        var result=await db.KetQuaKhoaHoc.AsNoTracking().SingleOrDefaultAsync(x=>x.HoSoTheoHocId==id,ct);
        return new {HoSoTheoHocId=id,SoNoiDungDaThamGia=rows.Where(x=>x.TrangThai=="CO_MAT").Select(x=>x.NoiDungDuocTinhId).Distinct().Count(),DiemDanh=rows,
            KetQua=result is null?null:ManagementService.Scalars(result)};
    }
}
