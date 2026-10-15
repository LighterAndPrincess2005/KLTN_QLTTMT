namespace MyThuat.Api.Models;

// Chính sách ưu đãi có hạn mức giữ và sử dụng.
public sealed class KhuyenMai : EntityBase
{
    public string MaKhuyenMai { get; set; } = string.Empty;
    public string TenKhuyenMai { get; set; } = string.Empty;
    public string NhomKhoanThu { get; set; } = string.Empty;
    public string LoaiGiam { get; set; } = string.Empty;
    public decimal GiaTriGiam { get; set; }
    public decimal? TranGiam { get; set; }
    public int TongLuot { get; set; }
    public short? SoNgayTuXacNhanTV { get; set; }
    public DateTime BatDau { get; set; }
    public DateTime KetThuc { get; set; }
    public string TrangThai { get; set; } = string.Empty;
    public long? KhoaApDungId { get; set; }

    public KhoaHoc? KhoaApDung { get; set; }
}
