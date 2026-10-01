namespace MyThuat.Api.Models;

// Buổi học theo ngày giờ, gộp cấu trúc nội dung lớp và lịch thực tế.
public sealed class BuoiHoc : EntityBase
{
    public long LopHocId { get; set; }
    public long NoiDungBuoiId { get; set; }
    public long PhongHocId { get; set; }
    public long? BuoiThayTheChoId { get; set; }
    public short ThuTuTrongLop { get; set; }
    public short LanXepLich { get; set; }
    public DateTime BatDau { get; set; }
    public DateTime KetThuc { get; set; }
    public short SoTiet { get; set; }
    public short PhutMoiTiet { get; set; }
    public short PhutNghi { get; set; }
    public string? LyDoDoiLich { get; set; }
    public string TrangThai { get; set; } = string.Empty;

    public LopHoc? LopHoc { get; set; }
    public NoiDungBuoi? NoiDungBuoi { get; set; }
    public PhongHoc? PhongHoc { get; set; }
    public BuoiHoc? BuoiThayTheCho { get; set; }
}
