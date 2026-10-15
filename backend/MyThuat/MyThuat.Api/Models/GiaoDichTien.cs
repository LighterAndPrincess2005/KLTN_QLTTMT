namespace MyThuat.Api.Models;

// Tiền thực thu hoặc hoàn; gộp phân bổ vào từng dòng giao dịch.
public sealed class GiaoDichTien : EntityBase
{
    public long DangKyId { get; set; }
    public long? KhoanThuId { get; set; }
    public long? GiaoDichThuGocId { get; set; }
    public long? YeuCauThayDoiId { get; set; }
    public string? MaChungTu { get; set; }
    public string? MaThamChieu { get; set; }
    public string LoaiGiaoDich { get; set; } = string.Empty;
    public string PhuongThuc { get; set; } = string.Empty;
    public decimal SoTien { get; set; }
    public DateTime ThoiDiem { get; set; }
    public long? NguoiXacNhan { get; set; }
    public string TrangThai { get; set; } = string.Empty;
    public string RequestFingerprint { get; set; } = string.Empty;
    public string IdempotencyKey { get; set; } = string.Empty;

    public DangKy? DangKy { get; set; }
    public KhoanThuDangKy? KhoanThu { get; set; }
    public GiaoDichTien? GiaoDichThuGoc { get; set; }
    public YeuCauThayDoi? YeuCauThayDoi { get; set; }
    public TaiKhoan? NguoiXacNhanTaiKhoan { get; set; }
}
