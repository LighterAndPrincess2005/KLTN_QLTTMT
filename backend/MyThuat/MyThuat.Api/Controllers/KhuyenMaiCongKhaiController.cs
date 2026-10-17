using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MyThuat.Api.Data;
using MyThuat.Api.Services;
namespace MyThuat.Api.Controllers;

[ApiController,AllowAnonymous,Route("api/khuyen-mai")]
public sealed class KhuyenMaiCongKhaiController(MyThuatDbContext db):ControllerBase
{
    [HttpGet]
    public async Task<object> List(CancellationToken ct)
    {
        var now=BusinessClock.Now;
        return await db.KhuyenMai.AsNoTracking()
            .Where(p=>p.TrangThai=="HOAT_DONG"&&p.BatDau<=now&&p.KetThuc>now&&p.NhomKhoanThu=="HOC_PHI"
                &&db.ApDungUuDai.Count(a=>a.KhuyenMaiId==p.Id&&(a.TrangThaiLuot=="DA_DUNG"||a.TrangThaiLuot=="GIU_LUOT"&&a.HanGiuLuot>now))<p.TongLuot)
            .Select(p=>new {p.MaKhuyenMai,p.TenKhuyenMai,p.KhoaApDungId,p.LoaiGiam,p.GiaTriGiam,p.TranGiam,p.KetThuc,p.SoNgayTuXacNhanTV})
            .Take(100).ToListAsync(ct);
    }
}
