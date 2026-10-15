using System.Security.Claims;
using System.Text.Encodings.Web;
using Microsoft.AspNetCore.Authentication;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using MyThuat.Api.Data;
using MyThuat.Api.Services;
namespace MyThuat.Api.Security;
public sealed class BearerAuthenticationHandler(IOptionsMonitor<AuthenticationSchemeOptions> options,
    ILoggerFactory logger, UrlEncoder encoder, AccessTokens tokens, MyThuatDbContext db)
    : AuthenticationHandler<AuthenticationSchemeOptions>(options, logger, encoder)
{
    protected override async Task<AuthenticateResult> HandleAuthenticateAsync()
    {
        var header = Request.Headers.Authorization.ToString();
        if (header.Length == 0) return AuthenticateResult.NoResult();
        if (!header.StartsWith("Bearer ", StringComparison.OrdinalIgnoreCase)) return AuthenticateResult.Fail("Invalid bearer token");
        var ticket = tokens.Read(header[7..].Trim());
        if (ticket is null) return AuthenticateResult.Fail("Invalid bearer token");
        var account = await db.TaiKhoan.AsNoTracking().SingleOrDefaultAsync(x => x.Id == ticket.Id, Context.RequestAborted);
        if (account is null || account.TrangThai != "HOAT_DONG" || account.PhienBanBaoMat != ticket.Version
            || account.KhoaDenLuc > BusinessClock.Now) return AuthenticateResult.Fail("Account inactive");
        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, account.Id.ToString()), new(ClaimTypes.Name, account.TenDangNhap),
            new(ClaimTypes.Role, account.VaiTroChinh),
            new("permissions", ((long)Permissions.ForRole(account.VaiTroChinh) | account.QuyenBoSung).ToString())
        };
        if (account.ThanhVienId is long member) claims.Add(new("thanhVienId", member.ToString()));
        if (account.GiaoVienId is long teacher) claims.Add(new("giaoVienId", teacher.ToString()));
        return AuthenticateResult.Success(new AuthenticationTicket(new ClaimsPrincipal(new ClaimsIdentity(claims, Scheme.Name)), Scheme.Name));
    }
}
