using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MyThuat.Api.Contracts;
using MyThuat.Api.Data;
using MyThuat.Api.Services;
using MyThuat.Api.Models;
using MyThuat.Api.Security;
namespace MyThuat.Api.Controllers;
[ApiController,Authorize,Route("api/hoc-tap")]
public sealed class HocTapController(LearningService service,MyThuatDbContext db,AccessService access):ControllerBase
{
    [HttpGet("giao-vien/buoi-cua-toi")]
    public async Task<object> TeacherSessions(CancellationToken ct){var teacher=User.TeacherId();ApiError.Require(teacher.HasValue,"Tài khoản không phải giáo viên.",403);return await db.GiangDayBuoi.AsNoTracking().Where(x=>x.GiaoVienId==teacher&&x.BuoiHoc!.TrangThai!="DA_HUY").OrderBy(x=>x.BuoiHoc!.BatDau).Select(x=>new{Id=x.BuoiHocId,x.BuoiHoc!.LopHocId,MaLop=x.BuoiHoc.LopHoc!.MaLop,TenLop=x.BuoiHoc.LopHoc.TenLop,TenKhoa=x.BuoiHoc.LopHoc.KhoaHoc!.TenKhoa,x.BuoiHoc.BatDau,x.BuoiHoc.KetThuc,x.BuoiHoc.TrangThai,ThuTu=x.BuoiHoc.ThuTuTrongLop,ChuDe=x.BuoiHoc.NoiDungBuoi!.ChuDe,TenPhong=x.BuoiHoc.PhongHoc!.TenPhong,x.VaiTro,SoHocVien=db.DiemDanh.Count(d=>d.BuoiHocId==x.BuoiHocId&&d.TrangThai!="HUY_BO"&&d.DangKy!.TrangThai=="DA_XAC_NHAN")}).Take(1000).ToListAsync(ct);}
    [HttpPost("de-nghi")]
    public async Task<object> Suggest(LearningSuggestion r,CancellationToken ct){
        ApiError.Require(r.LoaiYeuCau is "HOC_BU" or "DOI_LICH","Loại đề nghị không hợp lệ.");var row=await db.DiemDanh.Include(x=>x.BuoiHoc).Include(x=>x.DangKy).ThenInclude(x=>x!.HoSoTheoHoc).SingleOrDefaultAsync(x=>x.Id==r.DiemDanhId,ct)??throw new ApiError(404,"Không có buổi học.");await access.Student(row.DangKy!.HoSoTheoHoc!.HocVienId,true,ct);ApiError.Require(User.MemberId()!=null&&row.DangKy.TrangThai=="DA_XAC_NHAN","Cần đăng ký đã xác nhận.");ApiError.Require(r.LoaiYeuCau=="HOC_BU"?row.TrangThai=="VANG":row.TrangThai=="CHO_THAM_GIA"&&row.BuoiHoc!.BatDau>BusinessClock.Now,"Buổi không phù hợp loại đề nghị.");var prefix=$"[{r.LoaiYeuCau}:{row.Id}]\n";ApiError.Require(!await db.PhanHoi.AnyAsync(x=>x.DangKyId==row.DangKyId&&x.TrangThai=="CHO_XU_LY"&&x.NoiDung.StartsWith(prefix),ct),"Buổi này đã có đề nghị đang xử lý.",409);var request=new PhanHoi{DangKyId=row.DangKyId,NguoiGuiId=User.MemberId()!.Value,NoiDung=prefix+r.LyDo.Trim()+(string.IsNullOrWhiteSpace(r.KhungGioMongMuon)?"":"\nKhung giờ mong muốn: "+r.KhungGioMongMuon),GuiLuc=BusinessClock.Now,HanPhanHoi=BusinessClock.Now.AddDays(2),TrangThai="CHO_XU_LY"};db.PhanHoi.Add(request);await db.SaveChangesAsync(ct);return new{request.Id,request.TrangThai};
    }
    [HttpGet("buoi/{id:long:min(1)}/danh-sach")]public Task<object> Roster(long id,CancellationToken ct)=>service.Roster(id,ct);
    [HttpPut("diem-danh/{id:long:min(1)}")]public Task<object> Attend(long id,AttendRequest r,CancellationToken ct)=>service.Attend(id,r,ct);
    [HttpPost("diem-danh/{id:long:min(1)}/bao-nghi")]public Task<object> Absence(long id,AbsenceRequest r,CancellationToken ct)=>service.Absence(id,r,ct);
    [HttpPost("hoc-bu")]public Task<object> Makeup(MakeupRequest r,CancellationToken ct)=>service.Makeup(r,ct);
    [HttpPost("buoi/{id:long:min(1)}/hoan-thanh")]public async Task<IActionResult> Close(long id,CancellationToken ct){await service.CloseSession(id,ct);return NoContent();}
    [HttpPost("ket-qua/de-xuat")]public Task<object> ProposeResult(ResultRequest r,CancellationToken ct)=>service.ProposeResult(r,ct);
    [HttpPost("ket-qua")]public Task<object> Result(ResultRequest r,CancellationToken ct)=>service.Result(r,ct);
    [HttpGet("ho-so/{id:long:min(1)}/tien-do")]
    public async Task<object> Progress(long id,CancellationToken ct)
    {
        var profile=await db.HoSoTheoHoc.AsNoTracking().SingleOrDefaultAsync(x=>x.Id==id,ct)??throw new ApiError(404,"Không có hồ sơ.");
        await access.Student(profile.HocVienId,false,ct);
        var rows=await db.DiemDanh.AsNoTracking().Where(x=>x.DangKy!.HoSoTheoHocId==id).OrderBy(x=>x.BuoiHoc!.BatDau)
            .Select(x=>new {x.Id,x.DangKyId,x.BuoiHocId,x.NoiDungDuocTinhId,x.LoaiThamGia,x.TrangThai,x.NhanXet,x.DiemSanPham,x.SanPhamUrl,x.BaoNghiLuc,x.GhiLuc,
                NgayHoc=x.BuoiHoc!.BatDau,x.BuoiHoc.KetThuc,ThuTu=x.BuoiHoc.ThuTuTrongLop,ChuDe=x.BuoiHoc.NoiDungBuoi!.ChuDe,
                TrangThaiBuoi=x.BuoiHoc.TrangThai,TenPhong=x.BuoiHoc.PhongHoc!.TenPhong,x.BuoiHoc.LopHocId,TenLop=x.BuoiHoc.LopHoc!.TenLop,
                NguoiNhanXet=x.NguoiGhiTaiKhoan==null?null:x.NguoiGhiTaiKhoan.TenDangNhap,
                GiaoVien=db.PhanCong.Where(p=>p.LopHocId==x.BuoiHoc.LopHocId&&p.VaiTroTrongLop=="CHINH"
                    &&p.TuNgay<=DateOnly.FromDateTime(x.BuoiHoc.BatDau)&&(p.DenNgay==null||p.DenNgay>=DateOnly.FromDateTime(x.BuoiHoc.BatDau)))
                    .OrderByDescending(p=>p.TuNgay).Select(p=>p.GiaoVien!.HoTen).FirstOrDefault()}).ToListAsync(ct);
        var result=await db.KetQuaKhoaHoc.AsNoTracking().SingleOrDefaultAsync(x=>x.HoSoTheoHocId==id,ct);
        return new {HoSoTheoHocId=id,SoNoiDungDaThamGia=rows.Where(x=>x.TrangThai=="CO_MAT").Select(x=>x.NoiDungDuocTinhId).Distinct().Count(),DiemDanh=rows,
            HocBu=await db.HocBu.AsNoTracking().Where(x=>x.DiemDanhVang!.DangKy!.HoSoTheoHocId==id).Select(x=>new{x.Id,x.DiemDanhVangId,x.DiemDanhBuId,x.TrangThai,x.HanHoanTat,NgayBu=x.DiemDanhBuId==null?(DateTime?)null:x.DiemDanhBu!.BuoiHoc!.BatDau,TenLopBu=x.DiemDanhBuId==null?null:x.DiemDanhBu!.BuoiHoc!.LopHoc!.TenLop}).ToListAsync(ct),
            KetQua=result is null?null:ManagementService.Scalars(result)};
    }
}
