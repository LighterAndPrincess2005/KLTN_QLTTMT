namespace MyThuat.Api.Models;

// Tách học phí và họa cụ mua riêng, giữ giá chốt.
public sealed class KhoanThuDangKy : EntityBase
{
    public long DangKyId { get; set; }
    public string LoaiKhoan { get; set; } = string.Empty;
    public long? HoaCuId { get; set; }
    public string MoTaChot { get; set; } = string.Empty;
    public decimal SoLuong { get; set; }
    public decimal DonGiaChot { get; set; }
    public decimal TienGiamChot { get; set; }
    public decimal ThanhTienChot { get; set; }
    public decimal SoLuongDaGiao { get; set; }

    public DangKy? DangKy { get; set; }
    public HoaCu? HoaCu { get; set; }
}
