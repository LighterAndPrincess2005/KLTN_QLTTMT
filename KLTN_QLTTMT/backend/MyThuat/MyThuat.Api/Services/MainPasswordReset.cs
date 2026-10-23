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
        var changed=await ResetAccounts(db);
        await MainDataReport.Save(db,root);
        var oldGuide=Path.Combine(root,".local","MAIN_TAI_KHOAN.md");if(File.Exists(oldGuide)){var text=await File.ReadAllTextAsync(oldGuide);await File.WriteAllTextAsync(oldGuide,text.Replace("Bui123456789",MainData.Password));}
        Console.WriteLine($"Đã kiểm tra 32 tài khoản MAIN, đổi {changed} mật khẩu. Không thay tài khoản cũ ngoài bộ MAIN. Phiên đăng nhập cũ của tài khoản đã đổi hết hiệu lực.");
    }
    public static async Task<int> ResetAccounts(MyThuatDbContext db)
    {
        await using var tx=await db.Database.BeginTransactionAsync(IsolationLevel.Serializable);
        var roles=new Dictionary<string,string> {
            ["quantri01"]="QUAN_TRI", ["hocvu01"]="HOC_VU", ["thungan01"]="THU_NGAN", ["kho01"]="QUAN_LY_KHO"
        };
        for(var i=1;i<=20;i++)roles.Add($"hv{i:00}","THANH_VIEN");
        for(var i=1;i<=8;i++)roles.Add($"gv{i:00}","GIAO_VIEN");
        var names=roles.Keys.ToArray();
        var accounts=await db.TaiKhoan.Where(x=>names.Contains(x.TenDangNhap)).ToListAsync();
        if(accounts.Count!=32 || accounts.Any(x=>x.VaiTroChinh!=roles[x.TenDangNhap]))
            throw new InvalidOperationException("Thiếu tài khoản hoặc sai vai trò trong bộ 32 tài khoản. Không đặt lại ngoài dự kiến.");
        var admin=accounts.Single(x=>x.TenDangNhap=="quantri01");var hasher=new PasswordHasher<TaiKhoan>();var changed=0;
        foreach(var account in accounts){
            var passwordChanged=hasher.VerifyHashedPassword(account,account.PasswordHash,MainData.Password)!=PasswordVerificationResult.Success;
            var locked=account.TrangThai!="HOAT_DONG" || account.KhoaDenLuc!=null;
            if(!passwordChanged && !locked)continue;
            account.PasswordHash=hasher.HashPassword(account,MainData.Password);
            account.TrangThai="HOAT_DONG";account.KhoaDenLuc=null;
            account.PhienBanBaoMat++;changed++;
        }
        db.NhatKyThayDoi.Add(new(){TaiKhoanId=admin.Id,LoaiDoiTuong="MainData",IdDoiTuong=1,HanhDong="DOI_MAT_KHAU_MAU",ThoiDiem=BusinessClock.Now,Kenh="CLI",CorrelationId=Guid.NewGuid().ToString("N"),SauJson=System.Text.Json.JsonSerializer.Serialize(new{AccountsChecked=accounts.Count,AccountsChanged=changed})});
        await db.SaveChangesAsync();await tx.CommitAsync();
        return changed;
    }

}
