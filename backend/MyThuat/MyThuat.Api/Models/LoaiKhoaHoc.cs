namespace MyThuat.Api.Models;

// Nhóm chương trình Core, bổ trợ, workshop.
public sealed class LoaiKhoaHoc : EntityBase
{
    public string MaLoai { get; set; } = string.Empty;
    public string TenLoai { get; set; } = string.Empty;
    public string? MoTa { get; set; }
    public string TrangThai { get; set; } = string.Empty;


}
