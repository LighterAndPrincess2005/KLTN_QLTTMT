namespace MyThuat.Api.Models;

// Một lớp mở thực tế, có tên và lịch khai giảng.
public sealed class LopHoc : EntityBase
{
    public long KhoaHocId { get; set; }
    public string MaLop { get; set; } = string.Empty;
    public string TenLop { get; set; } = string.Empty;
    public DateOnly NgayKhaiGiangDuKien { get; set; }
    public DateOnly? NgayKhaiGiangThucTe { get; set; }
    public DateOnly? NgayKetThucDuKien { get; set; }
    public short SoBuoiKeHoach { get; set; }
    public string LichHocDuKien { get; set; } = string.Empty;
    public short SiSoToiThieu { get; set; }
    public short SiSoToiDa { get; set; }
    public decimal HocPhiApDung { get; set; }
    public DateTime MoDangKyLuc { get; set; }
    public DateTime DongDangKyLuc { get; set; }
    public long? NguoiDuyet { get; set; }
    public string TrangThai { get; set; } = string.Empty;

    public KhoaHoc? KhoaHoc { get; set; }
    public TaiKhoan? NguoiDuyetTaiKhoan { get; set; }
}
