namespace MyThuat.Api.Models;

// Ưu đãi chốt và vòng đời lượt theo đăng ký.
public sealed class ApDungUuDai : EntityBase
{
    public long DangKyId { get; set; }
    public long KhuyenMaiId { get; set; }
    public string NhomKhoanThu { get; set; } = string.Empty;
    public decimal GiaTriGiamChot { get; set; }
    public DateTime HanGiuLuot { get; set; }
    public string TrangThaiLuot { get; set; } = string.Empty;

    public DangKy? DangKy { get; set; }
    public KhuyenMai? KhuyenMai { get; set; }
}
