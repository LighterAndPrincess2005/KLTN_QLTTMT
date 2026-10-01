namespace MyThuat.Api.Models;

// Đơn vị cung cấp và liên hệ.
public sealed class NhaCungCap : EntityBase
{
    public string MaNCC { get; set; } = string.Empty;
    public string TenNCC { get; set; } = string.Empty;
    public string? NguoiLienHe { get; set; }
    public string DienThoai { get; set; } = string.Empty;
    public string? Email { get; set; }
    public string? DiaChi { get; set; }
    public string TrangThai { get; set; } = string.Empty;


}
