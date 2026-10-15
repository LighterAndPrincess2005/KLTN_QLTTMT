using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json.Nodes;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using MyThuat.Api.Data;
using MyThuat.Api.Models;
using MyThuat.Api.Services;

namespace MyThuat.Checks;
public static class CenterChecks
{
    public static async Task Run(DbContextOptions<MyThuatDbContext> options, HttpClient http)
    {
        var count = 0;
        void Check(bool value,string name) { if(!value)throw new Exception(name); Console.WriteLine($"PASS center {++count}: {name}"); }
        async Task<JsonNode> Call(string method,string path,string? token=null,object? body=null,int expected=200)
        {
            using var message=new HttpRequestMessage(new HttpMethod(method),path);
            if(token is not null)message.Headers.Authorization=new AuthenticationHeaderValue("Bearer",token);
            if(body is not null)message.Content=JsonContent.Create(body);
            using var response=await http.SendAsync(message);var text=await response.Content.ReadAsStringAsync();
            Check((int)response.StatusCode==expected,$"{method} {path}: {(int)response.StatusCode} expected {expected}"+(response.IsSuccessStatusCode?"":" "+text));
            return string.IsNullOrWhiteSpace(text)?new JsonObject():JsonNode.Parse(text)!;
        }
        async Task<string> Login(string name)=>(await Call("POST","/api/tai-khoan/dang-nhap",body:new {TenDangNhap=name,MatKhau=CenterSampleData.Password}))["accessToken"]!.GetValue<string>();
        static long Id(JsonNode x)=>(x["id"]??x["Id"])!.GetValue<long>();
        await using(var db=new CheckDbContext(options))
        {
            Check(await db.HocVien.CountAsync()==114,"114 học viên");
            Check(await db.LopHoc.CountAsync()==18,"18 lớp thuộc 3 giai đoạn");
            Check(await db.DangKy.CountAsync()==108,"108 đăng ký");
            Check(await db.TaiKhoan.CountAsync()==71,"71 tài khoản các vai trò");
            await CenterSampleData.Seed(db);
            Check(await db.DangKy.CountAsync()==108&&await db.TaiKhoan.CountAsync()==71,"Nạp lại không trùng dữ liệu");
            var hasher=new PasswordHasher<TaiKhoan>();
            Check((await db.TaiKhoan.ToListAsync()).All(a=>hasher.VerifyHashedPassword(a,a.PasswordHash,CenterSampleData.Password)!=PasswordVerificationResult.Failed),"Mật khẩu README khớp toàn bộ tài khoản mẫu, chỉ lưu băm");
            var slots=await db.BuoiHoc.ToListAsync();
            Check(!slots.Any(a=>slots.Any(b=>a.Id<b.Id&&a.PhongHocId==b.PhongHocId&&a.BatDau<b.KetThuc&&b.BatDau<a.KetThuc)),"Lịch không trùng phòng");
            var fees=await db.KhoanThuDangKy.ToListAsync();var paid=await db.GiaoDichTien.ToListAsync();
            var enrollments=await db.DangKy.ToListAsync();
            Check(enrollments.Where(e=>e.TrangThai=="DA_XAC_NHAN").All(e=>paid.Where(p=>p.DangKyId==e.Id).Sum(p=>p.SoTien)==fees.Where(f=>f.DangKyId==e.Id).Sum(f=>f.ThanhTienChot)),"Phiếu xác nhận đã thu đủ học phí");
            Check(enrollments.All(e=>e.TrangThai!="CHO_THANH_TOAN"||e.HanGiuCho>BusinessClock.Now),"Phiếu chờ còn hạn giữ chỗ");
        }
        var parent=await Login("phuhuynh01");var cashier=await Login("thungan");var admin=await Login("quantri");
        var teacher=await Login("giaovien01");var student=await Login("hocvien01");var warehouse=await Login("quanlykho");
        var courses=await Call("GET","/api/khoa-hoc");Check(courses["tongSo"]!.GetValue<int>()==6,"Website thấy 6 khóa từ database");
        var page=await Call("GET","/api/dang-ky/quan-ly?trang=1&kichThuoc=10",cashier);
        Check(page["tongSo"]!.GetValue<int>()==108&&page["duLieu"]!.AsArray().Count==10,"Phân trang danh sách đăng ký WinForms");
        await Call("GET","/api/dang-ky/quan-ly",parent,expected:403);
        await Call("GET","/api/dang-ky/quan-ly",teacher,expected:403);
        await Call("GET","/api/dang-ky/quan-ly?trang=0",admin,expected:400);
        var mine=await Call("GET","/api/dang-ky/toi",parent);
        Check(mine.AsArray().All(x=>x!["hocVienId"] is not null&&x["khoaHocId"] is not null&&x["coTheQuanLy"]!.GetValue<bool>()),"Đủ trường chọn học viên và phân quyền website");
        var studentMine=await Call("GET","/api/dang-ky/toi",student);
        Check(studentMine.AsArray().All(x=>!x!["coTheQuanLy"]!.GetValue<bool>()),"Tài khoản học viên chỉ xem kết quả");
        var schedule=await Call("GET","/api/hoc-tap/giao-vien/buoi-cua-toi",teacher);
        Check(schedule.AsArray().Count>0,"Góc giáo viên website nhận lịch");
        await Call("GET","/api/lich-hoc",teacher);
        var stock=await Call("GET","/api/kho/ton",warehouse);
        Check(stock.AsArray().Count==12 && stock.AsArray().All(x=>x!["khaDung"]!.GetValue<decimal>()>=0),"12 họa cụ có tồn kho không âm theo lô");
        long pupilId,memberId,classId;
        await using(var db=new CheckDbContext(options))
        {
            pupilId=(await db.HocVien.SingleAsync(x=>x.MaHocVien=="HVNEW01")).Id;
            memberId=(await db.ThanhVien.SingleAsync(x=>x.MaThanhVien=="TV001")).Id;
            classId=(await db.LopHoc.SingleAsync(x=>x.MaLop=="L01-03")).Id;
        }
        var body=new {HocVienId=pupilId,LopHocId=classId,NguoiDangKyId=memberId,IdempotencyKey="web-center-check-0001"};
        var enrolled=await Call("POST","/api/dang-ky",parent,body);var id=Id(enrolled);
        Check(Id(await Call("POST","/api/dang-ky",parent,body))==id,"Retry đăng ký website không nhân đôi");
        var list=await Call("GET","/api/dang-ky/quan-ly?timKiem=Tuệ%20An",cashier);
        Check(list["duLieu"]!.AsArray().Any(x=>Id(x!)==id),"WinForms thấy ngay đăng ký vừa gửi từ web");
        var debt=await Call("GET",$"/api/dang-ky/{id}/cong-no",cashier);
        var charge=debt["khoanThu"]!.AsArray()[0]!;
        var payment=new {DangKyId=id,KhoanThuId=Id(charge),SoTien=charge["conThieu"]!.GetValue<decimal>(),PhuongThuc="TIEN_MAT",MaChungTu="CHECK-CENTER-001",IdempotencyKey="win-center-receive-0001"};
        await Call("POST","/api/giao-dich/thu",cashier,payment);
        await Call("POST","/api/giao-dich/thu",cashier,payment);
        mine=await Call("GET","/api/dang-ky/toi",parent);
        var confirmed=mine.AsArray().Single(x=>Id(x!)==id)!;
        Check(confirmed["trangThai"]!.GetValue<string>()=="DA_XAC_NHAN","Web thấy xác nhận sau thu học phí bằng API WinForms");
        var progress=await Call("GET",$"/api/hoc-tap/ho-so/{confirmed["hoSoTheoHocId"]}/tien-do",parent);
        Check(progress["diemDanh"]!.AsArray().Count==16,"Web thấy lịch đủ 16 buổi sau xác nhận");
        var feedback=await Call("POST","/api/phan-hoi",parent,new {DangKyId=id,NoiDung="Nhờ trung tâm tư vấn giấy vẽ."});
        await Call("POST",$"/api/phan-hoi/{Id(feedback)}/tra-loi",admin,new {TraLoi="Dùng giấy A3 định lượng 200 gsm."});
        var replies=await Call("GET","/api/phan-hoi",parent);
        Check(replies.AsArray().Any(x=>Id(x!)==Id(feedback)&&(x!["TraLoi"]??x["traLoi"])!.GetValue<string>().Contains("200 gsm")),"Phản hồi web → nhân viên trả lời → web nhận câu trả lời");
        var monthly=await Call("GET","/api/bao-cao/doanh-thu-thang",admin);
        Check(monthly.AsArray().Count==6&&monthly.AsArray().Sum(x=>x!["hocPhi"]!.GetValue<decimal>())>0,"Biểu đồ đủ 6 tháng, học phí từ chứng từ đã ghi nhận");
        await Call("GET","/api/bao-cao/doanh-thu-thang",parent,expected:403);
        var current=studentMine.AsArray()[0]!;
        var currentProgress=await Call("GET",$"/api/hoc-tap/ho-so/{current["hoSoTheoHocId"]}/tien-do",student);
        var firstAttendance=currentProgress["diemDanh"]!.AsArray().First(x=>x!["trangThai"]!.GetValue<string>()=="CO_MAT")!;
        var roster=await Call("GET",$"/api/hoc-tap/buoi/{firstAttendance["buoiHocId"]}/danh-sach",teacher);
        var attendance=roster.AsArray().Single(x=>Id(x!)==Id(firstAttendance))!;
        await Call("PUT",$"/api/hoc-tap/diem-danh/{Id(attendance)}",admin,new{TrangThai="CO_MAT",PhutThamDu=90,DiemSanPham=8.5m,NhanXet="Nét vẽ tiến bộ, bố cục rõ ràng.",SanPhamUrl=(string?)null,RowVersion=attendance["rowVersion"]!.GetValue<string>()});
        var updated=await Call("GET",$"/api/hoc-tap/ho-so/{current["hoSoTheoHocId"]}/tien-do",student);
        Check(updated["diemDanh"]!.AsArray().Any(x=>Id(x!)==Id(attendance)&&x!["nhanXet"]!.GetValue<string>()=="Nét vẽ tiến bộ, bố cục rõ ràng."),"Nhận xét ghi bởi nhân viên hiển thị tại góc học tập web");
        var other=await Login("phuhuynh02");
        await Call("GET",$"/api/hoc-tap/ho-so/{confirmed["hoSoTheoHocId"]}/tien-do",other,expected:403);
        using(var desktop=new LhlArt.Networking.ArtApiClient(baseUrl:http.BaseAddress!.ToString()))
        {
            await desktop.Login("giaovien01",CenterSampleData.Password);
            var desktopSchedule=await desktop.Read("api/lich-hoc");
            Check(desktopSchedule.GetArrayLength()==48,"HTTP client thật của WinForms đọc đủ 48 buổi giáo viên");
            await desktop.Login("thungan",CenterSampleData.Password);
            var desktopEnrollments=await desktop.Read("api/dang-ky/quan-ly?kichThuoc=100");
            Check(LhlArt.Networking.ArtApiClient.Get(desktopEnrollments,"tongSo").GetInt32()==109,"HTTP client thật của WinForms đọc đăng ký mới từ web");
        }
        Console.WriteLine($"CENTER SYNC: ALL {count} CHECKS PASSED");
    }
}
