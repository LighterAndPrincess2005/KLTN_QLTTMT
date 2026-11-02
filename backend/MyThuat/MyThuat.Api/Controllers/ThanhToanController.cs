using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MyThuat.Api.Data;
using MyThuat.Api.Models;
using MyThuat.Api.Services;
namespace MyThuat.Api.Controllers;
[ApiController,Authorize,Route("api/thanh-toan")]
public sealed class ThanhToanController(OnlinePaymentService service,MyThuatDbContext db):ControllerBase
{
    [HttpPost("{id:long:min(1)}/phien")]public Task<object> Checkout(long id,CancellationToken ct)=>service.Checkout(id,ct);
    [HttpGet("{id:long:min(1)}/trang-thai")]public Task<object> Status(long id,CancellationToken ct)=>service.Status(id,ct);
    [AllowAnonymous,HttpPost("sepay/webhook")]
    public Task<object> Webhook(SePayEvent r,CancellationToken ct){service.Authenticate(Request.Headers.Authorization.ToString());return service.Receive(r,ct);}
    [Authorize(Policy="ThuTien"),HttpGet("doi-soat")]
    public async Task<object> Reconcile(CancellationToken ct)=>await db.Set<SuKienSePay>().AsNoTracking().Where(x=>x.TrangThai=="CHO_DOI_CHIEU").OrderByDescending(x=>x.Id).Take(100).Select(x=>new{x.Id,x.MaGiaoDichSePay,x.DangKyId,x.SoTien,x.MaThamChieu,x.NoiDung,x.CreatedAt}).ToListAsync(ct);
}
