using System.ComponentModel.DataAnnotations;
namespace MyThuat.Api.Contracts;
public sealed record InventoryLine([Range(1,long.MaxValue)]long HoaCuId,[Range(0,1000000)]decimal SoLuong,
    [Range(0,1000000000)]decimal DonGia,[StringLength(50)]string? MaLo,DateOnly? HanSuDung,
    long? ChiTietThamChieuId=null,long? KhoanThuDangKyId=null,decimal? SoLuongThucTe=null);
public sealed record InventoryRequest([Required,StringLength(30)]string MaPhieu,[Required]string LoaiPhieu,
    [Required,MinLength(1),MaxLength(100)]List<InventoryLine> ChiTiet,
    long? NhaCungCapId=null,long? BuoiHocId=null,long? PhieuThamChieuId=null,long? NguoiNhan=null,
    [StringLength(1000)]string? LyDo=null,[StringLength(500)]string? TepBienBanUrl=null);
public sealed record InventoryApproval([Required]string RowVersion);
