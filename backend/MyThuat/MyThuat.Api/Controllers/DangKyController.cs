using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using MyThuat.Api.Data;
using MyThuat.Api.Security;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MyThuat.Api.Contracts;
using MyThuat.Api.Services;
namespace MyThuat.Api.Controllers;
[ApiController,Authorize,Route("api/dang-ky")]
public sealed class DangKyController(EnrollmentService service,FinanceService finance,MyThuatDbContext db):ControllerBase
{
    [HttpGet("quan-ly")]
    public async Task<object> Manage([Range(1,1000000)] int trang=1,
        [Range(1,100)] int kichThuoc=20, string? timKiem=null, string? trangThai=null, CancellationToken ct=default)
    {
        ApiError.Require(User.Has(Permission.HoSo)||User.Has(Permission.ThuTien),"Cần quyền học vụ hoặc thu tiền.",403);
        await service.Expire(ct);
        var query=db.DangKy.AsNoTracking();
        if(!string.IsNullOrWhiteSpace(trangThai))query=query.Where(x=>x.TrangThai==trangThai);
        if(!string.IsNullOrWhiteSpace(timKiem))
        {
            var term=timKiem.Trim();
            query=query.Where(x=>x.MaDangKy.Contains(term)||x.HoSoTheoHoc!.HocVien!.HoTen.Contains(term)||x.LopHoc!.TenLop.Contains(term));
        }
        var total=await query.CountAsync(ct);
        var rows=await query.OrderByDescending(x=>x.LapLuc).ThenByDescending(x=>x.Id)
            .Skip((trang-1)*kichThuoc).Take(kichThuoc).Select(x=>new {
                x.Id,x.MaDangKy,x.HoSoTheoHocId,x.LopHocId,x.NguoiDangKyId,
                HocVienId=x.HoSoTheoHoc!.HocVienId,HocVien=x.HoSoTheoHoc.HocVien!.HoTen,
                TenKhoa=x.HoSoTheoHoc.KhoaHoc!.TenKhoa,TenLop=x.LopHoc!.TenLop,
                NguoiDangKy=x.NguoiDangKy!.HoTen,x.LapLuc,x.HanGiuCho,x.HocPhiGocChot,x.TrangThai,x.RowVersion
            }).ToListAsync(ct);
        return new {DuLieu=rows,TongSo=total,Trang=trang,KichThuoc=kichThuoc};
    }
    [HttpGet("toi")]
    public async Task<object> Mine(CancellationToken ct)
    {
        var member=User.MemberId();ApiError.Require(member.HasValue,"Tài khoản không liên kết thành viên.",403);
        var now=BusinessClock.Now;
        var staff=User.Has(Permission.HoSo);
        return await db.DangKy.AsNoTracking().Where(x=>db.DaiDienHocVien.Any(d=>d.ThanhVienId==member
            &&d.HocVienId==x.HoSoTheoHoc!.HocVienId&&d.XacNhanLuc!=null&&d.HieuLucTu<=now&&(d.HieuLucDen==null||d.HieuLucDen>now)))
            .OrderByDescending(x=>x.LapLuc).Select(x=>new {x.Id,x.MaDangKy,x.HoSoTheoHocId,x.LopHocId,x.TrangThai,x.HanGiuCho,
                HocVienId=x.HoSoTheoHoc!.HocVienId,HocVien=x.HoSoTheoHoc.HocVien!.HoTen,TenKhoa=x.HoSoTheoHoc.KhoaHoc!.TenKhoa,
                KhoaHocId=x.HoSoTheoHoc.KhoaHocId,SoBuoi=x.HoSoTheoHoc.KhoaHoc.SoBuoi,TenLop=x.LopHoc!.TenLop,LichHoc=x.LopHoc.LichHocDuKien,
                CoTheQuanLy=staff||db.DaiDienHocVien.Any(d=>d.ThanhVienId==member&&d.HocVienId==x.HoSoTheoHoc.HocVienId&&d.XacNhanLuc!=null
                    &&d.HieuLucTu<=now&&(d.HieuLucDen==null||d.HieuLucDen>now)&&(d.QuyenDaiDien=="DAY_DU"||d.QuyenDaiDien=="DANG_KY"))}).Take(500).ToListAsync(ct);
    }
    [HttpPost]public Task<object> Create(EnrollRequest r,CancellationToken ct)=>service.Create(r,ct);
    [HttpGet("{id:long:min(1)}")]public Task<object> Detail(long id,CancellationToken ct)=>service.Detail(id,ct);
    [HttpGet("{id:long:min(1)}/cong-no")]public Task<object> Debt(long id,CancellationToken ct)=>finance.Debt(id,ct);
}
