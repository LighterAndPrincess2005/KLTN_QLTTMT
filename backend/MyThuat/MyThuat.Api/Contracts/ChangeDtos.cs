using System.ComponentModel.DataAnnotations;
namespace MyThuat.Api.Contracts;
public sealed record ChangeRequest([Range(1,long.MaxValue)]long DangKyId,[Required]string LoaiYeuCau,
    [Required]string NguyenNhan,[Required,StringLength(1000)]string LyDo,
    [Required,StringLength(100,MinimumLength=8)]string IdempotencyKey,long? LopDichId=null);
public sealed record ChangeDecision(bool DongY,[Required,StringLength(1000)]string LyDo);
