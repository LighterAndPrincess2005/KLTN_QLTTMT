using System.Text.Json;
using System.Text.RegularExpressions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MyThuat.Api.Contracts;
using MyThuat.Api.Data;
using MyThuat.Api.Models;
using MyThuat.Api.Security;
using MyThuat.Api.Services;
namespace MyThuat.Api.Controllers;
[ApiController,Route("api/noi-dung")]
public sealed class NoiDungWebController(MyThuatDbContext db,AuditService audit):ControllerBase
{
    public static bool SafeUrl(string? path)=>!string.IsNullOrWhiteSpace(path)&&path.Length<=1000&&(path.StartsWith('/')&&!path.StartsWith("//")&&!path.Contains('\\')||Uri.TryCreate(path,UriKind.Absolute,out var uri)&&uri.Scheme=="https");
    static object Article(BaiViet a)=>new {a.Id,a.Slug,Title=a.TieuDe,Category=a.ChuDe,Image=a.AnhBia,Intro=a.TomTat,Sections=JsonSerializer.Deserialize<JsonElement>(a.NoiDungJson),Exercise=a.BaiThucHanh,Source=a.Nguon,Gallery=JsonSerializer.Deserialize<string[]>(a.ThuVienJson),a.TrangThai,a.DangLuc,a.RowVersion};
    [AllowAnonymous,HttpGet("bai-viet")]
    public async Task<object> Articles(CancellationToken ct)=> (await db.BaiViet.AsNoTracking().Where(x=>x.TrangThai=="DA_DANG"&&x.DangLuc<=BusinessClock.Now).OrderByDescending(x=>x.DangLuc).Take(100).ToListAsync(ct)).Select(Article);
    [AllowAnonymous,HttpGet("bai-viet/{slug}")]
    public async Task<object> Read(string slug,CancellationToken ct)=>Article(await db.BaiViet.AsNoTracking().SingleOrDefaultAsync(x=>x.Slug==slug&&x.TrangThai=="DA_DANG"&&x.DangLuc<=BusinessClock.Now,ct)??throw new ApiError(404,"Bài viết chưa công bố."));
    [Authorize(Policy="HoSo"),HttpGet("quan-ly/bai-viet")]
    public async Task<object> ArticleAdmin(CancellationToken ct)=>(await db.BaiViet.AsNoTracking().OrderByDescending(x=>x.UpdatedAt).Take(200).ToListAsync(ct)).Select(Article);
    [Authorize(Policy="HoSo"),HttpPut("quan-ly/bai-viet/{id:long}")]
    public async Task<object> SaveArticle(long id,ArticleRequest r,CancellationToken ct){
        ApiError.Require(Regex.IsMatch(r.Slug,"^[a-z0-9]+(?:-[a-z0-9]+)*$")&&r.Sections.Count is >0 and <=30&&r.Gallery.Count<=20&&SafeUrl(r.Image)&&r.Gallery.All(SafeUrl)&&(string.IsNullOrEmpty(r.Source)||SafeUrl(r.Source))&&r.Status is "NHAP" or "DA_DANG" or "AN","Nội dung hoặc đường dẫn không hợp lệ.");
        ApiError.Require(r.Sections.All(x=>!string.IsNullOrWhiteSpace(x.Title)&&!string.IsNullOrWhiteSpace(x.Text)),"Mỗi mục phải có tiêu đề và nội dung.");
        var row=id==0?new BaiViet():await db.BaiViet.SingleOrDefaultAsync(x=>x.Id==id,ct)??throw new ApiError(404,"Không có bài viết.");
        if(id>0)ApiError.Require(Convert.ToBase64String(row.RowVersion)==r.RowVersion,"Bài viết đã thay đổi.",409);
        row.Slug=r.Slug;row.TieuDe=r.Title.Trim();row.ChuDe=r.Category.Trim();row.AnhBia=r.Image;row.TomTat=r.Intro.Trim();row.NoiDungJson=JsonSerializer.Serialize(r.Sections,new JsonSerializerOptions{PropertyNamingPolicy=JsonNamingPolicy.CamelCase});row.BaiThucHanh=r.Exercise;row.Nguon=r.Source;row.ThuVienJson=JsonSerializer.Serialize(r.Gallery);row.TrangThai=r.Status;row.NguoiSuaId=User.AccountId();if(r.Status=="DA_DANG")row.DangLuc??=BusinessClock.Now;if(id==0)db.BaiViet.Add(row);audit.Add("BaiViet",id,"LUU_BAI_VIET");await db.SaveChangesAsync(ct);return Article(row);
    }
    [AllowAnonymous,HttpGet("giao-vien")]
    public async Task<object> Teachers(CancellationToken ct)=>await db.GioiThieuGiaoVien.AsNoTracking().Where(x=>x.CongKhai&&x.GiaoVien!.TrangThai=="HOAT_DONG").OrderBy(x=>x.GiaoVienId).Select(x=>new {Id=x.GiaoVienId,Name=x.GiaoVien!.HoTen,Location=x.KhuVuc,Image=x.AnhDaiDien,Description=x.GioiThieu,Specialty=x.GiaoVien.ChuyenMon,Works=x.TacPhamJson}).ToListAsync(ct);
    [Authorize(Policy="HoSo"),HttpGet("quan-ly/giao-vien")]
    public async Task<object> TeacherAdmin(CancellationToken ct)=>await db.GioiThieuGiaoVien.AsNoTracking().ToListAsync(ct);
    [Authorize(Policy="HoSo"),HttpPut("quan-ly/giao-vien/{id:long}")]
    public async Task<object> SaveTeacher(long id,TeacherIntroRequest r,CancellationToken ct){
        ApiError.Require(await db.GiaoVien.AnyAsync(x=>x.Id==id,ct)&&SafeUrl(r.AnhDaiDien)&&r.TacPham.Count<=20&&r.TacPham.All(SafeUrl),"Giáo viên hoặc hình ảnh không hợp lệ.");var row=await db.GioiThieuGiaoVien.SingleOrDefaultAsync(x=>x.GiaoVienId==id,ct);if(row==null){row=new(){GiaoVienId=id};db.GioiThieuGiaoVien.Add(row);}else ApiError.Require(Convert.ToBase64String(row.RowVersion)==r.RowVersion,"Thông tin đã thay đổi.",409);
        row.AnhDaiDien=r.AnhDaiDien;row.GioiThieu=r.GioiThieu;row.KhuVuc=r.KhuVuc;row.CongKhai=r.CongKhai;row.TacPhamJson=JsonSerializer.Serialize(r.TacPham);audit.Add("GiaoVien",id,"SUA_GIOI_THIEU_CONG_KHAI");await db.SaveChangesAsync(ct);return new{row.Id,row.RowVersion};
    }
    [AllowAnonymous,HttpGet("khoa-hoc")]
    public async Task<object> Courses(CancellationToken ct)=>await db.GioiThieuKhoaHoc.AsNoTracking().Where(x=>x.KhoaHoc!.TrangThai=="HOAT_DONG").ToListAsync(ct);
    [Authorize(Policy="HoSo"),HttpPut("quan-ly/khoa-hoc/{id:long}")]
    public async Task<object> SaveCourse(long id,CourseIntroRequest r,CancellationToken ct){ApiError.Require(await db.KhoaHoc.AnyAsync(x=>x.Id==id,ct)&&SafeUrl(r.AnhDaiDien)&&r.ThanhPham.Count<=20&&r.ThanhPham.All(SafeUrl)&&r.DauRa.Count is >0 and <=20,"Thông tin khóa không hợp lệ.");var row=await db.GioiThieuKhoaHoc.SingleOrDefaultAsync(x=>x.KhoaHocId==id,ct);if(row==null){row=new(){KhoaHocId=id};db.GioiThieuKhoaHoc.Add(row);}else ApiError.Require(Convert.ToBase64String(row.RowVersion)==r.RowVersion,"Thông tin đã thay đổi.",409);row.AnhDaiDien=r.AnhDaiDien;row.DauRaJson=JsonSerializer.Serialize(r.DauRa);row.ThanhPhamJson=JsonSerializer.Serialize(r.ThanhPham);row.HoaCuBaoGom=r.HoaCuBaoGom;row.CanChuanBi=r.CanChuanBi;audit.Add("KhoaHoc",id,"SUA_GIOI_THIEU_KHOA");await db.SaveChangesAsync(ct);return new{row.Id,row.RowVersion};}
    [AllowAnonymous,HttpGet("quy-dinh")]
    public async Task<object> Policy(CancellationToken ct){var rule=await db.QuyDinh.AsNoTracking().Where(x=>x.TrangThai=="HOAT_DONG"&&x.HieuLucTu<=BusinessClock.Now&&(x.HieuLucDen==null||x.HieuLucDen>BusinessClock.Now)).OrderByDescending(x=>x.HieuLucTu).FirstOrDefaultAsync(ct)??throw new ApiError(404,"Chưa công bố quy định.");return new{rule.Id,rule.MaBoQuyDinh,rule.SoPhienBan,QuyDinh=PolicySettings.Parse(rule.NoiDungJson)};}
}
