namespace MyThuat.Api.Models;

// Hồ sơ cá nhân, liên hệ, trình độ, chuyên môn và cấp độ có thể dạy.
public sealed class GiaoVien : EntityBase
{
    public string MaGiaoVien { get; set; } = string.Empty;
    public string HoTen { get; set; } = string.Empty;
    public DateOnly? NgaySinh { get; set; }
    public string? GioiTinh { get; set; }
    public string DienThoai { get; set; } = string.Empty;
    public string? Email { get; set; }
    public string? DiaChi { get; set; }
    public string TrinhDo { get; set; } = string.Empty;
    public string ChuyenMon { get; set; } = string.Empty;
    public string CapDoCoTheDay { get; set; } = string.Empty;
    public string TrangThai { get; set; } = string.Empty;


}
