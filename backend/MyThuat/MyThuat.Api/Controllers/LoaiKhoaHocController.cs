using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MyThuat.Api.Contracts;
using MyThuat.Api.Services;
namespace MyThuat.Api.Controllers;
[ApiController, AllowAnonymous]
[Route("api/loai-khoa-hoc")]
public sealed class LoaiKhoaHocController(DanhMucService service) : ControllerBase
{
    [HttpGet]
    public Task<List<LoaiKhoaHocDto>> LayDanhSach(CancellationToken ct) => service.LayLoai(ct);
}
