using Microsoft.EntityFrameworkCore;
using MyThuat.Api.Data;
namespace MyThuat.Api.Services;
public static class MainDataValidation
{
    public static async Task Check(MyThuatDbContext db){
        var now=BusinessClock.Now;var classes=await db.LopHoc.Where(x=>x.MaLop.StartsWith("MAIN_")).ToListAsync();
        var ids=classes.Select(x=>x.Id).ToArray();var registrations=await db.DangKy.Include(x=>x.HoSoTheoHoc).ThenInclude(x=>x!.HocVien).Include(x=>x.LopHoc).ThenInclude(x=>x!.KhoaHoc).Where(x=>ids.Contains(x.LopHocId)).ToListAsync();
        foreach(var cls in classes){var occupied=registrations.Count(x=>x.LopHocId==cls.Id&&(x.TrangThai=="DA_XAC_NHAN"||x.TrangThai=="CHO_THANH_TOAN"&&x.HanGiuCho>now));if(occupied>cls.SiSoToiDa)throw new InvalidOperationException("Vượt sĩ số: "+cls.MaLop);}
        foreach(var row in registrations){var birth=row.HoSoTheoHoc!.HocVien!.NgaySinh;var start=row.LopHoc!.NgayKhaiGiangDuKien;var age=start.Year-birth.Year-(birth.AddYears(start.Year-birth.Year)>start?1:0);if(age<(row.LopHoc.KhoaHoc!.TuoiToiThieu??0))throw new InvalidOperationException("Tuổi không hợp lệ "+row.MaDangKy);var charges=await db.KhoanThuDangKy.Where(x=>x.DangKyId==row.Id).ToListAsync();foreach(var c in charges){if(c.ThanhTienChot!=c.SoLuong*c.DonGiaChot-c.TienGiamChot)throw new InvalidOperationException("Sai khoản thu "+row.Id);var receipts=await db.GiaoDichTien.Where(x=>x.KhoanThuId==c.Id&&x.TrangThai=="DA_XAC_NHAN").ToListAsync();var paid=receipts.Sum(x=>x.LoaiGiaoDich=="THU"?x.SoTien:-x.SoTien);if(row.TrangThai=="DA_XAC_NHAN"&&paid<c.ThanhTienChot)throw new InvalidOperationException("Đã xác nhận nhưng chưa đủ tiền "+row.Id);}}
        var sessions=await db.BuoiHoc.Where(x=>ids.Contains(x.LopHocId)&&x.TrangThai!="DA_HUY").ToListAsync();
        foreach(var a in sessions)if(sessions.Any(b=>a.Id!=b.Id&&a.PhongHocId==b.PhongHocId&&a.BatDau<b.KetThuc.AddMinutes(15)&&b.BatDau<a.KetThuc.AddMinutes(15)))throw new InvalidOperationException("Trùng phòng bộ mới");
        var staff=await db.GiangDayBuoi.Where(x=>ids.Contains(x.BuoiHoc!.LopHocId)).ToListAsync();foreach(var a in staff){var sa=sessions.Single(s=>s.Id==a.BuoiHocId);foreach(var b in staff.Where(b=>b.Id!=a.Id&&b.GiaoVienId==a.GiaoVienId)){var sb=sessions.Single(s=>s.Id==b.BuoiHocId);if(sa.BatDau<sb.KetThuc.AddMinutes(15)&&sb.BatDau<sa.KetThuc.AddMinutes(15))throw new InvalidOperationException("Trùng giáo viên bộ mới");}}
        var attendance=await db.DiemDanh.Include(x=>x.DangKy).ThenInclude(x=>x!.HoSoTheoHoc).Include(x=>x.BuoiHoc).Where(x=>ids.Contains(x.DangKy!.LopHocId)&&x.TrangThai=="CHO_THAM_GIA").ToListAsync();foreach(var a in attendance)if(attendance.Any(b=>a.Id!=b.Id&&a.DangKy!.HoSoTheoHoc!.HocVienId==b.DangKy!.HoSoTheoHoc!.HocVienId&&a.BuoiHoc!.BatDau<b.BuoiHoc!.KetThuc&&b.BuoiHoc.BatDau<a.BuoiHoc.KetThuc))throw new InvalidOperationException("Trùng lịch học viên");
        Console.WriteLine($"Kiểm tra MAIN: {classes.Count} lớp, {registrations.Count} đăng ký; sĩ số, tuổi, tài chính và lịch hợp lệ.");
    }
}
