namespace MyThuat.Api.Models;

// Gom chuỗi đăng ký khi chuyển lớp, giữ nguyên quyền lợi.
public sealed class HoSoTheoHoc : EntityBase
{
    public long HocVienId { get; set; }
    public long KhoaHocId { get; set; }
    public long QuyDinhId { get; set; }
    public DateOnly NgayBatDau { get; set; }
    public string TrangThai { get; set; } = string.Empty;

    public HocVien? HocVien { get; set; }
    public KhoaHoc? KhoaHoc { get; set; }
    public QuyDinh? QuyDinh { get; set; }
}
