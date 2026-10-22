using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MyThuat.Api.Data;
using MyThuat.Api.Security;
using MyThuat.Api.Services;
namespace MyThuat.Api.Controllers;
public sealed record QuoteRequest(long HocVienId,long LopHocId,[StringLength(50)]string? MaGiamHocPhi);
[ApiController,Authorize,Route("api/dang-ky/bao-gia")]
public sealed class BaoGiaController(MyThuatDbContext db,AccessService access,PolicyService policies):ControllerBase
{
    [HttpPost]
    public async Task<object> Quote(QuoteRequest r,CancellationToken ct){
        await access.Student(r.HocVienId,true,ct);var now=BusinessClock.Now;var cls=await db.LopHoc.Include(x=>x.KhoaHoc).AsNoTracking().SingleOrDefaultAsync(x=>x.Id==r.LopHocId,ct)??throw new ApiError(404,"Không có lớp.");
        ApiError.Require(cls.TrangThai=="DANG_TUYEN_SINH"&&cls.MoDangKyLuc<=now&&cls.DongDangKyLuc>now,"Lớp không còn nhận đăng ký.",409);
        var student=await db.HocVien.AsNoTracking().SingleAsync(x=>x.Id==r.HocVienId,ct);var age=cls.NgayKhaiGiangDuKien.Year-student.NgaySinh.Year;if(student.NgaySinh.AddYears(age)>cls.NgayKhaiGiangDuKien)age--;ApiError.Require(age>=(cls.KhoaHoc!.TuoiToiThieu??0),"Học viên chưa đủ tuổi tối thiểu.");
        var owner=await db.ThanhVien.AsNoTracking().SingleOrDefaultAsync(x=>x.Id==User.MemberId()&&x.TrangThai=="HOAT_DONG",ct)??throw new ApiError(403,"Thành viên không hoạt động.");
        var link=await db.DaiDienHocVien.AsNoTracking().SingleAsync(x=>x.HocVienId==student.Id&&x.ThanhVienId==owner.Id&&x.HieuLucTu<=now&&(x.HieuLucDen==null||x.HieuLucDen>now),ct);ApiError.Require(age>=18||link.QuanHe!="TU_BAN_THAN","Người dưới 18 tuổi cần người đại diện.");
        var occupied=await db.DangKy.CountAsync(x=>x.LopHocId==cls.Id&&(x.TrangThai=="DA_XAC_NHAN"||x.TrangThai=="CHO_THANH_TOAN"&&x.HanGiuCho>now),ct);ApiError.Require(occupied<cls.SiSoToiDa,"Lớp đã hết chỗ.",409);
        decimal discount=0;string? code=null;if(!string.IsNullOrWhiteSpace(r.MaGiamHocPhi)){
            code=r.MaGiamHocPhi.Trim().ToUpperInvariant();var p=await db.KhuyenMai.AsNoTracking().SingleOrDefaultAsync(x=>x.MaKhuyenMai==code&&x.TrangThai=="HOAT_DONG"&&x.BatDau<=now&&x.KetThuc>now,ct)??throw new ApiError(400,"Mã ưu đãi không hiệu lực.");
            ApiError.Require(p.NhomKhoanThu=="HOC_PHI"&&(p.KhoaApDungId==null||p.KhoaApDungId==cls.KhoaHocId),"Mã không áp dụng cho khóa này.");ApiError.Require(p.SoNgayTuXacNhanTV==null||owner.XacNhanThanhVienLuc.HasValue&&now<=owner.XacNhanThanhVienLuc.Value.AddDays(p.SoNgayTuXacNhanTV.Value),"Thành viên ngoài thời hạn ưu đãi.");
            ApiError.Require(await db.ApDungUuDai.CountAsync(x=>x.KhuyenMaiId==p.Id&&(x.TrangThaiLuot=="DA_DUNG"||x.TrangThaiLuot=="GIU_LUOT"&&x.HanGiuLuot>now),ct)<p.TongLuot,"Ưu đãi đã hết lượt.",409);discount=Math.Min(cls.HocPhiApDung,Math.Min(p.TranGiam??decimal.MaxValue,p.LoaiGiam=="PHAN_TRAM"?decimal.Round(cls.HocPhiApDung*p.GiaTriGiam/100,0,MidpointRounding.AwayFromZero):p.GiaTriGiam));
        }
        var rule=await policies.Current(ct);var settings=PolicySettings.Parse(rule.NoiDungJson);var first=await db.BuoiHoc.Where(x=>x.LopHocId==cls.Id&&x.TrangThai!="DA_HUY").MinAsync(x=>(DateTime?)x.BatDau,ct);ApiError.Require(first.HasValue,"Lớp chưa có lịch.");var expiry=new[]{now.AddHours(settings.HanGiuChoGio),cls.DongDangKyLuc,first!.Value.AddHours(-settings.DongDangKyTruocGio)}.Min();ApiError.Require(expiry>now,"Không còn thời gian giữ chỗ.",409);
        return new{LopHocId=cls.Id,HocPhiGoc=cls.HocPhiApDung,TienGiam=discount,TongPhaiTra=cls.HocPhiApDung-discount,MaGiamHocPhi=code,HanGiuChoDuKien=expiry,QuyDinhId=rule.Id,QuyDinh=settings,ThongBao="Báo giá chưa giữ chỗ hoặc giữ lượt ưu đãi; hệ thống kiểm tra lại khi xác nhận đăng ký."};
    }
}
