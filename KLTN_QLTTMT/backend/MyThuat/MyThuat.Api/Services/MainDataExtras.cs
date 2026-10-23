using System.Data;
using Microsoft.EntityFrameworkCore;
using MyThuat.Api.Data;
using MyThuat.Api.Models;
namespace MyThuat.Api.Services;
public static class MainDataExtras
{
    public static async Task Seed(MyThuatDbContext db){
        if(await db.LopHoc.AnyAsync(x=>x.MaLop=="MAIN_L13"))return;
        await using var tx=await db.Database.BeginTransactionAsync(IsolationLevel.Serializable);var now=BusinessClock.Now;var today=BusinessClock.Today;var admin=await db.TaiKhoan.SingleAsync(x=>x.TenDangNhap=="quantri01");
        var room=await db.PhongHoc.SingleAsync(x=>x.MaPhong=="MAIN_P02");var teacher=await db.GiaoVien.SingleAsync(x=>x.MaGiaoVien=="MAIN_GV02");var assistant=await db.GiaoVien.SingleAsync(x=>x.MaGiaoVien=="MAIN_GV07");
        var courses=await db.KhoaHoc.Where(x=>x.MaKhoa=="DEMO_KH01"||x.MaKhoa=="DEMO_KH02").OrderBy(x=>x.MaKhoa).ToListAsync();
        LopHoc? makeupClass=null;
        for(var i=0;i<2;i++){
            var course=courses[i];var day=today.AddDays(10);var weekday=i==0?DayOfWeek.Monday:DayOfWeek.Thursday;while(day.DayOfWeek!=weekday)day=day.AddDays(1);var hour=i==0?16:18;
            var cls=new LopHoc{MaLop=i==0?"MAIN_L13":"MAIN_L14",TenLop=course.TenKhoa+(i==0?" · Thứ Hai 16h":" · Thứ Năm 18h"),KhoaHocId=course.Id,NgayKhaiGiangDuKien=day,NgayKetThucDuKien=day.AddDays((course.SoBuoi-1)*7),SoBuoiKeHoach=course.SoBuoi,SiSoToiThieu=4,SiSoToiDa=i==0?(short)8:(short)10,HocPhiApDung=course.HocPhi,MoDangKyLuc=now.AddDays(-2),DongDangKyLuc=day.ToDateTime(new TimeOnly(hour,0)).AddHours(-48),TrangThai="DANG_TUYEN_SINH",NguoiDuyet=admin.Id,LichHocDuKien=$"{(i==0?"Thứ Hai":"Thứ Năm")} · {hour:00}:00–{day.ToDateTime(new TimeOnly(hour,0)).AddMinutes(course.SoTietMoiBuoi*30):HH:mm} · Hàng tuần"};db.LopHoc.Add(cls);await db.SaveChangesAsync();
            var topics=await db.NoiDungBuoi.Where(x=>x.KhoaHocId==course.Id).OrderBy(x=>x.ThuTu).ToListAsync();foreach(var topic in topics){var at=day.AddDays((topic.ThuTu-1)*7).ToDateTime(new TimeOnly(hour,0));var b=new BuoiHoc{LopHocId=cls.Id,NoiDungBuoiId=topic.Id,PhongHocId=room.Id,ThuTuTrongLop=topic.ThuTu,LanXepLich=1,BatDau=at,KetThuc=at.AddMinutes(topic.SoTiet*30),SoTiet=topic.SoTiet,PhutMoiTiet=30,TrangThai="DA_XEP_LICH"};db.BuoiHoc.Add(b);await db.SaveChangesAsync();db.GiangDayBuoi.Add(new(){BuoiHocId=b.Id,GiaoVienId=teacher.Id,VaiTro="CHINH",SoTietThucDay=b.SoTiet,NguoiDuyet=admin.Id});if(i==0)db.GiangDayBuoi.Add(new(){BuoiHocId=b.Id,GiaoVienId=assistant.Id,VaiTro="TRO_GIANG",SoTietThucDay=b.SoTiet,NguoiDuyet=admin.Id});}
            db.PhanCong.Add(new(){LopHocId=cls.Id,GiaoVienId=teacher.Id,VaiTroTrongLop="CHINH",TuNgay=day,DenNgay=cls.NgayKetThucDuKien,XacNhanDuChuyenMon=true,XacNhanNhanLopLuc=now,NguoiDuyet=admin.Id});if(i==0)db.PhanCong.Add(new(){LopHocId=cls.Id,GiaoVienId=assistant.Id,VaiTroTrongLop="TRO_GIANG",TuNgay=day,DenNgay=cls.NgayKetThucDuKien,XacNhanDuChuyenMon=true,XacNhanNhanLopLuc=now,NguoiDuyet=admin.Id});else makeupClass=cls;await db.SaveChangesAsync();
        }
        var absent=await db.DiemDanh.Include(x=>x.DangKy).ThenInclude(x=>x!.LopHoc).FirstAsync(x=>x.DangKy!.LopHoc!.MaLop=="MAIN_L08"&&x.TrangThai=="VANG");var target=await db.BuoiHoc.FirstAsync(x=>x.LopHocId==makeupClass!.Id&&x.NoiDungBuoiId==absent.NoiDungDuocTinhId);var extra=new DiemDanh{DangKyId=absent.DangKyId,BuoiHocId=target.Id,NoiDungDuocTinhId=target.NoiDungBuoiId,LoaiThamGia="HOC_BU",TrangThai="CHO_THAM_GIA"};db.DiemDanh.Add(extra);await db.SaveChangesAsync();db.HocBu.Add(new(){DiemDanhVangId=absent.Id,DiemDanhBuId=extra.Id,HanHoanTat=absent.DangKy!.LopHoc!.NgayKetThucDuKien!.Value.AddDays(30).ToDateTime(TimeOnly.MaxValue),NguoiDuyet=admin.Id,TrangThai="DA_DAT",LyDo="Lịch bù giả lập cùng nội dung, không trùng lịch khác."});
        db.KhuyenMai.Add(new(){MaKhuyenMai="HOACU10",TenKhuyenMai="Bộ cá nhân giảm 10%",NhomKhoanThu="HOA_CU",LoaiGiam="PHAN_TRAM",GiaTriGiam=10,TranGiam=50000,TongLuot=100,BatDau=now.AddDays(-1),KetThuc=now.AddDays(90),TrangThai="HOAT_DONG"});
        await db.SaveChangesAsync();await MainDataValidation.Check(db);await tx.CommitAsync();Console.WriteLine("Đã thêm lớp Junior còn chỗ để thử JUNIOR10, lớp Foundation và lịch học bù; thêm mã HOACU10 cho khoản họa cụ.");
    }
}
