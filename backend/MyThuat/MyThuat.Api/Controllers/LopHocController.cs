using Microsoft.AspNetCore.Authorization;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc;
using MyThuat.Api.Contracts;
using MyThuat.Api.Services;
namespace MyThuat.Api.Controllers;

[ApiController, AllowAnonymous]
[Route("api/lop-hoc")]
public sealed class LopHocController(DanhMucService service) : ControllerBase
{
    [HttpGet]
    public Task<TrangKetQua<LopHocDto>> LayDanhSach(
        [Range(1, 1000000)] int trang = 1, [Range(1, 100)] int kichThuoc = 20,
        [Range(1, long.MaxValue)] long? khoaId = null, CancellationToken ct = default)
        => service.LayLop(trang, kichThuoc, khoaId, ct);

    [HttpGet("{id:long:min(1)}")]
    public async Task<ActionResult<LopHocDto>> LayChiTiet(long id, CancellationToken ct)
    {
        var result = await service.LayLop(id, ct);
        return result is null ? NotFound() : Ok(result);
    }
}
