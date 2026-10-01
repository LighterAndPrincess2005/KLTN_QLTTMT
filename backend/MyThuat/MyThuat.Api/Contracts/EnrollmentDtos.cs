using System.ComponentModel.DataAnnotations;
namespace MyThuat.Api.Contracts;
public sealed record MaterialPurchase([Range(1,long.MaxValue)]long HoaCuId,[Range(0.001,1000000)]decimal SoLuong);
public sealed record EnrollRequest([Range(1,long.MaxValue)]long HocVienId,[Range(1,long.MaxValue)]long LopHocId,
    [Range(1,long.MaxValue)]long NguoiDangKyId,[Required,StringLength(100,MinimumLength=8)]string IdempotencyKey,
    string? MaGiamHocPhi=null,string? MaGiamHoaCu=null,List<MaterialPurchase>? HoaCu=null);
public sealed record ReceiptRequest([Range(1,long.MaxValue)]long DangKyId,[Range(1,long.MaxValue)]long KhoanThuId,
    [Range(1,1000000000)]decimal SoTien,[Required,StringLength(20)]string PhuongThuc,
    [Required,StringLength(50)]string MaChungTu,[StringLength(100)]string? MaThamChieu,
    [Required,StringLength(100,MinimumLength=8)]string IdempotencyKey);
public sealed record RefundRequest([Range(1,long.MaxValue)]long GiaoDichThuGocId,[Range(1,1000000000)]decimal SoTien,
    [Required,StringLength(50)]string MaChungTu,[Required,StringLength(100,MinimumLength=8)]string IdempotencyKey,
    long? YeuCauThayDoiId=null);
