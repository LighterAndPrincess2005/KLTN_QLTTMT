using System.Security.Claims;
namespace MyThuat.Api.Security;
[Flags]
public enum Permission : long
{
    None = 0, DaoTao = 1, HoSo = 2, ThuTien = 4, DiemDanh = 8,
    Kho = 16, BaoCao = 32, TaiKhoan = 64, NhatKy = 128
}
public static class Permissions
{
    public static readonly string[] Roles = ["QUAN_TRI", "HOC_VU", "THU_NGAN", "GIAO_VIEN", "QUAN_LY_KHO", "THANH_VIEN"];
    public static Permission ForRole(string role) => role switch
    {
        "QUAN_TRI" => (Permission)255,
        "HOC_VU" => Permission.DaoTao | Permission.HoSo | Permission.DiemDanh | Permission.BaoCao,
        "THU_NGAN" => Permission.ThuTien,
        "GIAO_VIEN" => Permission.DiemDanh,
        "QUAN_LY_KHO" => Permission.Kho,
        _ => Permission.None
    };
    public static long AccountId(this ClaimsPrincipal p) => long.Parse(p.FindFirstValue(ClaimTypes.NameIdentifier)!);
    public static long? MemberId(this ClaimsPrincipal p) => long.TryParse(p.FindFirstValue("thanhVienId"), out var n) ? n : null;
    public static long? TeacherId(this ClaimsPrincipal p) => long.TryParse(p.FindFirstValue("giaoVienId"), out var n) ? n : null;
    public static bool Has(this ClaimsPrincipal p, Permission permission) =>
        long.TryParse(p.FindFirstValue("permissions"), out var n) && (((Permission)n & permission) == permission);
}
