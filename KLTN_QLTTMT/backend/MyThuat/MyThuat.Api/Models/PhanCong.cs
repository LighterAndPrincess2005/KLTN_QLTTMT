namespace MyThuat.Api.Models;

// Giáo viên phụ trách lớp theo thời gian.
public sealed class PhanCong : EntityBase
{
    public long LopHocId { get; set; }
    public long GiaoVienId { get; set; }
    public string VaiTroTrongLop { get; set; } = string.Empty;
    public DateOnly TuNgay { get; set; }
    public DateOnly? DenNgay { get; set; }
    public bool XacNhanDuChuyenMon { get; set; }
    public DateTime? XacNhanNhanLopLuc { get; set; }
    public long NguoiDuyet { get; set; }

    public LopHoc? LopHoc { get; set; }
    public GiaoVien? GiaoVien { get; set; }
    public TaiKhoan? NguoiDuyetTaiKhoan { get; set; }
}
