using Microsoft.EntityFrameworkCore;
using MyThuat.Api.Data;
using MyThuat.Api.Models;
namespace MyThuat.Api.Services;
public static class PaymentDemoSetup
{
    public static async Task Run(MyThuatDbContext db,string root)
    {
        await MainDatabaseSetup.Backup(db,root);
        await db.Database.ExecuteSqlRawAsync(await File.ReadAllTextAsync(Path.Combine(root,"Database","012_ThanhToanOnline.sql")));
        await using var tx=await db.Database.BeginTransactionAsync();
        var rooms=await db.PhongHoc.Where(x=>x.MaPhong.StartsWith("MAIN_P")).ToListAsync();
        foreach(var room in rooms)room.TenPhong=room.TenPhong.Split('—','·')[0].Trim()+" · 36 Tân Thắng";
        var cls=await db.LopHoc.SingleOrDefaultAsync(x=>x.MaLop=="TRAI_NGHIEM_0D");
        if(cls==null){
            var type=await db.LoaiKhoaHoc.FirstOrDefaultAsync(x=>x.TrangThai=="HOAT_DONG")??throw new InvalidOperationException("Chưa có loại khóa học.");
            var room=rooms.FirstOrDefault(x=>x.TrangThai=="HOAT_DONG")??await db.PhongHoc.FirstAsync(x=>x.TrangThai=="HOAT_DONG");
            var teacher=await db.GiaoVien.FirstAsync(x=>x.TrangThai=="HOAT_DONG"&&x.CapDoCoTheDay.Contains("JUNIOR"));
            var admin=await db.TaiKhoan.FirstAsync(x=>x.VaiTroChinh=="QUAN_TRI"&&x.TrangThai=="HOAT_DONG");
            var policy=PolicySettings.Parse((await db.QuyDinh.Where(x=>x.TrangThai=="HOAT_DONG"&&x.HieuLucTu<=BusinessClock.Now&&(x.HieuLucDen==null||x.HieuLucDen>BusinessClock.Now)).OrderByDescending(x=>x.SoPhienBan).FirstAsync()).NoiDungJson);
            var course=new KhoaHoc{MaKhoa="TRAI_NGHIEM",TenKhoa="Trải nghiệm hội họa",LoaiKhoaHocId=type.Id,PhienBan=1,CapDo="JUNIOR",TuoiToiThieu=3,MucTieu="Làm quen màu sắc và trải nghiệm một buổi vẽ tại Bụi.",SoBuoi=1,SoTietMoiBuoi=2,PhutMoiTiet=30,HocPhi=0,HieuLucTu=BusinessClock.Today,TrangThai="HOAT_DONG"};db.Add(course);await db.SaveChangesAsync();
            var start=BusinessClock.Today.AddDays(21).ToDateTime(new TimeOnly(15,0));
            // Find a genuinely free room slot rather than publishing an overlapping trial session.
            var found=false;for(var i=0;i<90;i++){var end=start.AddHours(1);if(!policy.NgayNghi.Contains(DateOnly.FromDateTime(start))&&!await db.BuoiHoc.AnyAsync(x=>x.PhongHocId==room.Id&&x.TrangThai!="DA_HUY"&&x.BatDau<end.AddMinutes(policy.NghiGiuaCaPhut)&&x.KetThuc>start.AddMinutes(-policy.NghiGiuaCaPhut))&&!await db.GiangDayBuoi.AnyAsync(x=>x.GiaoVienId==teacher.Id&&x.BuoiHoc!.TrangThai!="DA_HUY"&&x.BuoiHoc.BatDau<end.AddMinutes(policy.NghiGiuaCaPhut)&&x.BuoiHoc.KetThuc>start.AddMinutes(-policy.NghiGiuaCaPhut))){found=true;break;}start=start.AddDays(1);}if(!found)throw new InvalidOperationException("Không tìm được khung trải nghiệm trống.");
            cls=new LopHoc{MaLop="TRAI_NGHIEM_0D",TenLop="Lớp trải nghiệm 0đ",KhoaHocId=course.Id,NgayKhaiGiangDuKien=DateOnly.FromDateTime(start),NgayKetThucDuKien=DateOnly.FromDateTime(start),SoBuoiKeHoach=1,LichHocDuKien=$"{start:dd/MM/yyyy} · 15:00–16:00",SiSoToiThieu=1,SiSoToiDa=(short)Math.Min(10,(int)room.SucChua),HocPhiApDung=0,MoDangKyLuc=BusinessClock.Now.AddMinutes(-1),DongDangKyLuc=start.AddHours(-48),TrangThai="DANG_TUYEN_SINH"};db.Add(cls);
            var lesson=new NoiDungBuoi{KhoaHocId=course.Id,ThuTu=1,ChuDe="Trải nghiệm sắc màu",NoiDung="Làm quen màu sắc, cọ vẽ và hoàn thiện tranh nhỏ.",SoTiet=2,YeuCauSanPham="Một tranh thực hành tại lớp."};db.Add(lesson);await db.SaveChangesAsync();
            var session=new BuoiHoc{LopHocId=cls.Id,NoiDungBuoiId=lesson.Id,PhongHocId=room.Id,ThuTuTrongLop=1,LanXepLich=1,BatDau=start,KetThuc=start.AddHours(1),SoTiet=2,PhutMoiTiet=30,TrangThai="DA_XEP_LICH"};db.Add(session);await db.SaveChangesAsync();
            db.GiangDayBuoi.Add(new(){BuoiHocId=session.Id,GiaoVienId=teacher.Id,VaiTro="CHINH",SoTietThucDay=2,NguoiDuyet=admin.Id});
            db.PhanCong.Add(new(){LopHocId=cls.Id,GiaoVienId=teacher.Id,VaiTroTrongLop="CHINH",TuNgay=DateOnly.FromDateTime(start),DenNgay=DateOnly.FromDateTime(start),XacNhanDuChuyenMon=true,XacNhanNhanLopLuc=BusinessClock.Now,NguoiDuyet=admin.Id});
            db.GioiThieuKhoaHoc.Add(new(){KhoaHocId=course.Id,AnhDaiDien="/bui/children.webp",DauRaJson="[\"Làm quen màu sắc và hoạt động tại lớp\"]",HoaCuBaoGom="Vật tư thực hành trong buổi trải nghiệm.",CanChuanBi="Trang phục thuận tiện.",ThanhPhamJson="[]"});
        }
        await db.SaveChangesAsync();await tx.CommitAsync();Console.WriteLine("Đã chuẩn hóa địa chỉ và bổ sung lớp trải nghiệm miễn phí. Không tạo giao dịch SePay.");
    }
}
