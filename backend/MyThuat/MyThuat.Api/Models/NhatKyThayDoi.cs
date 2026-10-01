namespace MyThuat.Api.Models;

// Truy vết hành động và trạng thái trước sau.
public sealed class NhatKyThayDoi : EntityBase
{
    public long TaiKhoanId { get; set; }
    public string LoaiDoiTuong { get; set; } = string.Empty;
    public long IdDoiTuong { get; set; }
    public string HanhDong { get; set; } = string.Empty;
    public string? TruocJson { get; set; }
    public string? SauJson { get; set; }
    public DateTime ThoiDiem { get; set; }
    public string CorrelationId { get; set; } = string.Empty;
    public string Kenh { get; set; } = string.Empty;

    public TaiKhoan? TaiKhoan { get; set; }
}
