using System.ComponentModel.DataAnnotations;
namespace MyThuat.Api.Contracts;
public sealed record GenerateScheduleRequest([Range(1,long.MaxValue)]long PhongHocId,
    [Range(1,long.MaxValue)]long GiaoVienId,DateTime BuoiDau,[Range(1,52)]int CachTuan=1,
    [Range(0,120)]short PhutNghi=0,long? TroGiangId=null,bool XacNhanDuChuyenMon=false);
public sealed record AssignTeacherRequest([Range(1,long.MaxValue)]long GiaoVienId,[Required]string VaiTro,
    bool XacNhanDuChuyenMon,DateOnly TuNgay,DateOnly DenNgay);
public sealed record SubstituteRequest([Range(1,long.MaxValue)]long GiaoVienId,[Required]string VaiTro,
    [Required,StringLength(500)]string LyDo,bool XacNhanDuChuyenMon);
public sealed record RescheduleRequest(DateTime BatDau,[Range(1,long.MaxValue)]long PhongHocId,
    [Required,StringLength(1000)]string LyDo,[Required]string RowVersion);
public sealed record AttendRequest([Required]string TrangThai,[Range(0,1440)]short? PhutThamDu,
    [Range(0,10)]decimal? DiemSanPham,[StringLength(2000)]string? NhanXet,[StringLength(500)]string? SanPhamUrl,
    [Required]string RowVersion);
public sealed record AbsenceRequest([StringLength(2000)]string? LyDo);
public sealed record MakeupRequest([Range(1,long.MaxValue)]long DiemDanhVangId,[Range(1,long.MaxValue)]long BuoiHocId);
public sealed record StudentLearningRequest([Range(1,long.MaxValue)]long DiemDanhId,[Required]string LoaiYeuCau,
    [Required,StringLength(1000)]string LyDo,[StringLength(300)]string? KhungGioMongMuon);
public sealed record ResultRequest([Range(1,long.MaxValue)]long HoSoTheoHocId,[Range(0,10)]decimal DiemCuoiKhoa,
    [Required,StringLength(2000)]string NhanXet,[StringLength(500)]string? BaiCuoiKhoaUrl);
