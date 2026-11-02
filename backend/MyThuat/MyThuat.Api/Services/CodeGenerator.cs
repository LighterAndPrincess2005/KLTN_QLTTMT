using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;

namespace MyThuat.Api.Services;

// Use one sequence for both WinForms and web. Call inside the caller's serializable transaction.
public static class CodeGenerator
{
    public static async Task<string> NextAsync<T>(IQueryable<T> rows, Expression<Func<T,string>> code,
        string prefix, CancellationToken ct)
    {
        var used = (await rows.Select(code).ToListAsync(ct))
            .Where(x => x.StartsWith(prefix, StringComparison.OrdinalIgnoreCase))
            .Select(x => x[prefix.Length..])
            .Where(x => x.Length > 0 && x.All(char.IsAsciiDigit))
            .Select(x => long.TryParse(x, out var number) ? number : 0)
            .DefaultIfEmpty(0).Max();
        if (used == long.MaxValue) throw new InvalidOperationException("Dãy mã đã hết giá trị.");
        return prefix + (used + 1).ToString("D4", System.Globalization.CultureInfo.InvariantCulture);
    }
}
