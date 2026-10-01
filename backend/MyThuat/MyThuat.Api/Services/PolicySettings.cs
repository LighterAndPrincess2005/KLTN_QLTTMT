using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using MyThuat.Api.Data;
using MyThuat.Api.Models;
namespace MyThuat.Api.Services;
public sealed record PolicySettings
{
    public int HanGiuChoGio { get; init; } = 24;
    public int DongDangKyTruocGio { get; init; } = 48;
    public int BaoNghiTruocGio { get; init; } = 12;
    public int SoLuotHocBu { get; init; } = 2;
    public int HanHocBuNgay { get; init; } = 30;
    public int SoLanChuyen { get; init; } = 1;
    public decimal TyLeBuoiDat { get; init; } = 0.8m;
    public decimal DiemDat { get; init; } = 5;
    public int NghiGiuaCaPhut { get; init; } = 15;
    public DateOnly[] NgayNghi { get; init; } = [];
    public static PolicySettings Parse(string json)
    {
        PolicySettings s;
        try { s=JsonSerializer.Deserialize<PolicySettings>(json,new JsonSerializerOptions {PropertyNameCaseInsensitive=true,UnmappedMemberHandling=System.Text.Json.Serialization.JsonUnmappedMemberHandling.Disallow})??throw new JsonException(); }
        catch(JsonException) { throw new ApiError(400,"Nội dung quy định không đúng cấu trúc JSON."); }
        ApiError.Require(s.NgayNghi is not null&&s.HanGiuChoGio is >0 and <=168 && s.DongDangKyTruocGio is >=0 and <=720
            && s.BaoNghiTruocGio is >=0 and <=168 && s.SoLuotHocBu is >=0 and <=100
            && s.HanHocBuNgay is >0 and <=365 && s.SoLanChuyen is >=0 and <=10
            && s.TyLeBuoiDat is >0 and <=1 && s.DiemDat is >=0 and <=10 && s.NghiGiuaCaPhut is >=0 and <=120,
            "Tham số quy định vượt giới hạn hợp lệ.");
        return s;
    }
}
public sealed class PolicyService(MyThuatDbContext db)
{
    public async Task<QuyDinh> Current(CancellationToken ct)
    {
        var now=BusinessClock.Now;
        return await db.QuyDinh.Where(x=>x.TrangThai=="HOAT_DONG"&&x.HieuLucTu<=now&&(x.HieuLucDen==null||x.HieuLucDen>now))
            .OrderByDescending(x=>x.SoPhienBan).FirstOrDefaultAsync(ct)??throw new ApiError(409,"Chưa có quy định hiệu lực.");
    }
    public async Task<PolicySettings> ForProfile(long id,CancellationToken ct)
    {
        var profile=await db.HoSoTheoHoc.AsNoTracking().SingleAsync(x=>x.Id==id,ct);
        var rule=await db.QuyDinh.AsNoTracking().SingleAsync(x=>x.Id==profile.QuyDinhId,ct);
        return PolicySettings.Parse(rule.NoiDungJson);
    }
}
