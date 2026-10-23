using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MyThuat.Api.Data;
using MyThuat.Api.Security;
using MyThuat.Api.Services;
namespace MyThuat.Api.Controllers;
[ApiController,Authorize(Policy="BaoCao"),Route("api/bao-cao")]
public sealed class BaoCaoController(MyThuatDbContext db):ControllerBase
{
    [HttpGet("tong-quan")]
    public async Task<object> Overview(CancellationToken ct)=>new {
        HocVien=await db.HocVien.CountAsync(x=>x.TrangThai=="HOAT_DONG",ct),
        GiaoVien=await db.GiaoVien.CountAsync(x=>x.TrangThai=="HOAT_DONG",ct),
        LopTuyenSinh=await db.LopHoc.CountAsync(x=>x.TrangThai=="DANG_TUYEN_SINH",ct),
        LopDangHoc=await db.LopHoc.CountAsync(x=>x.TrangThai=="DANG_HOC",ct),
        PhanHoiQuaHan=await db.PhanHoi.CountAsync(x=>x.TrangThai=="CHO_XU_LY"&&x.HanPhanHoi<BusinessClock.Now,ct)};
    [HttpGet("thu-hoan")]
    public async Task<object> Cash(DateOnly tuNgay,DateOnly denNgay,CancellationToken ct)
    {
        ApiError.Require(User.Has(Permission.ThuTien),"Cần quyền thu tiền để xem báo cáo tài chính.",403);
        ApiError.Require(tuNgay.Year>=1900&&denNgay>=tuNgay&&denNgay.DayNumber-tuNgay.DayNumber<=366,"Khoảng ngày tối đa 366 ngày.");
        var from=tuNgay.ToDateTime(TimeOnly.MinValue);var to=denNgay.AddDays(1).ToDateTime(TimeOnly.MinValue);
        var rows=await db.GiaoDichTien.AsNoTracking().Where(x=>x.ThoiDiem>=from&&x.ThoiDiem<to).ToListAsync(ct);
        var posted=rows.Where(x=>x.TrangThai=="DA_XAC_NHAN"&&x.KhoanThuId!=null).ToList();
        return new {TuNgay=tuNgay,DenNgay=denNgay,ThuDaPhanBo=posted.Where(x=>x.LoaiGiaoDich=="THU").Sum(x=>x.SoTien),
            HoanDaPhanBo=posted.Where(x=>x.LoaiGiaoDich=="HOAN").Sum(x=>x.SoTien),
            ThuChoDoiChieu=rows.Where(x=>x.TrangThai=="CHO_DOI_CHIEU"&&x.LoaiGiaoDich=="THU").Sum(x=>x.SoTien),
            HoanChoDoiChieu=rows.Where(x=>x.KhoanThuId==null&&x.LoaiGiaoDich=="HOAN").Sum(x=>x.SoTien)};
    }
    [HttpGet("lop/{id:long:min(1)}")]
    public async Task<object> Class(long id,CancellationToken ct)
    {
        var cls=await db.LopHoc.AsNoTracking().SingleOrDefaultAsync(x=>x.Id==id,ct)??throw new ApiError(404,"Không có lớp.");
        var enrollments=await db.DangKy.AsNoTracking().Where(x=>x.LopHocId==id).ToListAsync(ct);
        return new {cls.Id,cls.TenLop,cls.SoBuoiKeHoach,SoBuoiDaToChuc=await db.BuoiHoc.CountAsync(x=>x.LopHocId==id&&x.TrangThai=="DA_TO_CHUC",ct),
            DaXacNhan=enrollments.Count(x=>x.TrangThai=="DA_XAC_NHAN"),DangGiuCho=enrollments.Count(x=>
                (x.TrangThai=="CHO_THANH_TOAN"||x.TrangThai=="CHO_CHUYEN")&&x.HanGiuCho>BusinessClock.Now)};
    }
    [Authorize(Policy="NhatKy"),HttpGet("nhat-ky")]
    public async Task<object> Audit([Range(1,1000000)]int trang=1,[Range(1,100)]int kichThuoc=20,CancellationToken ct=default)=>
        await db.NhatKyThayDoi.AsNoTracking().OrderByDescending(x=>x.Id).Skip((trang-1)*kichThuoc).Take(kichThuoc)
            .Select(x=>new{x.Id,x.TaiKhoanId,x.LoaiDoiTuong,x.IdDoiTuong,x.HanhDong,x.ThoiDiem,x.Kenh,x.CorrelationId,x.SauJson}).ToListAsync(ct);
}
