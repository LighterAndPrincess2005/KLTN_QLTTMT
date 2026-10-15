using System.Text.Json;
using Microsoft.AspNetCore.DataProtection;
using MyThuat.Api.Models;
namespace MyThuat.Api.Security;
public sealed record AccessTicket(long Id, int Version, DateTimeOffset Expires);
public sealed class AccessTokens(IDataProtectionProvider provider)
{
    private readonly IDataProtector protector = provider.CreateProtector("MyThuat.AccessToken.v1");
    public string Issue(TaiKhoan account) => protector.Protect(JsonSerializer.Serialize(
        new AccessTicket(account.Id, account.PhienBanBaoMat, DateTimeOffset.UtcNow.AddMinutes(30))));
    public AccessTicket? Read(string token)
    {
        try
        {
            var value = JsonSerializer.Deserialize<AccessTicket>(protector.Unprotect(token));
            return value is not null && value.Expires > DateTimeOffset.UtcNow ? value : null;
        }
        catch (Exception e) when (e is System.Security.Cryptography.CryptographicException or JsonException or FormatException)
        { return null; }
    }
}
