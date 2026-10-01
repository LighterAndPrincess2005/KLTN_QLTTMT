using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MyThuat.Api.Contracts;
using MyThuat.Api.Data;
using MyThuat.Api.Services;
namespace MyThuat.Api.Controllers;
[ApiController,Authorize(Policy="ThuTien"),Route("api/giao-dich")]
public sealed class GiaoDichController(FinanceService service,MyThuatDbContext db):ControllerBase
{
    [HttpPost("thu")]public Task<object> Receive(ReceiptRequest r,CancellationToken ct)=>service.Receive(r,ct);
    [HttpPost("hoan")]public Task<object> Refund(RefundRequest r,CancellationToken ct)=>service.Refund(r,ct);
    [HttpGet("cho-doi-chieu")]public async Task<object> Pending(CancellationToken ct)=>
        (await db.GiaoDichTien.AsNoTracking().Where(x=>x.TrangThai=="CHO_DOI_CHIEU").OrderBy(x=>x.ThoiDiem).Take(500).ToListAsync(ct)).Select(ManagementService.Scalars);
}
