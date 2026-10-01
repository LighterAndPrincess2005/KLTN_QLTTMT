using System.Security.Claims;
using System.Text.Json;
using MyThuat.Api.Data;
using MyThuat.Api.Models;
using MyThuat.Api.Security;
namespace MyThuat.Api.Services;
public sealed class AuditService(MyThuatDbContext db, IHttpContextAccessor accessor)
{
    public void Add(string entity, long id, string action, object? details = null, long? actor = null)
    {
        var ctx = accessor.HttpContext;
        var account = actor ?? (ctx?.User.Identity?.IsAuthenticated == true ? ctx.User.AccountId() : (long?)null);
        if (account is null) return;
        // Callers pass an explicit safe summary, never the entire account/password/request body.
        db.NhatKyThayDoi.Add(new NhatKyThayDoi {
            TaiKhoanId = account.Value, LoaiDoiTuong = entity, IdDoiTuong = id, HanhDong = action,
            SauJson = details is null ? null : JsonSerializer.Serialize(details), ThoiDiem = BusinessClock.Now,
            CorrelationId = ctx?.TraceIdentifier ?? Guid.NewGuid().ToString("N"), Kenh = "API"
        });
    }
}
