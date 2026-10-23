namespace MyThuat.Api.Models;

// Gộp chỗ dự kiến tham gia, điểm danh và bài vẽ từng buổi.
public sealed class DiemDanh : EntityBase
{
    public long DangKyId { get; set; }
    public long BuoiHocId { get; set; }
    public long NoiDungDuocTinhId { get; set; }
    public string LoaiThamGia { get; set; } = string.Empty;
    public string TrangThai { get; set; } = string.Empty;
    public DateTime? BaoNghiLuc { get; set; }
    public short? PhutThamDu { get; set; }
    public string? NhanXet { get; set; }
    public string? SanPhamUrl { get; set; }
    public decimal? DiemSanPham { get; set; }
    public long? NguoiGhi { get; set; }
    public DateTime? GhiLuc { get; set; }

    public DangKy? DangKy { get; set; }
    public BuoiHoc? BuoiHoc { get; set; }
    public NoiDungBuoi? NoiDungDuocTinh { get; set; }
    public TaiKhoan? NguoiGhiTaiKhoan { get; set; }
}
