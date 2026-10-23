using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;
using MyThuat.Api.Data;
using MyThuat.Api.Models;
using MyThuat.Api.Services;
using MyThuat.Checks;
var folder=Path.Combine(Path.GetTempPath(),"GroupAccounts-"+Guid.NewGuid().ToString("N"));
Directory.CreateDirectory(folder);
var options=new DbContextOptionsBuilder<MyThuatDbContext>().UseSqlite("Data Source="+Path.Combine(folder,"check.db")).Options;
try {
 await using var db=new CheckDbContext(options);await db.Database.EnsureCreatedAsync();
 var hasher=new PasswordHasher<TaiKhoan>();
 var admin=new TaiKhoan{TenDangNhap="quantri01",VaiTroChinh="QUAN_TRI",HoTenNhanVien="Quản trị",TrangThai="HOAT_DONG",PhienBanBaoMat=1};
 admin.PasswordHash=hasher.HashPassword(admin,"OldPassword2026!");db.Add(admin);await db.SaveChangesAsync();
 await DemoData.Seed(db,folder);await MainData.Seed(db,folder);await MainDataExtras.Seed(db);
 if(await db.TaiKhoan.CountAsync(x=>x.TenDangNhap=="quantri01")!=1)throw new Exception("Duplicate admin");
 Console.WriteLine("PASS: seed MAIN with existing administrator");
 var outsider=new TaiKhoan{TenDangNhap="outside",VaiTroChinh="QUAN_TRI",TrangThai="HOAT_DONG",PhienBanBaoMat=9};
 outsider.PasswordHash=hasher.HashPassword(outsider,"KeepThis2026!");db.Add(outsider);
 var hv=await db.TaiKhoan.SingleAsync(x=>x.TenDangNhap=="hv01");hv.TrangThai="KHOA";hv.KhoaDenLuc=DateTime.Now.AddDays(1);
 await db.SaveChangesAsync();var oldVersion=admin.PhienBanBaoMat;
 await MainPasswordReset.ResetAccounts(db);
 if(hasher.VerifyHashedPassword(admin,admin.PasswordHash,"Bui@2026")==PasswordVerificationResult.Failed || admin.PhienBanBaoMat<=oldVersion)throw new Exception("Admin reset failed");
 if(hv.TrangThai!="HOAT_DONG"||hv.KhoaDenLuc!=null)throw new Exception("Unlock failed");
 if(hasher.VerifyHashedPassword(outsider,outsider.PasswordHash,"KeepThis2026!")==PasswordVerificationResult.Failed || outsider.PhienBanBaoMat!=9)throw new Exception("Outside account changed");
 Console.WriteLine("PASS: password reset, unlock, token revocation, outside account preserved");
 if(await MainPasswordReset.ResetAccounts(db)!=0)throw new Exception("Repeated reset changed accounts");
 Console.WriteLine("PASS: repeat without changing accounts");
 hv.VaiTroChinh="QUAN_TRI";await db.SaveChangesAsync();
 bool rejected=false;try{await MainPasswordReset.ResetAccounts(db);}catch(InvalidOperationException){rejected=true;}
 if(!rejected)throw new Exception("Wrong role accepted");
 Console.WriteLine("PASS: mismatched role rejected");
} finally {Microsoft.Data.Sqlite.SqliteConnection.ClearAllPools();Directory.Delete(folder,true);}
