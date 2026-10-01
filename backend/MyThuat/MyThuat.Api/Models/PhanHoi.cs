namespace MyThuat.Api.Models;

// Phản hồi về lớp hoặc đăng ký với thời hạn xử lý.
public sealed class PhanHoi : EntityBase
{
    public long DangKyId { get; set; }
    public long NguoiGuiId { get; set; }
    public string NoiDung { get; set; } = string.Empty;
    public DateTime GuiLuc { get; set; }
    public long? NguoiXuLy { get; set; }
    public DateTime HanPhanHoi { get; set; }
    public string? TraLoi { get; set; }
    public DateTime? TraLoiLuc { get; set; }
    public string TrangThai { get; set; } = string.Empty;

    public DangKy? DangKy { get; set; }
    public ThanhVien? NguoiGui { get; set; }
    public TaiKhoan? NguoiXuLyTaiKhoan { get; set; }
}
