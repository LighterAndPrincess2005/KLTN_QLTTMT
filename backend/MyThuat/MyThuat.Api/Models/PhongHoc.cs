namespace MyThuat.Api.Models;

// Phòng và sức chứa tại một cơ sở.
public sealed class PhongHoc : EntityBase
{
    public string MaPhong { get; set; } = string.Empty;
    public string TenPhong { get; set; } = string.Empty;
    public short SucChua { get; set; }
    public string TrangThai { get; set; } = string.Empty;


}
