using System.Data;
using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using MyThuat.Api.Data;
using MyThuat.Api.Models;
using MyThuat.Api.Services;
using MyThuat.Checks;
using LhlArt.Networking;

internal static class BusinessIntegrityChecks
{
    static string T(JsonElement x,string key)=>ArtApiClient.Text(x,key);
    static long Id(JsonElement x)=>ArtApiClient.Get(x,"id").GetInt64();
    static string Day(DateOnly x)=>x.ToString("yyyy-MM-dd");
    static string Stamp(DateTime x)=>x.ToString("yyyy-MM-ddTHH:mm:ss");
    public static async Task Run(ArtApiClient staff,DbContextOptions<MyThuatDbContext> options,Action<bool,string> check)
    {
        async Task Reject(string path,object body,string version,string expected){
            try{await staff.Put(path,body,version);throw new Exception("Expected rejection: "+expected);}
            catch(InvalidOperationException ex){check(ex.Message.Contains(expected),"API chặn: "+expected);}
        }
        var studentBody=new{HoTen="Kiểm thử mã bền vững",NgaySinh="2000-01-02",LienHeKhanCapTen="Liên hệ thử",LienHeKhanCapSDT="0900000000",TrangThai="HOAT_DONG"};
        var student=await staff.Post("api/quan-ly/hoc-vien",studentBody);var oldCode=T(student,"maHocVien");
        check(oldCode.StartsWith("HV-"),"Mã học viên tự sinh khi không gửi MaHocVien");
        await Reject("api/quan-ly/hoc-vien/"+Id(student),new{MaHocVien="HV-9999"},T(student,"rowVersion"),"Mã đã cấp không được sửa");
        await using(var db=new CheckDbContext(options)){db.HocVien.Remove(await db.HocVien.SingleAsync(x=>x.Id==Id(student)));await db.SaveChangesAsync();}
        var nextStudent=await staff.Post("api/quan-ly/hoc-vien",studentBody);
        check(string.CompareOrdinal(T(nextStudent,"maHocVien"),oldCode)>0,"Xóa hồ sơ mới nhất rồi tạo lại: không tái sử dụng mã học viên");
        var teacherBody=new{HoTen="Giáo viên mã mới",DienThoai="0900000000",TrinhDo="Cử nhân",ChuyenMon="Hội họa",CapDoCoTheDay="FOUNDATION",TrangThai="HOAT_DONG"};
        var teacher=await staff.Post("api/quan-ly/giao-vien",teacherBody);
        check(T(teacher,"maGiaoVien").StartsWith("GV-"),"Giáo viên được cấp mã GV tự động");
        await using(var db=new CheckDbContext(options)){db.GiaoVien.Remove(await db.GiaoVien.SingleAsync(x=>x.Id==Id(teacher)));await db.SaveChangesAsync();}
        var nextTeacher=await staff.Post("api/quan-ly/giao-vien",teacherBody);
        check(T(nextTeacher,"maGiaoVien")!=T(teacher,"maGiaoVien"),"Xóa giáo viên mới nhất không làm bộ đếm lùi");
        long typeId;await using(var db=new CheckDbContext(options))typeId=await db.LoaiKhoaHoc.Select(x=>x.Id).FirstAsync();
        object Course(string prefix)=>new{TienToMa=prefix,LoaiKhoaHocId=typeId,TenKhoa="Khóa tiền tố thử",MucTieu="Quan sát",PhienBan=1,CapDo="FOUNDATION",TuoiToiThieu=3,SoBuoi=1,SoTietMoiBuoi=1,PhutMoiTiet=60,HocPhi=100000,HieuLucTu=Day(BusinessClock.Today),TrangThai="NHAP"};
        var course=await staff.Post("api/quan-ly/khoa-hoc",Course("jun"));
        check(T(course,"maKhoa")=="JUN-0001","Tiền tố jun được chuẩn hóa và cấp mã khóa JUN-0001");
        foreach(var prefix in new[]{"JR","JUNIO","J1N","JŨN"}){
            var bad=false;try{await staff.Post("api/quan-ly/khoa-hoc",Course(prefix));}catch(InvalidOperationException ex){bad=ex.Message.Contains("3–4");}
            check(bad,"Từ chối tiền tố khóa sai: "+prefix);
        }
        await Reject("api/quan-ly/khoa-hoc/"+Id(course),new{TienToMa="ABC"},T(course,"rowVersion"),"Không đổi tiền tố");
        var content=await staff.Post("api/quan-ly/noi-dung-buoi",new{KhoaHocId=Id(course),ThuTu=1,ChuDe="Buổi thử",NoiDung="Vẽ",SoTiet=1,YeuCauSanPham="Tranh"});
        course=await staff.Put("api/quan-ly/khoa-hoc/"+Id(course),new{TrangThai="HOAT_DONG"},T(course,"rowVersion"));
        object Class()=>new{KhoaHocId=Id(course),TenLop="Lớp tự sinh mã",NgayKhaiGiangDuKien=Day(BusinessClock.Today.AddDays(40)),SoBuoiKeHoach=1,LichHocDuKien="Thứ Bảy · 09:00–10:00",SiSoToiThieu=1,SiSoToiDa=10,HocPhiApDung=100000,MoDangKyLuc=Stamp(BusinessClock.Now),DongDangKyLuc=Stamp(BusinessClock.Now.AddDays(30)),TrangThai="NHAP"};
        var cls=await staff.Post("api/quan-ly/lop-hoc",Class());var cls2=await staff.Post("api/quan-ly/lop-hoc",Class());
        check(T(cls,"maLop")=="JUN-L0001"&&T(cls2,"maLop")=="JUN-L0002","Chọn khóa JUN: hai lớp tự nhận JUN-L0001 và JUN-L0002");
        await using(var db=new CheckDbContext(options)){db.LopHoc.Remove(await db.LopHoc.SingleAsync(x=>x.Id==Id(cls2)));await db.SaveChangesAsync();}
        var cls3=await staff.Post("api/quan-ly/lop-hoc",Class());check(T(cls3,"maLop")=="JUN-L0003","Xóa lớp cuối rồi thêm lớp: mã lớp tiếp tục tăng");
        var unused=await staff.Post("api/quan-ly/khoa-hoc",Course("ARTS"));
        await using(var db=new CheckDbContext(options)){db.KhoaHoc.Remove(await db.KhoaHoc.SingleAsync(x=>x.Id==Id(unused)));await db.SaveChangesAsync();}
        var nextCourse=await staff.Post("api/quan-ly/khoa-hoc",Course("ARTS"));check(T(nextCourse,"maKhoa")=="ARTS-0002","Khóa mới dùng tiền tố 4 chữ và không cấp lại mã sau khi xóa");
        // Each connection commits a reserved code. The counter survives even without a business row.
        if(options.Extensions.Any(e=>e.GetType().Name.Contains("SqlServer"))){
            var codes=await Task.WhenAll(Enumerable.Range(0,12).Select(async _=>{
                await using var db=new CheckDbContext(options);await using var tx=await db.Database.BeginTransactionAsync(IsolationLevel.Serializable);
                var code=await CodeGenerator.NextAsync(db,db.HocVien,x=>x.MaHocVien,"PAR-",default);await tx.CommitAsync();return code;
            }));
            check(codes.Distinct().Count()==12,"12 kết nối SQL Server cấp mã đồng thời không trùng");
            var teachers=await Task.WhenAll(Enumerable.Range(0,8).Select(_=>staff.Post("api/quan-ly/giao-vien",teacherBody)));
            check(teachers.Select(x=>T(x,"maGiaoVien")).Distinct().Count()==8,"8 yêu cầu thêm giáo viên đồng thời qua API nhận 8 mã khác nhau");
        }
        long historicalId;
        var historicalDate=BusinessClock.Today.AddDays(-10);
        await using(var db=new CheckDbContext(options)){
            var room=await db.PhongHoc.FirstAsync();
            var history=new LopHoc{KhoaHocId=Id(course),MaLop="HISTORY_TEST",TenLop="Lớp đang học lịch sử",NgayKhaiGiangDuKien=historicalDate,NgayKhaiGiangThucTe=historicalDate,NgayKetThucDuKien=historicalDate,SoBuoiKeHoach=1,LichHocDuKien="Lịch đã chốt",SiSoToiThieu=1,SiSoToiDa=10,HocPhiApDung=100000,MoDangKyLuc=BusinessClock.Now.AddDays(-30),DongDangKyLuc=BusinessClock.Now.AddDays(-13),TrangThai="DANG_HOC"};
            db.Add(history);await db.SaveChangesAsync();historicalId=history.Id;
            var session=new BuoiHoc{LopHocId=history.Id,NoiDungBuoiId=Id(content),PhongHocId=room.Id,ThuTuTrongLop=1,LanXepLich=1,BatDau=historicalDate.ToDateTime(new TimeOnly(9,0)),KetThuc=historicalDate.ToDateTime(new TimeOnly(10,0)),SoTiet=1,PhutMoiTiet=60,TrangThai="DA_TO_CHUC"};db.Add(session);await db.SaveChangesAsync();
            db.GiangDayBuoi.Add(new(){BuoiHocId=session.Id,GiaoVienId=Id(nextTeacher),VaiTro="CHINH",SoTietThucDay=1,NguoiDuyet=await db.TaiKhoan.Where(x=>x.TenDangNhap=="root").Select(x=>x.Id).SingleAsync()});await db.SaveChangesAsync();
        }
        var historyPath="api/quan-ly/lop-hoc/"+historicalId;
        var historyRow=await staff.Read(historyPath);
        var originalApprover=T(historyRow,"nguoiDuyet");
        historyRow=await staff.Put(historyPath,new{TenLop="Chỉ sửa tên lớp"},T(historyRow,"rowVersion"));
        check(T(historyRow,"ngayKhaiGiangThucTe")==Day(historicalDate),"Sửa lớp đang học giữ nguyên ngày khai giảng thực tế, không đổi thành hôm nay");
        check(T(historyRow,"nguoiDuyet")==originalApprover,"Sửa tên lớp không ghi đè người duyệt lịch sử");
        foreach(var payload in new object[]{new{NgayKhaiGiangDuKien=Day(BusinessClock.Today)},new{NgayKetThucDuKien=Day(BusinessClock.Today)},new{LichHocDuKien="Lịch giả"}})
            await Reject(historyPath,payload,T(historyRow,"rowVersion"),"Lớp đã xếp lịch");
        await Reject(historyPath,new{NgayKhaiGiangThucTe=Day(BusinessClock.Today)},T(historyRow,"rowVersion"),"Ngày khai giảng thực tế");
        await Reject(historyPath,new{TrangThai="NHAP"},T(historyRow,"rowVersion"),"Không được chuyển ngược");
        await Reject(historyPath,new{DongDangKyLuc=Stamp(BusinessClock.Now)},T(historyRow,"rowVersion"),"thời gian tuyển sinh");
        historyRow=await staff.Put(historyPath,new{TrangThai="DA_KET_THUC"},T(historyRow,"rowVersion"));
        check(T(historyRow,"ngayKhaiGiangThucTe")==Day(historicalDate),"Kết thúc lớp không sửa ngày khai giảng gốc");
        await Reject(historyPath,new{TrangThai="DANG_HOC"},T(historyRow,"rowVersion"),"Không được chuyển ngược");
    }
}
