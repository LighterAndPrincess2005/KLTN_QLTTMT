namespace MyThuat.Api.Models;

// Kết quả chốt trên toàn chuỗi theo học, không cấp trùng sau chuyển.
public sealed class KetQuaKhoaHoc : EntityBase
{
    public long HoSoTheoHocId { get; set; }
    public short SoBuoiHopLeChot { get; set; }
    public decimal DiemCuoiKhoaChot { get; set; }
    public string KetLuan { get; set; } = string.Empty;
    public string? NhanXetTongKet { get; set; }
    public long NguoiDuyet { get; set; }
    public DateTime ChotLuc { get; set; }
    public string? SoXacNhan { get; set; }
    public DateOnly? NgayCap { get; set; }
    public string? BaiCuoiKhoaUrl { get; set; }

    public HoSoTheoHoc? HoSoTheoHoc { get; set; }
    public TaiKhoan? NguoiDuyetTaiKhoan { get; set; }
}
