using System.ComponentModel.DataAnnotations;
namespace MyThuat.Api.Contracts;
public sealed record StudentRequest([Required,StringLength(150)]string HoTen,DateOnly NgaySinh,
    [Required,StringLength(150)]string LienHeKhanCapTen,[Required,StringLength(25)]string LienHeKhanCapSDT,
    [Required]string QuanHe,[StringLength(1000)]string? MucTieuHoc);
public sealed record AssessRequest([Range(1,long.MaxValue)]long GiaoVienId,[Required,StringLength(100)]string CapDoDeXuat,
    [Required,StringLength(2000)]string NhanXet);
public sealed record FeedbackRequest([Range(1,long.MaxValue)]long DangKyId,[Required,StringLength(2000)]string NoiDung);
public sealed record FeedbackReply([Required,StringLength(2000)]string TraLoi);
