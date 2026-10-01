using Microsoft.AspNetCore.Authorization;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc;
using MyThuat.Api.Contracts;
using MyThuat.Api.Services;
namespace MyThuat.Api.Controllers;

[ApiController, AllowAnonymous]
[Route("api/khoa-hoc")]
public sealed class KhoaHocController(DanhMucService service) : ControllerBase
{
    [HttpGet]
    public Task<TrangKetQua<KhoaHocDto>> LayDanhSach(
        [Range(1, 1000000)] int trang = 1, [Range(1, 100)] int kichThuoc = 20,
        [Range(1, long.MaxValue)] long? loaiId = null, [StringLength(200)] string? tuKhoa = null,
        CancellationToken ct = default) => service.LayKhoa(trang, kichThuoc, loaiId, tuKhoa, ct);

    [HttpGet("{id:long:min(1)}")]
    public async Task<ActionResult<KhoaHocDto>> LayChiTiet(long id, CancellationToken ct)
    {
        var result = await service.LayKhoa(id, ct);
        return result is null ? NotFound() : Ok(result);
    }

    [HttpGet("{id:long:min(1)}/noi-dung")]
    public async Task<ActionResult<List<NoiDungBuoiDto>>> LayNoiDung(long id, CancellationToken ct)
    {
        var result = await service.LayNoiDung(id, ct);
        return result is null ? NotFound() : Ok(result);
    }
}
