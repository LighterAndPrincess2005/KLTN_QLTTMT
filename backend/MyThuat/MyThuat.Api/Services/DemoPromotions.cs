using System.Data;
using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using MyThuat.Api.Data;
using MyThuat.Api.Models;

namespace MyThuat.Api.Services;

// Explicit local CLI seed for the test guide; never runs on normal API startup.
public static class DemoPromotions
{
    public static async Task Seed(MyThuatDbContext db)
    {
        await using var tx=await db.Database.BeginTransactionAsync(IsolationLevel.Serializable);
        var admin=await db.TaiKhoan.Where(x=>x.VaiTroChinh=="QUAN_TRI"&&x.TrangThai=="HOAT_DONG")
            .OrderBy(x=>x.Id).FirstOrDefaultAsync()??throw new InvalidOperationException("Cần có quản trị hoạt động trước khi nạp mã kiểm thử.");
        var foundation=await db.KhoaHoc.SingleOrDefaultAsync(x=>x.MaKhoa=="DEMO_KH02")
            ??throw new InvalidOperationException("Cần có bộ khóa học mẫu trước khi nạp mã kiểm thử.");
        var start=BusinessClock.Today.ToDateTime(TimeOnly.MinValue);
        var end=start.AddDays(90);
        var examples=new[] {
            new KhuyenMai {MaKhuyenMai="LHLTEST10",TenKhuyenMai="Mẫu kiểm thử: giảm 10% học phí",NhomKhoanThu="HOC_PHI",LoaiGiam="PHAN_TRAM",GiaTriGiam=10,TranGiam=500000,TongLuot=1000,BatDau=start,KetThuc=end,TrangThai="HOAT_DONG"},
            new KhuyenMai {MaKhuyenMai="LHLTEST200K",TenKhuyenMai="Mẫu kiểm thử: Foundation giảm 200.000đ",NhomKhoanThu="HOC_PHI",LoaiGiam="SO_TIEN",GiaTriGiam=200000,TongLuot=1000,BatDau=start,KetThuc=end,TrangThai="HOAT_DONG",KhoaApDungId=foundation.Id},
        };
        var added=new List<string>();
        foreach(var example in examples)
        {
            if(await db.KhuyenMai.AnyAsync(x=>x.MaKhuyenMai==example.MaKhuyenMai))continue;
            db.KhuyenMai.Add(example);added.Add(example.MaKhuyenMai);
        }
        if(added.Count>0)
        {
            db.NhatKyThayDoi.Add(new(){TaiKhoanId=admin.Id,LoaiDoiTuong="DemoPromotions",IdDoiTuong=0,HanhDong="THEM_MAU",
                SauJson=JsonSerializer.Serialize(new{Codes=added}),ThoiDiem=BusinessClock.Now,CorrelationId=Guid.NewGuid().ToString("N"),Kenh="CLI"});
            await db.SaveChangesAsync();
        }
        await tx.CommitAsync();
        Console.WriteLine($"Đã thêm {added.Count} mã kiểm thử. Mã đã tồn tại được giữ nguyên; không thay đổi quy định, tài khoản hoặc đăng ký.");
    }
}
