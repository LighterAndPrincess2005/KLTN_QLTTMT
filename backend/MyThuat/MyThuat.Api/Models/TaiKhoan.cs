namespace MyThuat.Api.Models;

// Gộp tài khoản, vai trò và thông tin nhân viên nội bộ cơ bản.
public sealed class TaiKhoan : EntityBase
{
    public long? GiaoVienId { get; set; }
    public long? ThanhVienId { get; set; }
    public string TenDangNhap { get; set; } = string.Empty;
    public string PasswordHash { get; set; } = string.Empty;
    public string? HoTenNhanVien { get; set; }
    public string? DienThoaiNhanVien { get; set; }
    public string? Email { get; set; }
    public string VaiTroChinh { get; set; } = string.Empty;
    public long QuyenBoSung { get; set; }
    public DateTime? EmailXacNhanLuc { get; set; }
    public string? TokenXacNhanHash { get; set; }
    public DateTime? TokenXacNhanHetHan { get; set; }
    public DateTime? KhoaDenLuc { get; set; }
    public string TrangThai { get; set; } = string.Empty;
    public int PhienBanBaoMat { get; set; }

    public GiaoVien? GiaoVien { get; set; }
    public ThanhVien? ThanhVien { get; set; }
}
