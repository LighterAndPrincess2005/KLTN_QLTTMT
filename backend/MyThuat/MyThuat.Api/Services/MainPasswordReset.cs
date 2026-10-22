using System.Data;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using MyThuat.Api.Data;
using MyThuat.Api.Models;
namespace MyThuat.Api.Services;
public static class MainPasswordReset
{
    public static async Task Run(MyThuatDbContext db,string root){
        await MainDatabaseSetup.Backup(db,root);
        await using var tx=await db.Database.BeginTransactionAsync(IsolationLevel.Serializable);
        var accounts=await db.TaiKhoan.Where(x=>x.ThanhVien!=null&&x.ThanhVien.MaThanhVien.StartsWith("MAIN_")||x.GiaoVien!=null&&x.GiaoVien.MaGiaoVien.StartsWith("MAIN_")||x.TenDangNhap=="hocvu01"||x.TenDangNhap=="thungan01"||x.TenDangNhap=="kho01"||x.TenDangNhap=="quantri01").ToListAsync();
        if(accounts.Count!=32)throw new InvalidOperationException("Số tài khoản MAIN không phải 32. Dừng để kiểm tra phạm vi, không đặt lại ngoài dự kiến.");
        var admin=accounts.Single(x=>x.TenDangNhap=="quantri01");var hasher=new PasswordHasher<TaiKhoan>();var changed=0;
        foreach(var account in accounts){if(hasher.VerifyHashedPassword(account,account.PasswordHash,MainData.Password)!=PasswordVerificationResult.Failed)continue;account.PasswordHash=hasher.HashPassword(account,MainData.Password);account.PhienBanBaoMat++;changed++;}
        db.NhatKyThayDoi.Add(new(){TaiKhoanId=admin.Id,LoaiDoiTuong="MainData",IdDoiTuong=1,HanhDong="DOI_MAT_KHAU_MAU",ThoiDiem=BusinessClock.Now,Kenh="CLI",CorrelationId=Guid.NewGuid().ToString("N"),SauJson=System.Text.Json.JsonSerializer.Serialize(new{AccountsChecked=accounts.Count,AccountsChanged=changed})});
        await db.SaveChangesAsync();await tx.CommitAsync();await MainDataReport.Save(db,root);
        var oldGuide=Path.Combine(root,".local","MAIN_TAI_KHOAN.md");if(File.Exists(oldGuide)){var text=await File.ReadAllTextAsync(oldGuide);await File.WriteAllTextAsync(oldGuide,text.Replace("Bui123456789",MainData.Password));}
        Console.WriteLine($"Đã kiểm tra 32 tài khoản MAIN, đổi {changed} mật khẩu. Không thay tài khoản cũ ngoài bộ MAIN. Phiên đăng nhập cũ của tài khoản đã đổi hết hiệu lực.");
    }
}
