namespace MyThuat.Api.Models;

// Chứng từ nhập, cấp, trả, bán, hỏng và điều chỉnh.
public sealed class PhieuKho : EntityBase
{
    public string MaPhieu { get; set; } = string.Empty;
    public string LoaiPhieu { get; set; } = string.Empty;
    public long? NhaCungCapId { get; set; }
    public long? BuoiHocId { get; set; }
    public long? PhieuThamChieuId { get; set; }
    public DateTime LapLuc { get; set; }
    public DateTime? DuyetLuc { get; set; }
    public string? LyDo { get; set; }
    public string TrangThai { get; set; } = string.Empty;
    public long NguoiLap { get; set; }
    public long? NguoiDuyet { get; set; }
    public long? NguoiNhan { get; set; }
    public string? TepBienBanUrl { get; set; }

    public NhaCungCap? NhaCungCap { get; set; }
    public BuoiHoc? BuoiHoc { get; set; }
    public PhieuKho? PhieuThamChieu { get; set; }
    public TaiKhoan? NguoiLapTaiKhoan { get; set; }
    public TaiKhoan? NguoiDuyetTaiKhoan { get; set; }
    public TaiKhoan? NguoiNhanTaiKhoan { get; set; }
}
