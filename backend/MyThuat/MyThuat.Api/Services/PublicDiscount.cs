using Microsoft.EntityFrameworkCore;
using MyThuat.Api.Data;
using MyThuat.Api.Models;
namespace MyThuat.Api.Services;

public static class PublicDiscount
{
    public static decimal Amount(KhuyenMai p,decimal original)=>Math.Clamp(Math.Min(p.TranGiam??decimal.MaxValue,
        p.LoaiGiam=="PHAN_TRAM"?decimal.Round(original*p.GiaTriGiam/100,0,MidpointRounding.AwayFromZero):p.GiaTriGiam),0,original);
    public static IQueryable<KhuyenMai> Available(MyThuatDbContext db,DateTime now)=>db.KhuyenMai.Where(p=>p.TuDongApDung
        &&p.TrangThai=="HOAT_DONG"&&p.BatDau<=now&&p.KetThuc>now&&p.NhomKhoanThu=="HOC_PHI"&&p.KhoaApDungId!=null&&p.SoNgayTuXacNhanTV==null
        &&db.ApDungUuDai.Count(a=>a.KhuyenMaiId==p.Id&&(a.TrangThaiLuot=="DA_DUNG"||a.TrangThaiLuot=="GIU_LUOT"&&a.HanGiuLuot>now))<p.TongLuot);
    public static async Task<KhuyenMai?> Best(MyThuatDbContext db,long courseId,decimal original,DateTime now,CancellationToken ct)=>
        (await Available(db,now).Where(p=>p.KhoaApDungId==courseId).ToListAsync(ct)).OrderByDescending(p=>Amount(p,original)).ThenBy(p=>p.Id).FirstOrDefault();
}
