using Microsoft.EntityFrameworkCore;
using MyThuat.Api.Data;
using MyThuat.Api.Models;
namespace MyThuat.Api.Services;
public sealed class NotificationService(MyThuatDbContext db)
{
    // Enqueued in the same transaction as the business change; SMTP never blocks saving a class.
    public void Add(long? member,string? email,string? phone,string kind,string title,string text)=>db.Set<ThongBao>().Add(new(){
        ThanhVienId=member,Email=email?.Trim(),DienThoai=phone?.Trim(),Loai=kind,TieuDe=title,NoiDung=text,
        TrangThaiEmail=string.IsNullOrWhiteSpace(email)||email.EndsWith(".invalid",StringComparison.OrdinalIgnoreCase)?"KHONG_CO_EMAIL":"CHO_GUI"});
    public async Task ScheduleChanged(long oldSession,DateTime oldStart,BuoiHoc next,string className,string reason,CancellationToken ct)
    {
        var students=await db.DiemDanh.Where(x=>x.BuoiHocId==next.Id&&x.TrangThai!="HUY_BO"&&x.DangKy!.TrangThai=="DA_XAC_NHAN")
            .Select(x=>x.DangKy!.HoSoTheoHoc!.HocVienId).Distinct().ToListAsync(ct);
        var now=BusinessClock.Now;
        var contacts=await db.DaiDienHocVien.Where(x=>students.Contains(x.HocVienId)&&x.XacNhanLuc!=null&&x.HieuLucTu<=now
            &&(x.HieuLucDen==null||x.HieuLucDen>now)&&(x.QuyenDaiDien=="DAY_DU"||x.QuyenDaiDien=="XEM_KET_QUA")&&x.ThanhVien!.TrangThai=="HOAT_DONG")
            .Select(x=>new{x.ThanhVienId,x.ThanhVien!.Email,x.ThanhVien.DienThoai}).Distinct().ToListAsync(ct);
        var room=await db.PhongHoc.Where(x=>x.Id==next.PhongHocId).Select(x=>x.TenPhong).SingleAsync(ct);
        foreach(var c in contacts)Add(c.ThanhVienId,c.Email,c.DienThoai,"DOI_LICH","Thông báo đổi lịch · "+className,
            $"Lớp: {className}\nLịch cũ: {oldStart:dd/MM/yyyy HH:mm}\nLịch mới: {next.BatDau:dd/MM/yyyy HH:mm}–{next.KetThuc:HH:mm} (giờ Việt Nam)\nPhòng: {room}\nLý do: {reason}\nVui lòng mở Góc học tập trên website để xem lịch cập nhật. Nếu không thể tham gia, hãy liên hệ trung tâm.");
    }
}
