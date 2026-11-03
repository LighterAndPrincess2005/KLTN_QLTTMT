using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;
using MyThuat.Api.Data;
using MyThuat.Api.Models;
using System.Text.RegularExpressions;

namespace MyThuat.Api.Services;

// Persistent high-water marks shared by WinForms and web, independent of deleted profiles.
public static class CodeGenerator
{
    public static async Task<string> NextAsync<T>(MyThuatDbContext db,IQueryable<T> rows, Expression<Func<T,string>> code,
        string prefix, CancellationToken ct)
    {
        if(db.Database.CurrentTransaction is null)throw new InvalidOperationException("Cấp mã cần transaction.");
        if(!Regex.IsMatch(prefix,@"\A[A-Z][A-Z0-9-]{1,11}\z"))throw new InvalidOperationException("Tiền tố mã không hợp lệ.");
        var counter=db.Database.IsSqlServer()
            ?await db.BoDemMa.FromSqlInterpolated($"SELECT * FROM dbo.BoDemMa WITH (UPDLOCK,HOLDLOCK) WHERE TienTo={prefix}").SingleOrDefaultAsync(ct)
            :await db.BoDemMa.SingleOrDefaultAsync(x=>x.TienTo==prefix,ct);
        if(counter is null){
            var used = (await rows.Select(code).ToListAsync(ct))
            .Where(x => x.StartsWith(prefix, StringComparison.OrdinalIgnoreCase))
            .Select(x => x[prefix.Length..])
            .Where(x => x.Length > 0 && x.All(char.IsAsciiDigit))
            .Select(x => long.TryParse(x, out var number) ? number : 0)
            .DefaultIfEmpty(0).Max();
            counter=new BoDemMa{TienTo=prefix,GiaTriCuoi=used};db.BoDemMa.Add(counter);
        }
        if(counter.GiaTriCuoi==long.MaxValue)throw new InvalidOperationException("Dãy mã đã hết giá trị.");
        counter.GiaTriCuoi++;
        await db.SaveChangesAsync(ct);
        return prefix+counter.GiaTriCuoi.ToString("D4",System.Globalization.CultureInfo.InvariantCulture);
    }
    public static string CoursePrefix(string value)
    {
        var prefix=value.Trim().ToUpperInvariant();
        ApiError.Require(Regex.IsMatch(prefix,@"\A[A-Z]{3,4}\z"),"Tiền tố mã khóa cần 3–4 chữ cái A–Z.");
        return prefix;
    }
    public static string ClassPrefix(KhoaHoc course)
    {
        var match=Regex.Match(course.MaKhoa,@"\A([A-Z]{3,4})-\d+\z",RegexOptions.IgnoreCase);
        if(match.Success)return match.Groups[1].Value.ToUpperInvariant()+"-L";
        return course.CapDo.ToUpperInvariant() switch {
            "JUNIOR"=>"JUN-L","FOUNDATION"=>"FND-L","PRE_BASIC"=>"PRE-L","BASIC"=>"BAS-L",
            "INTERMEDIATE"=>"INT-L","ADVANCED"=>"ADV-L",_=>"LOP-"};
    }
}
