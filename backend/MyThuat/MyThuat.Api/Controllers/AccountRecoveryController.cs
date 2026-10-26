using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using MyThuat.Api.Security;
using MyThuat.Api.Services;
namespace MyThuat.Api.Controllers;

public sealed record ForgotPasswordRequest([Required,MaxLength(100)]string TenDangNhap);
public sealed record RecoveryResetRequest([Required,StringLength(32,MinimumLength=32)]string MaYeuCau,[Required,RegularExpression(@"^\d{6}$",ErrorMessage="Mã khôi phục gồm 6 chữ số.")]string Ma,[Required,StringLength(128,MinimumLength=8)]string MatKhauMoi);

[ApiController,Route("api/tai-khoan"),EnableRateLimiting("recovery")]
public sealed class AccountRecoveryController(AccountRecoveryService service):ControllerBase
{
    [AllowAnonymous,HttpPost("quen-mat-khau")]
    public Task<object> Forgot(ForgotPasswordRequest r,CancellationToken ct)=>service.RequestReset(r.TenDangNhap,ct);
    [AllowAnonymous,HttpPost("dat-lai-mat-khau")]
    public async Task<IActionResult> Reset(RecoveryResetRequest r,CancellationToken ct){await service.Complete(r.MaYeuCau,r.Ma,r.MatKhauMoi,ct);return NoContent();}
}
