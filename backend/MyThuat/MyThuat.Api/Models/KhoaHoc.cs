namespace MyThuat.Api.Models;

// Khóa học gộp thông tin phiên bản và chương trình.
public sealed class KhoaHoc : EntityBase
{
    public long LoaiKhoaHocId { get; set; }
    public long? KhoaGocId { get; set; }
    public string MaKhoa { get; set; } = string.Empty;
    public string TenKhoa { get; set; } = string.Empty;
    public int PhienBan { get; set; }
    public string CapDo { get; set; } = string.Empty;
    public short? TuoiToiThieu { get; set; }
    public string? YeuCauDauVao { get; set; }
    public string MucTieu { get; set; } = string.Empty;
    public short SoBuoi { get; set; }
    public short SoTietMoiBuoi { get; set; }
    public short PhutMoiTiet { get; set; }
    public decimal HocPhi { get; set; }
    public DateOnly HieuLucTu { get; set; }
    public string TrangThai { get; set; } = string.Empty;

    public LoaiKhoaHoc? LoaiKhoaHoc { get; set; }
    public KhoaHoc? KhoaGoc { get; set; }
}
