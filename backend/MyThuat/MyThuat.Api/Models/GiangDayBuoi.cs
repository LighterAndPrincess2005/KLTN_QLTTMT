namespace MyThuat.Api.Models;

// Giữ riêng người thực dạy từng buổi để xử lý thay giáo viên.
public sealed class GiangDayBuoi : EntityBase
{
    public long BuoiHocId { get; set; }
    public long GiaoVienId { get; set; }
    public string VaiTro { get; set; } = string.Empty;
    public short SoTietThucDay { get; set; }
    public bool LaThayThe { get; set; }
    public string? LyDoThayThe { get; set; }
    public long NguoiDuyet { get; set; }

    public BuoiHoc? BuoiHoc { get; set; }
    public GiaoVien? GiaoVien { get; set; }
    public TaiKhoan? NguoiDuyetTaiKhoan { get; set; }
}
