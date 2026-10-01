using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MyThuat.Api.Contracts;
using MyThuat.Api.Data;
using MyThuat.Api.Security;
using MyThuat.Api.Services;
namespace MyThuat.Api.Controllers;
[ApiController,Authorize,Route("api/thay-doi")]
public sealed class ThayDoiController(ChangeService service,MyThuatDbContext db):ControllerBase
{
    [HttpPost]public Task<object> RequestChange(ChangeRequest r,CancellationToken ct)=>service.Request(r,ct);
    [HttpGet]public async Task<object> List(CancellationToken ct)
    {
        var q=db.YeuCauThayDoi.AsNoTracking();if(!User.Has(Permission.DaoTao))q=q.Where(x=>x.NguoiYeuCauId==User.MemberId());
        return (await q.OrderByDescending(x=>x.GuiLuc).Take(500).ToListAsync(ct)).Select(ManagementService.Scalars);
    }
    [Authorize(Policy="DaoTao"),HttpPost("{id:long:min(1)}/duyet")]
    public Task<object> Approve(long id,ChangeDecision r,CancellationToken ct)=>service.Approve(id,r,ct);
    [Authorize(Policy="DaoTao"),HttpPost("{id:long:min(1)}/hoan-tat-chuyen")]
    public Task<object> FinalizeTransfer(long id,CancellationToken ct)=>service.FinalizeTransfer(id,ct);
}
