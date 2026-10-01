using System.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MyThuat.Api.Contracts;
using MyThuat.Api.Data;
using MyThuat.Api.Models;
using MyThuat.Api.Security;
using MyThuat.Api.Services;
namespace MyThuat.Api.Controllers;
[ApiController,Authorize,Route("api/hoc-vien")]
public sealed class HocVienController(MyThuatDbContext db,AuditService audit):ControllerBase
{
    [HttpGet("toi")]
    public async Task<object> Mine(CancellationToken ct)
    {
        var member=User.MemberId();ApiError.Require(member.HasValue,"Tài khoản không liên kết thành viên.",403);
        var now=BusinessClock.Now;
        return await db.DaiDienHocVien.AsNoTracking().Where(x=>x.ThanhVienId==member&&x.HieuLucTu<=now&&(x.HieuLucDen==null||x.HieuLucDen>now))
            .Select(x=>new {x.Id,x.HocVienId,x.HocVien!.HoTen,x.HocVien.NgaySinh,x.HocVien.TrangThai,x.QuanHe,x.QuyenDaiDien,x.XacNhanLuc,x.HocVien.CapDoDeXuat}).ToListAsync(ct);
    }
    [HttpPost("toi")]
    public async Task<object> Add(StudentRequest r,CancellationToken ct)
    {
        var member=User.MemberId();ApiError.Require(member.HasValue,"Tài khoản không liên kết thành viên.",403);
        ApiError.Require(r.NgaySinh.Year>=1900&&r.NgaySinh<=BusinessClock.Today&&r.QuanHe is "CHA" or "ME" or "GIAM_HO" or "TU_BAN_THAN","Ngày sinh hoặc quan hệ không hợp lệ.");
        await using var tx=await db.Database.BeginTransactionAsync(IsolationLevel.Serializable,ct);
        var student=new HocVien {MaHocVien="HV"+Guid.NewGuid().ToString("N")[..16],HoTen=r.HoTen,NgaySinh=r.NgaySinh,
            LienHeKhanCapTen=r.LienHeKhanCapTen,LienHeKhanCapSDT=r.LienHeKhanCapSDT,MucTieuHoc=r.MucTieuHoc,TrangThai="CHO_XAC_NHAN"};
        db.HocVien.Add(student);await db.SaveChangesAsync(ct);
        db.DaiDienHocVien.Add(new(){ThanhVienId=member!.Value,HocVienId=student.Id,QuanHe=r.QuanHe,QuyenDaiDien="DAY_DU",LaLienHeChinh=true,HieuLucTu=BusinessClock.Now});
        audit.Add("HocVien",student.Id,"GUI_HO_SO");await db.SaveChangesAsync(ct);await tx.CommitAsync(ct);return new {student.Id,student.HoTen,student.TrangThai};
    }
    [HttpPost("{id:long:min(1)}/danh-gia")]
    public async Task<IActionResult> Assess(long id,AssessRequest r,CancellationToken ct)
    {
        ApiError.Require(User.Has(Permission.HoSo)||User.TeacherId()==r.GiaoVienId,"Không có quyền đánh giá.",403);
        var student=await db.HocVien.SingleOrDefaultAsync(x=>x.Id==id,ct)??throw new ApiError(404,"Không có học viên.");
        if(!User.Has(Permission.HoSo))ApiError.Require(student.GiaoVienDanhGia==r.GiaoVienId,"Học vụ chưa chỉ định bạn đánh giá học viên này.",403);
        ApiError.Require(await db.GiaoVien.AnyAsync(x=>x.Id==r.GiaoVienId&&x.TrangThai=="HOAT_DONG",ct),"Giáo viên không hoạt động.");
        student.GiaoVienDanhGia=r.GiaoVienId;student.CapDoDeXuat=r.CapDoDeXuat;student.NhanXetDauVao=r.NhanXet;student.NgayDanhGiaDauVao=BusinessClock.Today;
        audit.Add("HocVien",id,"DANH_GIA_DAU_VAO",new {r.GiaoVienId,r.CapDoDeXuat});await db.SaveChangesAsync(ct);return NoContent();
    }
    [Authorize(Policy="HoSo"),HttpPost("{id:long:min(1)}/chi-dinh/{teacher:long:min(1)}")]
    public async Task<IActionResult> AssignAssessment(long id,long teacher,CancellationToken ct)
    {
        var student=await db.HocVien.SingleOrDefaultAsync(x=>x.Id==id,ct)??throw new ApiError(404,"Không có học viên.");
        ApiError.Require(await db.GiaoVien.AnyAsync(x=>x.Id==teacher&&x.TrangThai=="HOAT_DONG",ct),"Giáo viên không hoạt động.");
        student.GiaoVienDanhGia=teacher;student.NgayDanhGiaDauVao=null;student.CapDoDeXuat=null;student.NhanXetDauVao=null;
        audit.Add("HocVien",id,"CHI_DINH_DANH_GIA",new {GiaoVienId=teacher});await db.SaveChangesAsync(ct);return NoContent();
    }
    [HttpGet("danh-gia-duoc-giao")]
    public async Task<object> Assessments(CancellationToken ct)
    {
        ApiError.Require(User.TeacherId().HasValue,"Tài khoản không phải giáo viên.",403);
        return await db.HocVien.AsNoTracking().Where(x=>x.GiaoVienDanhGia==User.TeacherId())
            .Select(x=>new {x.Id,x.HoTen,x.NgaySinh,x.MucTieuHoc,x.NhanXetDauVao,x.CapDoDeXuat,x.NgayDanhGiaDauVao}).Take(500).ToListAsync(ct);
    }
}
