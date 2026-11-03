using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MyThuat.Api.Data;
using MyThuat.Api.Models;
using MyThuat.Api.Services;
namespace MyThuat.Api.Controllers;
[ApiController,Authorize(Policy="Kho"),Route("api/quan-ly/dinh-muc")]
public sealed class QuanLyDinhMucHoaCuController(ManagementService service,MyThuatDbContext db):ManagementControllerBase<DinhMucHoaCu>(service)
{
    [HttpGet("noi-dung")]
    public async Task<object> Contents(CancellationToken ct) => await db.NoiDungBuoi.AsNoTracking()
        .OrderBy(x=>x.KhoaHoc!.TenKhoa).ThenBy(x=>x.ThuTu)
        .Select(x=>new {x.Id,x.KhoaHoc!.TenKhoa,x.KhoaHoc.PhienBan,x.ThuTu,x.ChuDe}).ToListAsync(ct);
}
