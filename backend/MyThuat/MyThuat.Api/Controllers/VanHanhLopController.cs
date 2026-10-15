using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MyThuat.Api.Contracts;
using MyThuat.Api.Services;
namespace MyThuat.Api.Controllers;
[ApiController,Authorize(Policy="DaoTao"),Route("api/van-hanh-lop")]
public sealed class VanHanhLopController(ClassLifecycleService service):ControllerBase
{
    [HttpPost("{id:long:min(1)}/hoan")]public Task<object> Postpone(long id,PostponeClassRequest r,CancellationToken ct)=>service.Postpone(id,r,ct);
    [HttpPost("{id:long:min(1)}/huy")]public Task<object> Cancel(long id,CancelClassRequest r,CancellationToken ct)=>service.Cancel(id,r,ct);
}
