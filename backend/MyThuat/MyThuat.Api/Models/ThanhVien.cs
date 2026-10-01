namespace MyThuat.Api.Models;

// Người liên hệ hoặc người đại diện, không đồng nhất với học viên.
public sealed class ThanhVien : EntityBase
{
    public string MaThanhVien { get; set; } = string.Empty;
    public string HoTen { get; set; } = string.Empty;
    public string DienThoai { get; set; } = string.Empty;
    public string? Email { get; set; }
    public string? DiaChi { get; set; }
    public DateTime? XacNhanThanhVienLuc { get; set; }
    public string TrangThai { get; set; } = string.Empty;
    public string? NhuCauTuVan { get; set; }
    public string? KetQuaTuVan { get; set; }
    public DateTime? HanPhanHoiTuVan { get; set; }
    public long? NguoiTuVan { get; set; }
    public string? TrangThaiTuVan { get; set; }
    public bool DongYQuangBa { get; set; }
    public DateTime? ThoiDiemDongYQuangBa { get; set; }

    public TaiKhoan? NguoiTuVanTaiKhoan { get; set; }
}
