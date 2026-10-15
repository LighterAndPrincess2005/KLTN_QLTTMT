namespace MyThuat.Api.Models;

// Một thành viên có thể đại diện nhiều học viên; mỗi học viên có thể nhiều đại diện.
public sealed class DaiDienHocVien : EntityBase
{
    public long ThanhVienId { get; set; }
    public long HocVienId { get; set; }
    public string QuanHe { get; set; } = string.Empty;
    public bool LaLienHeChinh { get; set; }
    public string QuyenDaiDien { get; set; } = string.Empty;
    public DateTime HieuLucTu { get; set; }
    public DateTime? HieuLucDen { get; set; }
    public DateTime? XacNhanLuc { get; set; }
    public long? NguoiXacNhan { get; set; }
    public bool DongYHinhAnh { get; set; }
    public bool DongYTacPham { get; set; }
    public DateTime? ThoiDiemDongY { get; set; }

    public ThanhVien? ThanhVien { get; set; }
    public HocVien? HocVien { get; set; }
    public TaiKhoan? NguoiXacNhanTaiKhoan { get; set; }
}
