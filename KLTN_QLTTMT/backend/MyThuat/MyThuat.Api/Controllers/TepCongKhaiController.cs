using System.Text.RegularExpressions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MyThuat.Api.Services;
namespace MyThuat.Api.Controllers;
// Only deliberately public editorial images. Student-private work is not uploaded here automatically.
[ApiController,Route("api/tep/cong-khai")]
public sealed class TepCongKhaiController(IWebHostEnvironment env):ControllerBase
{
    [Authorize(Policy="HoSo"),HttpPost,RequestSizeLimit(6*1024*1024)]
    public async Task<object> Upload(IFormFile file,CancellationToken ct){
        ApiError.Require(file.Length is >0 and <=5*1024*1024,"Ảnh tối đa 5 MB.");
        using var memory=new MemoryStream();await file.CopyToAsync(memory,ct);var bytes=memory.ToArray();
        var extension=bytes.Length>8&&bytes.Take(8).SequenceEqual(new byte[]{137,80,78,71,13,10,26,10})?"png":bytes.Length>3&&bytes[0]==255&&bytes[1]==216&&bytes[2]==255?"jpg":bytes.Length>12&&System.Text.Encoding.ASCII.GetString(bytes,0,4)=="RIFF"&&System.Text.Encoding.ASCII.GetString(bytes,8,4)=="WEBP"?"webp":null;
        ApiError.Require(extension!=null,"Chỉ nhận ảnh PNG, JPEG hoặc WebP có chữ ký tệp hợp lệ.");
        var name=Guid.NewGuid().ToString("N")+"."+extension;var directory=Path.Combine(env.ContentRootPath,".local","public-media");Directory.CreateDirectory(directory);await System.IO.File.WriteAllBytesAsync(Path.Combine(directory,name),bytes,ct);
        return new{Url="/api/tep/cong-khai/"+name,Public=true,ThongBao="Ảnh dùng cho nội dung công khai; không tải tài liệu cá nhân hoặc ảnh chưa được phép công bố."};
    }
    [AllowAnonymous,HttpGet("{name}")]
    public IActionResult Read(string name){if(!Regex.IsMatch(name,"^[a-f0-9]{32}\\.(png|jpg|webp)$"))return NotFound();var path=Path.Combine(env.ContentRootPath,".local","public-media",name);if(!System.IO.File.Exists(path))return NotFound();Response.Headers.CacheControl="public,max-age=86400";Response.Headers.XContentTypeOptions="nosniff";return PhysicalFile(path,name.EndsWith(".png")?"image/png":name.EndsWith(".webp")?"image/webp":"image/jpeg");}
}
