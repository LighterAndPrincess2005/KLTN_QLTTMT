namespace MyThuat.Api.Models;

public sealed class DanhGiaKhoaHoc : EntityBase
{
    public long HoSoTheoHocId { get; set; }
    public long NguoiGuiId { get; set; }
    public int SoSao { get; set; }
    public string NoiDung { get; set; } = "";
    public string? TenNguoiDangCongKhai { get; set; }
    public bool DongYCongKhai { get; set; }
    public string TrangThai { get; set; } = "CHO_DUYET";
    public byte[]? Anh { get; set; }
    public string? LoaiAnh { get; set; }
    public string? GhiChuDuyet { get; set; }
}
