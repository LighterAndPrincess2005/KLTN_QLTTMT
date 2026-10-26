using System.Data;
using System.Security.Cryptography;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using MyThuat.Api.Data;
using MyThuat.Api.Models;

namespace MyThuat.Api.Services;

public sealed class AccountRecoveryService(MyThuatDbContext db,IRecoveryMail mail,IPasswordHasher<TaiKhoan> passwords,AuditService audit,ILogger<AccountRecoveryService> logger)
{
    const string Reset="DAT_LAI_MAT_KHAU";
    static readonly PasswordHasher<KhoiPhucTaiKhoan> codes=new();
    public const string GenericMessage="Nếu tài khoản có email hợp lệ, mã sẽ được gửi đến email đã lưu. Kiểm tra cả thư rác. Mã có hạn 10 phút; chờ ít nhất 60 giây trước khi yêu cầu lại. Nếu không nhận được mã, vui lòng kiểm tra tên đăng nhập hoặc liên hệ trung tâm.";
    static string Email(string value)=>value.Trim().ToLowerInvariant();
    public async Task<object> RequestReset(string name,CancellationToken ct)
    {
        mail.EnsureConfigured();
        var account=await db.TaiKhoan.SingleOrDefaultAsync(x=>x.TenDangNhap==name.Trim().ToLower()&&x.TrangThai=="HOAT_DONG",ct);
        var id=Guid.NewGuid().ToString("N");string? masked=null;
        if(account!=null&&!string.IsNullOrWhiteSpace(account.Email)){
            var now=BusinessClock.Now;
            var recent=await db.KhoiPhucTaiKhoan.AsNoTracking().Where(x=>x.TaiKhoanId==account.Id&&x.MucDich==Reset&&x.DaDungLuc==null
                &&x.HetHanLuc>now&&x.CreatedAt>now.AddSeconds(-60)&&x.PhienBanBaoMat==account.PhienBanBaoMat&&x.EmailNhan==Email(account.Email))
                .OrderByDescending(x=>x.Id).FirstOrDefaultAsync(ct);
            if(recent!=null)return new{MaYeuCau=recent.MaYeuCau,EmailNhanDaAn=MaskEmail(account.Email),ThongBao=$"Mã đã được gửi gần đây đến {MaskEmail(account.Email)}. Bạn có thể dùng mã đó; vui lòng chờ ít nhất 60 giây trước khi yêu cầu gửi lại."};
            var issued=await Issue(account,Email(account.Email),Reset,ct);
            if(issued!=null){id=issued;masked=MaskEmail(account.Email);}
        }
        return new{MaYeuCau=id,EmailNhanDaAn=masked,ThongBao=masked==null?GenericMessage:$"Mã khôi phục đã được gửi đến {masked}. Mã có hạn 10 phút. Kiểm tra cả thư rác; không chia sẻ mã cho người khác."};
    }
    static string MaskEmail(string email){
        var value=Email(email);var at=value.LastIndexOf('@');if(at<=0)return "***";
        var local=value[..at];var host=value[(at+1)..];var dot=host.LastIndexOf('.');
        var prefix=local[..Math.Min(3,Math.Max(1,local.Length-1))];
        return prefix+"***@"+(host.Length>0?host[..1]:"")+"***"+(dot>0?host[dot..]:"");
    }
    async Task<string?> Issue(TaiKhoan account,string email,string purpose,CancellationToken ct)
    {
        var now=BusinessClock.Now;
        await using(var tx=await db.Database.BeginTransactionAsync(IsolationLevel.Serializable,ct)){
            if(await db.KhoiPhucTaiKhoan.AnyAsync(x=>x.TaiKhoanId==account.Id&&x.MucDich==purpose&&x.CreatedAt>now.AddSeconds(-60),ct))return null;
            foreach(var old in await db.KhoiPhucTaiKhoan.Where(x=>x.TaiKhoanId==account.Id&&x.MucDich==purpose&&x.DaDungLuc==null).ToListAsync(ct))old.DaDungLuc=now;
            var code=RandomNumberGenerator.GetInt32(1000000).ToString("D6");
            var row=new KhoiPhucTaiKhoan{MaYeuCau=Guid.NewGuid().ToString("N"),TaiKhoanId=account.Id,EmailNhan=email,MucDich=purpose,PhienBanBaoMat=account.PhienBanBaoMat,HetHanLuc=now.AddMinutes(10)};
            row.MaBam=codes.HashPassword(row,code);db.KhoiPhucTaiKhoan.Add(row);await db.SaveChangesAsync(ct);await tx.CommitAsync(ct);
            try{await mail.Send(email,code,false,ct);}
            catch(Exception ex) when(ex is not OperationCanceledException){
                row.DaDungLuc=BusinessClock.Now;await db.SaveChangesAsync(ct);
                // Never log message bodies, codes, credentials or recipient addresses.
                logger.LogWarning("Gửi thư khôi phục thất bại ({ErrorType}), yêu cầu {Id}",ex.GetType().Name,row.Id);return null;
            }
            return row.MaYeuCau;
        }
    }
    public async Task Complete(string id,string code,string newPassword,CancellationToken ct)
    {
        ApiError.Require(newPassword is {Length:>=8 and <=128}&&newPassword.Any(char.IsLetter)&&newPassword.Any(char.IsDigit),"Mật khẩu mới cần 8–128 ký tự, có chữ và số.");
        await using var tx=await db.Database.BeginTransactionAsync(IsolationLevel.Serializable,ct);
        var row=await db.KhoiPhucTaiKhoan.Include(x=>x.TaiKhoan).SingleOrDefaultAsync(x=>x.MaYeuCau==id,ct);
        var now=BusinessClock.Now;var purpose=Reset;
        ApiError.Require(row!=null&&row.MucDich==purpose&&row.DaDungLuc==null&&row.HetHanLuc>now&&row.SoLanSai<5
            &&row.TaiKhoan!.TrangThai=="HOAT_DONG"&&row.PhienBanBaoMat==row.TaiKhoan.PhienBanBaoMat,
            "Mã không hợp lệ, đã hết hạn hoặc đã dùng. Hãy yêu cầu mã mới.",400);
        if(codes.VerifyHashedPassword(row!,row!.MaBam,code)!=PasswordVerificationResult.Success){
            row.SoLanSai++;if(row.SoLanSai>=5)row.DaDungLuc=now;
            await db.SaveChangesAsync(ct);await tx.CommitAsync(ct);throw new ApiError(400,"Mã không đúng. Tối đa 5 lần thử cho mỗi mã.");
        }
        var account=row.TaiKhoan!;
        ApiError.Require(Email(account.Email??"")==row.EmailNhan,"Email tài khoản đã thay đổi. Hãy yêu cầu mã mới.",400);
        account.PasswordHash=passwords.HashPassword(account,newPassword);account.PhienBanBaoMat++;
        foreach(var pending in await db.KhoiPhucTaiKhoan.Where(x=>x.TaiKhoanId==account.Id&&x.DaDungLuc==null).ToListAsync(ct))pending.DaDungLuc=now;
        audit.Add("TaiKhoan",account.Id,"DAT_LAI_MAT_KHAU",actor:account.Id);
        await db.SaveChangesAsync(ct);await tx.CommitAsync(ct);
    }
}
