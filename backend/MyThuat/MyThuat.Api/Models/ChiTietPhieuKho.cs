namespace MyThuat.Api.Models;

// Gộp chi tiết lô, nhận mua, cấp phát và kiểm kê vào dòng chứng từ.
public sealed class ChiTietPhieuKho : EntityBase
{
    public long PhieuKhoId { get; set; }
    public long HoaCuId { get; set; }
    public long? ChiTietThamChieuId { get; set; }
    public long? KhoanThuDangKyId { get; set; }
    public string? MaLo { get; set; }
    public DateOnly? HanSuDung { get; set; }
    public decimal SoLuong { get; set; }
    public decimal DonGiaChot { get; set; }
    public decimal? SoLuongSoSachTaiKho { get; set; }
    public decimal? SoLuongThucTeTaiKho { get; set; }
    public decimal? SoLuongSoSachDangMuon { get; set; }
    public decimal? SoLuongThucTeDangMuon { get; set; }
    public string? GhiChu { get; set; }

    public PhieuKho? PhieuKho { get; set; }
    public HoaCu? HoaCu { get; set; }
    public ChiTietPhieuKho? ChiTietThamChieu { get; set; }
    public KhoanThuDangKy? KhoanThuDangKy { get; set; }
}
