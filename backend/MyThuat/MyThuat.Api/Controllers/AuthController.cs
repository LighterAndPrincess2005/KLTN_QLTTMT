using System.Data;
using System.Security.Claims;
using System.Net;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.EntityFrameworkCore;
using MyThuat.Api.Contracts;
using MyThuat.Api.Data;
using MyThuat.Api.Models;
using MyThuat.Api.Security;
using MyThuat.Api.Services;
namespace MyThuat.Api.Controllers;

[ApiController, Route("api/tai-khoan")]
public sealed class AuthController(MyThuatDbContext db, IPasswordHasher<TaiKhoan> hasher,
    AccessTokens tokens, AuditService audit) : ControllerBase
{
    [AllowAnonymous, HttpGet("thiet-lap")]
    public async Task<object> SetupState(CancellationToken ct) => new { CanTaoQuanTri = IsLocal() && !await db.TaiKhoan.AnyAsync(ct) };

    private bool IsLocal() => HttpContext.Connection.RemoteIpAddress is { } ip && IPAddress.IsLoopback(ip);
    private static string Normalize(string name) => name.Trim().ToLowerInvariant();
    private static void Password(string password) => ApiError.Require(password.Length is >= 12 and <= 128
        && password.Any(char.IsLetter) && password.Any(char.IsDigit), "Mật khẩu cần 12–128 ký tự, có chữ và số.");

    [AllowAnonymous, EnableRateLimiting("login"), HttpPost("thiet-lap")]
    public async Task<IActionResult> Setup(AdminSetupRequest request, CancellationToken ct)
    {
        ApiError.Require(IsLocal(), "Chỉ thiết lập quản trị tại máy chạy API.",403);
        Password(request.MatKhau);
        await using var tx=await db.Database.BeginTransactionAsync(IsolationLevel.Serializable,ct);
        ApiError.Require(!await db.TaiKhoan.AnyAsync(ct), "Tài khoản quản trị đã được thiết lập.",409);
        var account=new TaiKhoan {TenDangNhap=Normalize(request.TenDangNhap), HoTenNhanVien=request.HoTen,
            VaiTroChinh="QUAN_TRI", TrangThai="HOAT_DONG", PhienBanBaoMat=1};
        account.PasswordHash=hasher.HashPassword(account,request.MatKhau);
        db.TaiKhoan.Add(account); await db.SaveChangesAsync(ct);
        db.QuyDinh.Add(new QuyDinh {MaBoQuyDinh="QD_DE_XUAT",SoPhienBan=1,HieuLucTu=BusinessClock.Now,
            NoiDungJson=System.Text.Json.JsonSerializer.Serialize(new PolicySettings()),NguoiDuyet=account.Id,TrangThai="NHAP"});
        audit.Add("TaiKhoan",account.Id,"TAO_QUAN_TRI",actor:account.Id);
        await db.SaveChangesAsync(ct); await tx.CommitAsync(ct);
        return Ok(new {account.Id,account.TenDangNhap});
    }

    [AllowAnonymous, EnableRateLimiting("login"), HttpPost("dang-ky")]
    public async Task<IActionResult> Signup(SignupRequest request,CancellationToken ct)
    {
        Password(request.MatKhau);
        await using var tx=await db.Database.BeginTransactionAsync(IsolationLevel.Serializable,ct);
        ApiError.Require(await db.TaiKhoan.AnyAsync(ct),"Cần thiết lập quản trị trước.",409);
        var name=Normalize(request.TenDangNhap);
        ApiError.Require(!await db.TaiKhoan.AnyAsync(x=>x.TenDangNhap==name,ct),"Tên đăng nhập đã tồn tại.",409);
        var member=new ThanhVien {MaThanhVien="TV"+Guid.NewGuid().ToString("N")[..16],HoTen=request.HoTen,
            DienThoai=request.DienThoai,Email=request.Email,TrangThai="CHO_XAC_NHAN"};
        db.ThanhVien.Add(member); await db.SaveChangesAsync(ct);
        var account=new TaiKhoan {TenDangNhap=name,ThanhVienId=member.Id,Email=request.Email,
            VaiTroChinh="THANH_VIEN",TrangThai="HOAT_DONG",PhienBanBaoMat=1};
        account.PasswordHash=hasher.HashPassword(account,request.MatKhau);
        db.TaiKhoan.Add(account); await db.SaveChangesAsync(ct);
        audit.Add("TaiKhoan",account.Id,"DANG_KY",actor:account.Id);
        await db.SaveChangesAsync(ct); await tx.CommitAsync(ct);
        return Ok(new {account.Id,ThanhVienId=member.Id,ThongBao="Học vụ cần xác minh thành viên và quyền đại diện trước khi ghi danh."});
    }

    [AllowAnonymous, EnableRateLimiting("login"), HttpPost("dang-nhap")]
    public async Task<IActionResult> Login(LoginRequest request,CancellationToken ct)
    {
        var account=await db.TaiKhoan.SingleOrDefaultAsync(x=>x.TenDangNhap==Normalize(request.TenDangNhap),ct);
        if(account is null || account.TrangThai!="HOAT_DONG" || account.KhoaDenLuc>BusinessClock.Now
            || hasher.VerifyHashedPassword(account,account.PasswordHash,request.MatKhau)==PasswordVerificationResult.Failed)
            return Unauthorized(new {ThongBao="Tên đăng nhập hoặc mật khẩu không đúng, hoặc tài khoản đã bị khóa."});
        audit.Add("TaiKhoan",account.Id,"DANG_NHAP",actor:account.Id); await db.SaveChangesAsync(ct);
        return Ok(new {AccessToken=tokens.Issue(account),TokenType="Bearer",ExpiresIn=1800,
            account.TenDangNhap,account.VaiTroChinh,Quyen=(long)Permissions.ForRole(account.VaiTroChinh)|account.QuyenBoSung});
    }

    [Authorize, HttpGet("toi")]
    public object Me()=>new {Id=User.AccountId(),TenDangNhap=User.Identity!.Name,VaiTro=User.FindFirstValue(ClaimTypes.Role),
        ThanhVienId=User.MemberId(),GiaoVienId=User.TeacherId(),Quyen=User.FindFirstValue("permissions")};

    [Authorize, HttpPost("dang-xuat")]
    public async Task<IActionResult> Logout(CancellationToken ct)
    {
        var account=await db.TaiKhoan.SingleAsync(x=>x.Id==User.AccountId(),ct);
        account.PhienBanBaoMat++; audit.Add("TaiKhoan",account.Id,"DANG_XUAT"); await db.SaveChangesAsync(ct); return NoContent();
    }
    [Authorize, HttpPost("doi-mat-khau")]
    public async Task<IActionResult> ChangePassword(PasswordRequest request,CancellationToken ct)
    {
        Password(request.MatKhauMoi);
        var account=await db.TaiKhoan.SingleAsync(x=>x.Id==User.AccountId(),ct);
        ApiError.Require(hasher.VerifyHashedPassword(account,account.PasswordHash,request.MatKhauCu)!=PasswordVerificationResult.Failed,"Mật khẩu cũ không đúng.");
        account.PasswordHash=hasher.HashPassword(account,request.MatKhauMoi); account.PhienBanBaoMat++;
        audit.Add("TaiKhoan",account.Id,"DOI_MAT_KHAU"); await db.SaveChangesAsync(ct); return NoContent();
    }

    [Authorize(Policy="TaiKhoan"),HttpGet]
    public async Task<object> Accounts(CancellationToken ct)=>await db.TaiKhoan.AsNoTracking().OrderBy(x=>x.Id)
        .Select(x=>new {x.Id,x.TenDangNhap,x.HoTenNhanVien,x.VaiTroChinh,x.QuyenBoSung,x.GiaoVienId,x.ThanhVienId,x.TrangThai,x.RowVersion}).Take(500).ToListAsync(ct);

    [Authorize(Policy="TaiKhoan"),HttpPost]
    public async Task<IActionResult> Create(StaffAccountRequest request,CancellationToken ct)
    {
        Password(request.MatKhau); ValidateRole(request.VaiTro,request.QuyenBoSung);
        ApiError.Require(request.VaiTro!="GIAO_VIEN" || request.GiaoVienId.HasValue,"Tài khoản giáo viên phải liên kết hồ sơ giáo viên.");
        ApiError.Require(request.VaiTro!="THANH_VIEN" || request.ThanhVienId.HasValue,"Tài khoản thành viên phải liên kết thành viên.");
        var account=new TaiKhoan {TenDangNhap=Normalize(request.TenDangNhap),VaiTroChinh=request.VaiTro,
            HoTenNhanVien=request.HoTen,GiaoVienId=request.GiaoVienId,ThanhVienId=request.ThanhVienId,
            QuyenBoSung=request.QuyenBoSung,TrangThai="HOAT_DONG",PhienBanBaoMat=1};
        account.PasswordHash=hasher.HashPassword(account,request.MatKhau);
        await using var tx=await db.Database.BeginTransactionAsync(ct);
        db.TaiKhoan.Add(account); await db.SaveChangesAsync(ct); audit.Add("TaiKhoan",account.Id,"TAO_TAI_KHOAN",new {account.VaiTroChinh});
        await db.SaveChangesAsync(ct); await tx.CommitAsync(ct);return Ok(new {account.Id,account.TenDangNhap});
    }

    private static void ValidateRole(string role,long bits)=>ApiError.Require(Permissions.Roles.Contains(role)&&bits>=0&&(bits&~255L)==0,"Vai trò hoặc quyền bổ sung không hợp lệ.");
    [Authorize(Policy="TaiKhoan"),HttpPut("{id:long:min(1)}/quyen")]
    public async Task<IActionResult> Role(long id,AccountRoleRequest request,CancellationToken ct)
    {
        ValidateRole(request.VaiTro,request.QuyenBoSung);
        ApiError.Require(User.IsInRole("QUAN_TRI"),"Chỉ quản trị được cấp quyền.",403);
        await using var tx=await db.Database.BeginTransactionAsync(IsolationLevel.Serializable,ct);
        var account=await db.TaiKhoan.SingleOrDefaultAsync(x=>x.Id==id,ct)??throw new ApiError(404,"Không tìm thấy tài khoản.");
        ApiError.Require(request.VaiTro!="GIAO_VIEN"||account.GiaoVienId.HasValue,"Thiếu hồ sơ giáo viên.");
        ApiError.Require(request.VaiTro!="THANH_VIEN"||account.ThanhVienId.HasValue,"Thiếu hồ sơ thành viên.");
        if(account.VaiTroChinh=="QUAN_TRI"&&account.TrangThai=="HOAT_DONG"&&(!request.HoatDong||request.VaiTro!="QUAN_TRI"))
            ApiError.Require(await db.TaiKhoan.CountAsync(x=>x.VaiTroChinh=="QUAN_TRI"&&x.TrangThai=="HOAT_DONG",ct)>1,"Phải giữ ít nhất một quản trị hoạt động.",409);
        account.VaiTroChinh=request.VaiTro;account.QuyenBoSung=request.QuyenBoSung;
        account.TrangThai=request.HoatDong?"HOAT_DONG":"KHOA";account.PhienBanBaoMat++;
        audit.Add("TaiKhoan",id,"DOI_QUYEN",new {request.VaiTro,request.QuyenBoSung,request.HoatDong});
        await db.SaveChangesAsync(ct);await tx.CommitAsync(ct);return NoContent();
    }
    [Authorize(Policy="TaiKhoan"),HttpPost("{id:long:min(1)}/dat-lai-mat-khau")]
    public async Task<IActionResult> ResetPassword(long id,ResetPasswordRequest r,CancellationToken ct)
    {
        Password(r.MatKhauMoi);
        var account=await db.TaiKhoan.SingleOrDefaultAsync(x=>x.Id==id,ct)??throw new ApiError(404,"Không có tài khoản.");
        account.PasswordHash=hasher.HashPassword(account,r.MatKhauMoi);account.PhienBanBaoMat++;
        audit.Add("TaiKhoan",id,"DAT_LAI_MAT_KHAU",new {r.LyDo});await db.SaveChangesAsync(ct);return NoContent();
    }

}
