namespace MyThuat.Api.Models;

// Nhu cầu vật tư theo người hoặc buổi.
public sealed class DinhMucHoaCu : EntityBase
{
    public long NoiDungBuoiId { get; set; }
    public long HoaCuId { get; set; }
    public string CoSoTinh { get; set; } = string.Empty;
    public decimal SoLuong { get; set; }

    public NoiDungBuoi? NoiDungBuoi { get; set; }
    public HoaCu? HoaCu { get; set; }
}
