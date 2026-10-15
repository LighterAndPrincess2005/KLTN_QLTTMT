using System.Data;
using System.Security.Cryptography;
using System.Text.Json;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using MyThuat.Api.Data;
using MyThuat.Api.Models;

namespace MyThuat.Api.Services;

// Opt-in local demonstration data. Never runs during normal API startup.
public static class DemoLearningAccounts
{
    public static async Task Seed(MyThuatDbContext db,string contentRoot)
    {
        var local=Path.Combine(contentRoot,".local");Directory.CreateDirectory(local);
        var credentials=Path.Combine(local,"Tai_khoan_hoc_tap_mau.txt");
        string[] names=["gv_lhl_demo","hv_baoan_demo","hv_minhkhang_demo","hv_khanhlinh_demo"];
        await using var tx=await db.Database.BeginTransactionAsync(IsolationLevel.Serializable);
        if(await db.KhoaHoc.AnyAsync(x=>x.MaKhoa=="DEMO_LT_KH"))
        {
            if(await db.TaiKhoan.CountAsync(x=>names.Contains(x.TenDangNhap))!=4||!File.Exists(credentials))
                throw new InvalidOperationException("Bộ mẫu đã có nhưng thiếu tài khoản/file mật khẩu. Không tự tạo trùng hoặc đổi mật khẩu.");
            Console.WriteLine("Bộ tài khoản học tập đã có. Giữ nguyên tài khoản, mật khẩu và dữ liệu đã thử.");return;
        }
        if(await db.TaiKhoan.AnyAsync(x=>names.Contains(x.TenDangNhap)))throw new InvalidOperationException("Tên tài khoản mẫu đã tồn tại. Không ghi đè tài khoản.");
        var admin=await db.TaiKhoan.Where(x=>x.VaiTroChinh=="QUAN_TRI"&&x.TrangThai=="HOAT_DONG").OrderBy(x=>x.Id).FirstOrDefaultAsync()
            ??throw new InvalidOperationException("Cần quản trị hoạt động trước khi nạp tài khoản mẫu.");
        var parent=await db.ThanhVien.SingleOrDefaultAsync(x=>x.MaThanhVien=="DEMO_TV01"&&x.TrangThai=="HOAT_DONG")
            ??throw new InvalidOperationException("Cần bộ mẫu từ Word đã nạp trước.");
        var pupils=await db.HocVien.Where(x=>x.MaHocVien=="DEMO_HV01"||x.MaHocVien=="DEMO_HV02"||x.MaHocVien=="DEMO_HV03").OrderBy(x=>x.MaHocVien).ToListAsync();
        if(pupils.Count!=3||pupils.Any(x=>x.TrangThai!="HOAT_DONG"))throw new InvalidOperationException("Cần đủ ba học viên mẫu đã xác minh.");
        foreach(var pupil in pupils)
            if(!await db.DaiDienHocVien.AnyAsync(x=>x.ThanhVienId==parent.Id&&x.HocVienId==pupil.Id&&x.XacNhanLuc!=null&&x.QuyenDaiDien=="DAY_DU"&&x.HieuLucDen==null))
                throw new InvalidOperationException("Phụ huynh mẫu chưa có quyền đại diện phù hợp.");
        var template=await db.QuyDinh.OrderBy(x=>x.Id).FirstOrDefaultAsync()??throw new InvalidOperationException("Cần bộ quy định để chụp chính sách kiểm thử.");
        _=PolicySettings.Parse(template.NoiDungJson);
        var type=await db.LoaiKhoaHoc.SingleAsync(x=>x.MaLoai=="DEMO_CORE");
        var now=BusinessClock.Now;var yesterday=BusinessClock.Today.AddDays(-1);
        var teacher=new GiaoVien{MaGiaoVien="DEMO_LT_GV",HoTen="Giáo viên LHL Art (kiểm thử)",DienThoai="0000000000",TrinhDo="Hồ sơ mẫu",ChuyenMon="Mỹ thuật sáng tạo",CapDoCoTheDay="TRAI_NGHIEM",TrangThai="HOAT_DONG"};
        var room=new PhongHoc{MaPhong="DEMO_LT_P",TenPhong="Phòng kiểm thử LHL Art",SucChua=6,TrangThai="HOAT_DONG"};
        var rule=new QuyDinh{MaBoQuyDinh="DEMO_LT_QD",SoPhienBan=1,HieuLucTu=yesterday.ToDateTime(TimeOnly.MinValue),NoiDungJson=template.NoiDungJson,NguoiDuyet=admin.Id,TrangThai="NHAP"};
        // Draft catalog/policy keep this opt-in fixture out of public sales and global policy selection.
        var course=new KhoaHoc{MaKhoa="DEMO_LT_KH",TenKhoa="Trải nghiệm mỹ thuật (kiểm thử miễn phí)",LoaiKhoaHocId=type.Id,PhienBan=1,CapDo="TRAI_NGHIEM",MucTieu="Dữ liệu mẫu để kiểm thử góc học tập và giáo viên; không phải khóa bán cho khách.",SoBuoi=3,SoTietMoiBuoi=4,PhutMoiTiet=30,HocPhi=0,HieuLucTu=yesterday,TrangThai="NHAP"};
        db.AddRange(teacher,room,rule,course);await db.SaveChangesAsync();
        var password=Environment.GetEnvironmentVariable("MYTHUAT_DEMO_LEARNING_PASSWORD")??"LhlDemo"+Convert.ToHexString(RandomNumberGenerator.GetBytes(6))+"!";
        if(password.Length<12||password.Length>128||!password.Any(char.IsLetter)||!password.Any(char.IsDigit))throw new InvalidOperationException("Mật khẩu mẫu cần 12–128 ký tự, có chữ và số.");
        var hasher=new PasswordHasher<TaiKhoan>();
        TaiKhoan Account(string name,string role,long? member,long? teacherId)
        {
            var account=new TaiKhoan{TenDangNhap=name,VaiTroChinh=role,ThanhVienId=member,GiaoVienId=teacherId,HoTenNhanVien=role=="GIAO_VIEN"?teacher.HoTen:null,TrangThai="HOAT_DONG",PhienBanBaoMat=1};
            account.PasswordHash=hasher.HashPassword(account,password);db.TaiKhoan.Add(account);return account;
        }
        var teacherAccount=Account(names[0],"GIAO_VIEN",null,teacher.Id);await db.SaveChangesAsync();
        var topics=new[]{"Quan sát và phác hình","Phối màu sáng tạo","Hoàn thiện bài thực hành"}.Select((title,i)=>new NoiDungBuoi{KhoaHocId=course.Id,ThuTu=(short)(i+1),ChuDe=title,NoiDung="Buổi minh họa để kiểm thử dữ liệu học tập.",SoTiet=4,YeuCauSanPham="Bài thực hành minh họa."}).ToArray();
        db.NoiDungBuoi.AddRange(topics);await db.SaveChangesAsync();
        var currentStart=now.AddMinutes(-Math.Min(120,(now-now.Date).TotalMinutes));
        var starts=new[]{yesterday.ToDateTime(new TimeOnly(9,0)),currentStart,BusinessClock.Today.AddDays(7).ToDateTime(new TimeOnly(9,0))};
        LopHoc MakeClass(string code,string title,DateTime first,DateTime last)=>new(){MaLop=code,TenLop=title,KhoaHocId=course.Id,NgayKhaiGiangDuKien=DateOnly.FromDateTime(first),NgayKhaiGiangThucTe=DateOnly.FromDateTime(first),NgayKetThucDuKien=DateOnly.FromDateTime(last),SoBuoiKeHoach=3,LichHocDuKien="Lịch riêng trong dữ liệu kiểm thử",SiSoToiThieu=1,SiSoToiDa=6,HocPhiApDung=0,MoDangKyLuc=first.AddDays(-7),DongDangKyLuc=first.AddDays(-1),NguoiDuyet=admin.Id,TrangThai="DANG_HOC"};
        var main=MakeClass("DEMO_LT_LOP","Lớp góc học tập LHL (kiểm thử)",starts[0],starts[2]);
        var makeupStart=BusinessClock.Today.AddDays(2).ToDateTime(new TimeOnly(14,0));
        var makeupClass=MakeClass("DEMO_LT_BU","Lớp học bù LHL (kiểm thử)",makeupStart,makeupStart.AddDays(14));
        db.LopHoc.AddRange(main,makeupClass);await db.SaveChangesAsync();
        async Task<BuoiHoc[]> Schedule(LopHoc cls,DateTime[] times)
        {
            var sessions=times.Select((start,i)=>new BuoiHoc{LopHocId=cls.Id,NoiDungBuoiId=topics[i].Id,PhongHocId=room.Id,ThuTuTrongLop=(short)(i+1),LanXepLich=1,BatDau=start,KetThuc=start.AddMinutes(120),SoTiet=4,PhutMoiTiet=30,TrangThai=cls.Id==main.Id&&i==0?"DA_TO_CHUC":"DA_XEP_LICH"}).ToArray();
            db.BuoiHoc.AddRange(sessions);await db.SaveChangesAsync();
            foreach(var session in sessions)db.GiangDayBuoi.Add(new(){BuoiHocId=session.Id,GiaoVienId=teacher.Id,VaiTro="CHINH",SoTietThucDay=4,NguoiDuyet=admin.Id});
            db.PhanCong.Add(new(){LopHocId=cls.Id,GiaoVienId=teacher.Id,VaiTroTrongLop="CHINH",TuNgay=DateOnly.FromDateTime(times[0]),DenNgay=DateOnly.FromDateTime(times[^1]),XacNhanDuChuyenMon=true,XacNhanNhanLopLuc=now,NguoiDuyet=admin.Id});
            await db.SaveChangesAsync();return sessions;
        }
        var sessions=await Schedule(main,starts);
        var makeupSessions=await Schedule(makeupClass,[makeupStart,makeupStart.AddDays(7),makeupStart.AddDays(14)]);
        var enrollmentIds=new List<long>();
        for(var i=0;i<pupils.Count;i++)
        {
            var pupil=pupils[i];
            var member=new ThanhVien{MaThanhVien=$"DEMO_LT_TV{i+1}",HoTen=pupil.HoTen,DienThoai="0000000000",TrangThai="HOAT_DONG",XacNhanThanhVienLuc=now};
            db.ThanhVien.Add(member);await db.SaveChangesAsync();
            Account(names[i+1],"THANH_VIEN",member.Id,null);
            db.DaiDienHocVien.Add(new(){ThanhVienId=member.Id,HocVienId=pupil.Id,QuanHe="TU_BAN_THAN",QuyenDaiDien="XEM_KET_QUA",LaLienHeChinh=false,HieuLucTu=now,XacNhanLuc=now,NguoiXacNhan=admin.Id});
            var profile=new HoSoTheoHoc{HocVienId=pupil.Id,KhoaHocId=course.Id,QuyDinhId=rule.Id,NgayBatDau=yesterday,TrangThai="DANG_THEO_HOC"};
            db.HoSoTheoHoc.Add(profile);await db.SaveChangesAsync();
            var enrolled=new DangKy{MaDangKy=$"DEMO_LT_DK{i+1}",HoSoTheoHocId=profile.Id,LopHocId=main.Id,NguoiDangKyId=parent.Id,LapLuc=now,HanGiuCho=now,HieuLucTu=now,HocPhiGocChot=0,TrangThai="DA_XAC_NHAN",IdempotencyKey=$"demo-learning-enroll-{i+1}",RequestFingerprint=$"DEMO_LEARNING_{i+1}"};
            db.DangKy.Add(enrolled);await db.SaveChangesAsync();enrollmentIds.Add(enrolled.Id);
            db.KhoanThuDangKy.Add(new(){DangKyId=enrolled.Id,LoaiKhoan="HOC_PHI",MoTaChot="Lớp kiểm thử miễn phí — không có giao dịch thu tiền",SoLuong=1,DonGiaChot=0,ThanhTienChot=0});
            var attendance=sessions.Select((session,n)=>new DiemDanh{DangKyId=enrolled.Id,BuoiHocId=session.Id,NoiDungDuocTinhId=topics[n].Id,LoaiThamGia="HOC_CHINH",TrangThai=n==0?(i==1?"VANG":"CO_MAT"):"CHO_THAM_GIA",PhutThamDu=n==0?(short?)(i==1?0:120):null,DiemSanPham=n==0&&i!=1?8m+i/2m:null,NhanXet=n==0?(i==1?"Vắng có báo trước (dữ liệu kiểm thử).":"Nhận xét mẫu: quan sát tốt, cần luyện thêm nét và bố cục."):null,BaoNghiLuc=n==0&&i==1?session.BatDau.AddHours(-24):null,NguoiGhi=n==0?teacherAccount.Id:null,GhiLuc=n==0?session.KetThuc:null}).ToArray();
            db.DiemDanh.AddRange(attendance);await db.SaveChangesAsync();
            if(i==1)
            {
                var extra=new DiemDanh{DangKyId=enrolled.Id,BuoiHocId=makeupSessions[0].Id,NoiDungDuocTinhId=topics[0].Id,LoaiThamGia="HOC_BU",TrangThai="CHO_THAM_GIA"};db.DiemDanh.Add(extra);await db.SaveChangesAsync();
                var settings=PolicySettings.Parse(rule.NoiDungJson);
                db.HocBu.Add(new(){DiemDanhVangId=attendance[0].Id,DiemDanhBuId=extra.Id,HanHoanTat=starts[^1].AddDays(settings.HanHocBuNgay),NguoiDuyet=admin.Id,TrangThai="DA_DAT",LyDo="Lịch bù minh họa trong bộ kiểm thử."});
            }
            db.PhanHoi.Add(new(){DangKyId=enrolled.Id,NguoiGuiId=parent.Id,NoiDung="Phản hồi minh họa: học viên cần luyện thêm phần nào?",GuiLuc=now,HanPhanHoi=now.AddDays(2),NguoiXuLy=admin.Id,TraLoi="Trung tâm trả lời mẫu: xem nhận xét của giáo viên theo từng buổi để biết nội dung cần luyện.",TraLoiLuc=now,TrangThai="DA_TRA_LOI"});
        }
        db.NhatKyThayDoi.Add(new(){TaiKhoanId=admin.Id,LoaiDoiTuong="DemoLearningAccounts",IdDoiTuong=main.Id,HanhDong="THEM_MAU",Kenh="CLI",ThoiDiem=now,CorrelationId=Guid.NewGuid().ToString("N"),SauJson=JsonSerializer.Serialize(new{Accounts=names,Course=course.MaKhoa,Classes=new[]{main.MaLop,makeupClass.MaLop},Enrollments=enrollmentIds,FreeTuition=true})});
        await db.SaveChangesAsync();
        await File.WriteAllTextAsync(credentials,"Tài khoản kiểm thử góc học tập LHL Art\n"+string.Join('\n',names.Select(name=>name+" / "+password))+"\nLớp kiểm thử miễn phí, đã xác nhận vào lớp. Không có giao dịch thu tiền giả. Phụ huynh mẫu cũ xem được cả ba con.\n");
        await tx.CommitAsync();
        Console.WriteLine("Đã tạo 1 giáo viên, 3 tài khoản học viên và 3 phiếu vào lớp kiểm thử miễn phí. Mật khẩu trong .local/Tai_khoan_hoc_tap_mau.txt.");
    }
}
