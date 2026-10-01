using System.Security.Claims;
using Microsoft.EntityFrameworkCore;
using MyThuat.Api.Data;
using MyThuat.Api.Security;
namespace MyThuat.Api.Services;
public sealed class AccessService(MyThuatDbContext db, IHttpContextAccessor accessor)
{
    public ClaimsPrincipal User => accessor.HttpContext!.User;
    public void Need(Permission permission) => ApiError.Require(User.Has(permission), "Bạn không có quyền thực hiện thao tác này.", 403);
    public async Task Student(long id, bool forRegistration, CancellationToken ct)
    {
        if (User.Has(Permission.HoSo)) return;
        var member = User.MemberId();
        ApiError.Require(member.HasValue, "Không có quyền với học viên này.", 403);
        var now = BusinessClock.Now;
        var link = await db.DaiDienHocVien.AsNoTracking().SingleOrDefaultAsync(x => x.ThanhVienId == member
            && x.HocVienId == id && x.HieuLucTu <= now && (x.HieuLucDen == null || x.HieuLucDen > now) && x.XacNhanLuc != null, ct);
        ApiError.Require(link is not null && (forRegistration ? link.QuyenDaiDien == "DAY_DU" || link.QuyenDaiDien == "DANG_KY"
            : link.QuyenDaiDien == "DAY_DU" || link.QuyenDaiDien == "XEM_KET_QUA"), "Không có quyền đại diện tương ứng.", 403);
    }
    public async Task TeacherSession(long sessionId, CancellationToken ct)
    {
        if (User.Has(Permission.DaoTao)) return;
        ApiError.Require(User.Has(Permission.DiemDanh) && User.TeacherId() is long, "Không có quyền với buổi học.",403);
        ApiError.Require(await db.GiangDayBuoi.AnyAsync(x => x.BuoiHocId == sessionId && x.GiaoVienId == User.TeacherId(), ct),
            "Bạn không được phân công dạy buổi này.", 403);
    }
}
