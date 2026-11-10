using System.Text.RegularExpressions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MyThuat.Api.Data;
using MyThuat.Api.Models;
using MyThuat.Api.Security;
using MyThuat.Api.Services;
namespace MyThuat.Api.Controllers;
// Only deliberately public editorial images. Student-private work is not uploaded here automatically.
[ApiController,Route("api/tep/cong-khai")]
public sealed class TepCongKhaiController(IWebHostEnvironment env,MyThuatDbContext db,AuditService audit):ControllerBase
{
    [Authorize(Policy="HoSo"),HttpPost,RequestSizeLimit(6*1024*1024)]
    public Task<object> Upload(IFormFile file,CancellationToken ct) => Save(file,ct);

    [Authorize(Policy="DaoTao"),HttpPost("khoa-hoc"),RequestSizeLimit(6*1024*1024)]
    public Task<object> UploadCourseImage(IFormFile file,CancellationToken ct) => Save(file,ct);

    [Authorize(Roles="GIAO_VIEN"),HttpGet("giao-vien/toi")]
    public async Task<object> OwnTeacherPhoto(CancellationToken ct)
    {
        var teacherId=await ActiveTeacherId(ct);
        var profile=await db.GioiThieuGiaoVien.AsNoTracking().SingleOrDefaultAsync(x=>x.GiaoVienId==teacherId,ct);
        return new { Url=profile?.AnhDaiDien??"", CongKhai=profile?.CongKhai??true };
    }

    [Authorize(Roles="GIAO_VIEN"),HttpPost("giao-vien/toi"),RequestSizeLimit(6*1024*1024)]
    public async Task<object> UploadOwnTeacherPhoto(IFormFile file,CancellationToken ct)
    {
        var teacherId=await ActiveTeacherId(ct);
        var profile=await db.GioiThieuGiaoVien.SingleOrDefaultAsync(x=>x.GiaoVienId==teacherId,ct);
        var name=await SaveImage(file,ct);
        if(profile is null)
        {
            profile=new GioiThieuGiaoVien { GiaoVienId=teacherId,CongKhai=true };
            db.GioiThieuGiaoVien.Add(profile);
        }
        profile.AnhDaiDien=ImageUrl(name);
        audit.Add("GiaoVien",teacherId,"TU_CAP_NHAT_ANH_CONG_KHAI");
        try { await db.SaveChangesAsync(ct); }
        catch(DbUpdateException)
        {
            System.IO.File.Delete(Path.Combine(MediaDirectory,name));
            throw;
        }
        return new { Url=profile.AnhDaiDien,profile.CongKhai };
    }

    async Task<long> ActiveTeacherId(CancellationToken ct)
    {
        var id=User.TeacherId();
        if(id is null||!await db.GiaoVien.AnyAsync(x=>x.Id==id.Value&&x.TrangThai=="HOAT_DONG",ct))
            throw new ApiError(403,"Tài khoản chưa liên kết với giáo viên đang hoạt động.");
        return id.Value;
    }

    string MediaDirectory=>Path.Combine(env.ContentRootPath,".local","public-media");
    static string ImageUrl(string name)=>"/api/tep/cong-khai/"+name;

    async Task<object> Save(IFormFile file,CancellationToken ct)=>
        new { Url=ImageUrl(await SaveImage(file,ct)),Public=true,ThongBao="Ảnh dùng cho nội dung công khai; không tải tài liệu cá nhân hoặc ảnh chưa được phép công bố." };

    async Task<string> SaveImage(IFormFile file,CancellationToken ct){
        ApiError.Require(file.Length is >0 and <=5*1024*1024,"Ảnh tối đa 5 MB.");
        using var memory=new MemoryStream();await file.CopyToAsync(memory,ct);var bytes=memory.ToArray();
        var extension=ImageExtension(bytes);
        ApiError.Require(extension!=null,"Chỉ nhận ảnh PNG, JPEG hoặc WebP có chữ ký tệp hợp lệ.");
        var name=Guid.NewGuid().ToString("N")+"."+extension;
        Directory.CreateDirectory(MediaDirectory);
        await System.IO.File.WriteAllBytesAsync(Path.Combine(MediaDirectory,name),bytes,ct);
        return name;
    }
    internal static string? ImageExtension(byte[] bytes) => bytes.Length>8&&bytes.Take(8).SequenceEqual(new byte[]{137,80,78,71,13,10,26,10})?"png":bytes.Length>3&&bytes[0]==255&&bytes[1]==216&&bytes[2]==255?"jpg":bytes.Length>12&&System.Text.Encoding.ASCII.GetString(bytes,0,4)=="RIFF"&&System.Text.Encoding.ASCII.GetString(bytes,8,4)=="WEBP"?"webp":null;
    [AllowAnonymous,HttpGet("{name}")]
    public IActionResult Read(string name){if(!Regex.IsMatch(name,"^[a-f0-9]{32}\\.(png|jpg|webp)$"))return NotFound();var path=Path.Combine(MediaDirectory,name);if(!System.IO.File.Exists(path))return NotFound();Response.Headers.CacheControl="public,max-age=86400";Response.Headers.XContentTypeOptions="nosniff";return PhysicalFile(path,name.EndsWith(".png")?"image/png":name.EndsWith(".webp")?"image/webp":"image/jpeg");}
}
