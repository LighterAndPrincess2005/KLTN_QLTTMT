namespace MyThuat.Api.Models;

// Hồ sơ người trực tiếp học, tồn tại dù không có tài khoản.
public sealed class HocVien : EntityBase
{
    public string MaHocVien { get; set; } = string.Empty;
    public string HoTen { get; set; } = string.Empty;
    public DateOnly NgaySinh { get; set; }
    public string? GioiTinh { get; set; }
    public string? DiaChi { get; set; }
    public string? MucTieuHoc { get; set; }
    public string? LuuYHoTroHocTap { get; set; }
    public string LienHeKhanCapTen { get; set; } = string.Empty;
    public string LienHeKhanCapSDT { get; set; } = string.Empty;
    public string TrangThai { get; set; } = string.Empty;
    public string? NhanXetDauVao { get; set; }
    public string? CapDoDeXuat { get; set; }
    public DateOnly? NgayDanhGiaDauVao { get; set; }
    public long? GiaoVienDanhGia { get; set; }

    public GiaoVien? GiaoVienDanhGiaHoSo { get; set; }
}
