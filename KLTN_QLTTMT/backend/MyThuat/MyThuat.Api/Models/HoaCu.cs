namespace MyThuat.Api.Models;

// Mã SKU riêng cho quy cách, màu và đơn vị cơ sở.
public sealed class HoaCu : EntityBase
{
    public string MaHoaCu { get; set; } = string.Empty;
    public string TenHoaCu { get; set; } = string.Empty;
    public string QuyCach { get; set; } = string.Empty;
    public string? MauSac { get; set; }
    public string DonViCoSo { get; set; } = string.Empty;
    public string NhomHoaCu { get; set; } = string.Empty;
    public decimal GiaBanThamKhao { get; set; }
    public decimal MucDatHang { get; set; }
    public string? CanhBaoAnToan { get; set; }
    public string TrangThai { get; set; } = string.Empty;


}
