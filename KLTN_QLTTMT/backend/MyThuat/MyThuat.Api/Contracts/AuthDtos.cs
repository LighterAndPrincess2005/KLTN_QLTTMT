using System.ComponentModel.DataAnnotations;
namespace MyThuat.Api.Contracts;
public sealed record UpdateMemberRequest([Required,StringLength(150)]string HoTen,
    [Required,StringLength(25)]string DienThoai,[EmailAddress,StringLength(254)]string? Email,
    DateOnly? NgaySinh,[Required]string RowVersion);
public sealed record LoginRequest([Required, StringLength(100)] string TenDangNhap, [Required, StringLength(128)] string MatKhau);
public sealed record PasswordRequest([Required] string MatKhauCu, [Required, StringLength(128, MinimumLength=8)] string MatKhauMoi);
public sealed record AdminSetupRequest([Required, StringLength(100,MinimumLength=3)] string TenDangNhap,
    [Required, StringLength(128,MinimumLength=8)] string MatKhau, [Required, StringLength(150)] string HoTen);
public sealed record SignupRequest([Required, StringLength(100,MinimumLength=3)] string TenDangNhap,
    [Required, StringLength(128,MinimumLength=8)] string MatKhau, [Required, StringLength(150)] string HoTen,
    [Required, StringLength(25)] string DienThoai, [EmailAddress, StringLength(254)] string? Email, DateOnly? NgaySinh = null);
public sealed record StaffAccountRequest([Required, StringLength(100,MinimumLength=3)] string TenDangNhap,
    [Required, StringLength(128,MinimumLength=8)] string MatKhau, [Required, StringLength(30)] string VaiTro,
    [Required, StringLength(150)] string HoTen, long? GiaoVienId, long? ThanhVienId, long QuyenBoSung=0);
public sealed record AccountRoleRequest([Required] string VaiTro, long QuyenBoSung, bool HoatDong);
