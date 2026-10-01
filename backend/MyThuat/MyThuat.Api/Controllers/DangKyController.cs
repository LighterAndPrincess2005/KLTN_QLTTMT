using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MyThuat.Api.Contracts;
using MyThuat.Api.Services;
namespace MyThuat.Api.Controllers;
[ApiController,Authorize,Route("api/dang-ky")]
public sealed class DangKyController(EnrollmentService service,FinanceService finance):ControllerBase
{
    [HttpPost]public Task<object> Create(EnrollRequest r,CancellationToken ct)=>service.Create(r,ct);
    [HttpGet("{id:long:min(1)}")]public Task<object> Detail(long id,CancellationToken ct)=>service.Detail(id,ct);
    [HttpGet("{id:long:min(1)}/cong-no")]public Task<object> Debt(long id,CancellationToken ct)=>finance.Debt(id,ct);
}
