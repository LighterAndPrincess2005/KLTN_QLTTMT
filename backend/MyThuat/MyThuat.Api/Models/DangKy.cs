namespace MyThuat.Api.Models;

// Đăng ký một lớp cụ thể, giữ lịch sử nguồn và đích.
public sealed class DangKy : EntityBase
{
    public string MaDangKy { get; set; } = string.Empty;
    public long HoSoTheoHocId { get; set; }
    public long LopHocId { get; set; }
    public long NguoiDangKyId { get; set; }
    public long? DangKyNguonId { get; set; }
    public DateTime LapLuc { get; set; }
    public DateTime HanGiuCho { get; set; }
    public DateTime? HieuLucTu { get; set; }
    public DateTime? HieuLucDen { get; set; }
    public decimal HocPhiGocChot { get; set; }
    public string TrangThai { get; set; } = string.Empty;
    public string RequestFingerprint { get; set; } = string.Empty;
    public string IdempotencyKey { get; set; } = string.Empty;

    public HoSoTheoHoc? HoSoTheoHoc { get; set; }
    public LopHoc? LopHoc { get; set; }
    public ThanhVien? NguoiDangKy { get; set; }
    public DangKy? DangKyNguon { get; set; }
}
