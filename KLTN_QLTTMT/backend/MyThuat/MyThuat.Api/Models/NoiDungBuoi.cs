namespace MyThuat.Api.Models;

// Đề cương chi tiết từng buổi trong khóa.
public sealed class NoiDungBuoi : EntityBase
{
    public long KhoaHocId { get; set; }
    public short ThuTu { get; set; }
    public string ChuDe { get; set; } = string.Empty;
    public string NoiDung { get; set; } = string.Empty;
    public short SoTiet { get; set; }
    public string? YeuCauSanPham { get; set; }

    public KhoaHoc? KhoaHoc { get; set; }
}
