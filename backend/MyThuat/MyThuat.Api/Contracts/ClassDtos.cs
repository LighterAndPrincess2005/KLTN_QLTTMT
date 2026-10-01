using System.ComponentModel.DataAnnotations;
namespace MyThuat.Api.Contracts;
public sealed record PostponeClassRequest(DateTime BuoiDauMoi,[Required,StringLength(1000)]string LyDo);
public sealed record CancelClassRequest([Required,StringLength(1000)]string LyDo);
public sealed record ResetPasswordRequest([Required,StringLength(128,MinimumLength=12)]string MatKhauMoi,[Required,StringLength(1000)]string LyDo);
