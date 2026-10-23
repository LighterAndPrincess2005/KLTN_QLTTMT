using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using MyThuat.Api.Data;
using MyThuat.Api.Models;

namespace MyThuat.Api.Services;

// Explicit local CLI maintenance; never called by HTTP or normal startup.
public static class GroupAccountSetup
{
    public static async Task Run(MyThuatDbContext db, string root)
    {
        Console.WriteLine($"Database: {db.Database.GetDbConnection().DataSource} / {db.Database.GetDbConnection().Database}");
        await db.Database.EnsureCreatedAsync();
        await MainDatabaseSetup.Backup(db, root);
        var admin = await db.TaiKhoan.SingleOrDefaultAsync(x => x.TenDangNhap == "quantri01");
        if (admin != null && (admin.VaiTroChinh != "QUAN_TRI" || admin.ThanhVienId != null || admin.GiaoVienId != null))
            throw new InvalidOperationException("quantri01 đang thuộc hồ sơ/vai trò khác. Dừng để tránh ghi đè nhầm.");
        if (admin == null)
        {
            admin = new TaiKhoan { TenDangNhap="quantri01", HoTenNhanVien="Quản trị trung tâm",
                VaiTroChinh="QUAN_TRI", TrangThai="HOAT_DONG", PhienBanBaoMat=1 };
            admin.PasswordHash = new PasswordHasher<TaiKhoan>().HashPassword(admin, MainData.Password);
            db.TaiKhoan.Add(admin);
            await db.SaveChangesAsync();
        }
        // Seed routines need an active administrator. This named account is in the requested scope.
        if(admin.TrangThai!="HOAT_DONG" || admin.KhoaDenLuc!=null)admin.PhienBanBaoMat++;
        admin.TrangThai="HOAT_DONG"; admin.KhoaDenLuc=null;
        await db.SaveChangesAsync();
        foreach (var script in new[] { "007_WebsiteMain.sql", "008_TachUuDaiCongKhai.sql" })
            await db.Database.ExecuteSqlRawAsync(await File.ReadAllTextAsync(Path.Combine(root,"Database",script)));
        if (!await db.LopHoc.AnyAsync(x=>x.MaLop=="MAIN_L01"))
        {
            await DemoData.Seed(db, root);
            await MainData.Seed(db, root);
        }
        await MainDataExtras.Seed(db);
        await MainPasswordReset.Run(db, root);
        Console.WriteLine("THIET LAP THANH CONG: 32 tai khoan, mat khau Bui@2026. WinForms: quantri01.");
    }
}
