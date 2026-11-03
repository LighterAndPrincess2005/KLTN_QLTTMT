using Microsoft.Extensions.Configuration;
using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Hosting.Server;
using Microsoft.AspNetCore.Hosting.Server.Features;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using MyThuat.Api.Controllers;
using MyThuat.Api.Data;
using MyThuat.Api.Models;
using MyThuat.Api.Security;
using MyThuat.Api.Services;
using MyThuat.Checks;
using LhlArt.Networking;
using LhlArt.Services;

var folder=Path.Combine(Path.GetTempPath(),"LhlWinFormsChecks-"+Guid.NewGuid().ToString("N"));Directory.CreateDirectory(folder);
var sql=args.Contains("--sqlserver");var database="LhlWinFormsChecks_"+Guid.NewGuid().ToString("N");
var options=sql?new DbContextOptionsBuilder<MyThuatDbContext>().UseSqlServer($"Server=(localdb)\\MSSQLLocalDB;Database={database};Trusted_Connection=True;TrustServerCertificate=True").Options
    :new DbContextOptionsBuilder<MyThuatDbContext>().UseSqlite("Data Source="+Path.Combine(folder,"checks.db")).Options;
var builder=WebApplication.CreateBuilder(new WebApplicationOptions{EnvironmentName="Development",ApplicationName=typeof(AuthController).Assembly.FullName});
builder.WebHost.UseUrls("http://127.0.0.1:0");builder.Logging.ClearProviders();
builder.Services.AddControllers().AddApplicationPart(typeof(AuthController).Assembly);
builder.Services.AddScoped<MyThuatDbContext>(_=>new CheckDbContext(options));
builder.Services.AddSingleton<IDataProtectionProvider>(new EphemeralDataProtectionProvider());builder.Services.AddSingleton<AccessTokens>();
builder.Services.AddScoped<IPasswordHasher<TaiKhoan>,PasswordHasher<TaiKhoan>>();builder.Services.AddHttpContextAccessor();
builder.Services.AddScoped<AuditService>();builder.Services.AddScoped<AccessService>();builder.Services.AddScoped<PolicyService>();
builder.Services.AddScoped<DanhMucService>();builder.Services.AddScoped<ManagementValidation>();builder.Services.AddScoped<ManagementService>();
builder.Services.AddScoped<ScheduleService>();builder.Services.AddScoped<EnrollmentService>();builder.Services.AddScoped<FinanceService>();
builder.Services.AddScoped<NotificationService>();var notificationMail=new CaptureNotificationMail();builder.Services.AddSingleton<INotificationMail>(notificationMail);
builder.Services.AddScoped<OnlinePaymentService>();
builder.Configuration.AddInMemoryCollection(new Dictionary<string,string?>{{"SePay:Enabled","true"},{"SePay:BankCode","ACB"},{"SePay:AccountNumber","0000000000"},{"SePay:AccountName","TAI KHOAN GIA LAP"},{"SePay:WebhookKey","local-only-payment-secret"}});
var recoveryMail=new CaptureRecoveryMail();builder.Services.AddSingleton<IRecoveryMail>(recoveryMail);builder.Services.AddScoped<AccountRecoveryService>();
builder.Services.AddScoped<LearningService>();builder.Services.AddScoped<ChangeService>();builder.Services.AddScoped<InventoryService>();builder.Services.AddScoped<ClassLifecycleService>();builder.Services.AddScoped<ClassInventoryService>();
builder.Services.AddAuthentication("Bearer").AddScheme<AuthenticationSchemeOptions,BearerAuthenticationHandler>("Bearer",_=>{});
builder.Services.AddAuthorization(o=>{
    o.FallbackPolicy=new AuthorizationPolicyBuilder().RequireAuthenticatedUser().Build();
    foreach(var p in Enum.GetValues<Permission>().Where(x=>x!=Permission.None))o.AddPolicy(p.ToString(),b=>b.RequireAuthenticatedUser().RequireAssertion(c=>p is Permission.TaiKhoan or Permission.NhatKy?c.User.IsInRole("QUAN_TRI"):c.User.Has(p)));
});
builder.Services.AddRateLimiter(o=>{foreach(var name in new[]{"login","consultation","recovery"})o.AddFixedWindowLimiter(name,v=>{v.PermitLimit=1000;v.Window=TimeSpan.FromMinutes(1);});});
var app=builder.Build();
app.Use(async(ctx,next)=>{try{await next(ctx);}catch(Exception ex){ctx.Response.StatusCode=ex is ApiError e?e.Status:ex is DbUpdateException?409:500;await ctx.Response.WriteAsJsonAsync(new{error=ex.Message});}});
app.UseRouting();app.UseRateLimiter();app.UseAuthentication();app.UseAuthorization();app.MapControllers();
int checks=0;bool storeCreated=false;
void Check(bool condition,string name){if(!condition)throw new Exception(name);Console.WriteLine($"PASS {++checks}: {name}");}
static long Id(JsonElement row,string key="id")=>ApiDataService.Id(row,key);
static string T(JsonElement row,string key)=>ArtApiClient.Text(row,key);
static string Stamp(DateTime value)=>value.ToString("yyyy-MM-ddTHH:mm:ss");
const string password="WinFormsCheck2026!";
try
{
    await using(var db=new CheckDbContext(options))await db.Database.EnsureCreatedAsync();storeCreated=true;
    await app.StartAsync();
    var address=app.Services.GetRequiredService<IServer>().Features.Get<IServerAddressesFeature>()!.Addresses.Single();
    using var staff=new ArtApiClient(baseUrl:address);using var web=new HttpClient{BaseAddress=new Uri(address)};
    async Task<JsonElement> Web(string method,string path,object? body=null,HttpStatusCode expected=HttpStatusCode.OK)
    {
        using var request=new HttpRequestMessage(new HttpMethod(method),path);if(body!=null)request.Content=JsonContent.Create(body);
        using var response=await web.SendAsync(request);var text=await response.Content.ReadAsStringAsync();
        if(response.StatusCode!=expected)throw new Exception($"{path}: {response.StatusCode}: {text}");
        return string.IsNullOrWhiteSpace(text)?JsonSerializer.SerializeToElement(new{}):JsonDocument.Parse(text).RootElement.Clone();
    }
    await staff.Post("api/tai-khoan/thiet-lap",new{TenDangNhap="root",MatKhau=password,HoTen="Quản trị kiểm thử"});await staff.Login("root",password);
    var data=new ApiDataService(staff);var rule=(await data.All("api/quan-ly/quy-dinh")).Single();
    await staff.Put("api/quan-ly/quy-dinh/"+Id(rule),new{TrangThai="HOAT_DONG"},T(rule,"rowVersion"));
    long courseId,classId;
    await using(var db=new CheckDbContext(options))
    {
        var type=new LoaiKhoaHoc{MaLoai="VE",TenLoai="Mỹ thuật",TrangThai="HOAT_DONG"};db.Add(type);await db.SaveChangesAsync();
        var course=new KhoaHoc{LoaiKhoaHocId=type.Id,MaKhoa="CB",TenKhoa="Sắc màu sáng tạo",PhienBan=1,CapDo="BASIC",TuoiToiThieu=6,MucTieu="Luyện vẽ",SoBuoi=2,SoTietMoiBuoi=4,PhutMoiTiet=30,HocPhi=100000,HieuLucTu=BusinessClock.Today,TrangThai="HOAT_DONG"};
        var room=new PhongHoc{MaPhong="P1",TenPhong="Phòng vẽ",SucChua=20,TrangThai="HOAT_DONG"};db.AddRange(course,room);await db.SaveChangesAsync();courseId=course.Id;
        var start=BusinessClock.Today.AddDays(14).ToDateTime(new TimeOnly(10,0));
        var cls=new LopHoc{KhoaHocId=course.Id,MaLop="BASIC1",TenLop="Lớp sắc màu",NgayKhaiGiangDuKien=DateOnly.FromDateTime(start),SoBuoiKeHoach=2,LichHocDuKien="Hàng tuần",SiSoToiThieu=1,SiSoToiDa=20,HocPhiApDung=100000,MoDangKyLuc=BusinessClock.Now.AddDays(-1),DongDangKyLuc=start.AddHours(-48),TrangThai="DANG_TUYEN_SINH"};db.Add(cls);await db.SaveChangesAsync();classId=cls.Id;
        for(short i=1;i<=2;i++)
        {
            var content=new NoiDungBuoi{KhoaHocId=course.Id,ThuTu=i,ChuDe="Buổi "+i,NoiDung="Vẽ",SoTiet=4};db.Add(content);await db.SaveChangesAsync();
            db.BuoiHoc.Add(new(){LopHocId=cls.Id,NoiDungBuoiId=content.Id,PhongHocId=room.Id,ThuTuTrongLop=i,BatDau=start.AddDays(7*(i-1)),KetThuc=start.AddDays(7*(i-1)).AddHours(2),SoTiet=4,PhutMoiTiet=30,LanXepLich=1,TrangThai="DA_XEP_LICH"});
        }
        await db.SaveChangesAsync();
    }
    foreach(var invalidEmail in new string?[]{null,"","   ","email-khong-hop-le"}){
        await Web("POST","api/tai-khoan/dang-ky",new{TenDangNhap="email_invalid",MatKhau=password,HoTen="Thử ràng buộc email",DienThoai="0900000000",Email=invalidEmail},HttpStatusCode.BadRequest);
    }
    Check(true,"Đăng ký bị chặn nếu thiếu email, để trống, chỉ khoảng trắng hoặc sai định dạng");
    var signup=await staff.Post("api/tai-khoan/dang-ky",new{TenDangNhap="phuhuynh",MatKhau=password,HoTen="Nguyễn Thị Ánh",DienThoai="0900000000",Email="parent@example.invalid",NgaySinh="1985-06-17"});var memberId=Id(signup,"thanhVienId");
    var login=await Web("POST","api/tai-khoan/dang-nhap",new{TenDangNhap="phuhuynh",MatKhau=password});web.DefaultRequestHeaders.Authorization=new AuthenticationHeaderValue("Bearer",T(login,"accessToken"));
    var me=await Web("GET","api/tai-khoan/toi");Check(T(me,"hoTen")=="Nguyễn Thị Ánh"&&T(me,"ngaySinh")=="1985-06-17","Tạo tài khoản qua WinForms, web đăng nhập ngay và đọc đúng tiếng Việt / ngày sinh");
    Check((await Web("GET","api/hoc-vien/toi")).GetArrayLength()==0&&(await Web("GET","api/dang-ky/toi")).GetArrayLength()==0,"Tạo tài khoản không tự tạo học viên hoặc đăng ký lớp");
    var student=await staff.Post("api/quan-ly/hoc-vien",new{MaHocVien="HVTEST01",HoTen="Võ Huỳnh Gia Huy",NgaySinh="2005-06-17",LienHeKhanCapTen="Nguyễn Thị Ánh",LienHeKhanCapSDT="0900000000",TrangThai="HOAT_DONG"});
    var link=await staff.Post("api/quan-ly/dai-dien",new{ThanhVienId=memberId,HocVienId=Id(student),QuanHe="ME",QuyenDaiDien="DAY_DU",LaLienHeChinh=true,HieuLucTu=Stamp(BusinessClock.Now.AddMinutes(-1))});
    var mine=await Web("GET","api/hoc-vien/toi");Check(mine.GetArrayLength()==1&&T(mine[0],"hoTen")=="Võ Huỳnh Gia Huy","Hồ sơ và quan hệ tạo từ WinForms hiện trên web ngay");
    student=await staff.Put("api/quan-ly/hoc-vien/"+Id(student),new{HoTen="Võ Huỳnh Gia Huy – Mỹ thuật"},T(student,"rowVersion"));
    mine=await Web("GET","api/hoc-vien/toi");Check(T(mine[0],"hoTen")=="Võ Huỳnh Gia Huy – Mỹ thuật","Sửa tên tiếng Việt trong WinForms được web đọc lại");
    var display=await new ApiDisplayService(staff).Enrich("api/quan-ly/dai-dien",await data.All("api/quan-ly/dai-dien"));
    Check(T(display[0],"tenNguoiDaiDien")=="Nguyễn Thị Ánh"&&T(display[0],"tenHocVien").Contains("Gia Huy"),"Danh sách quan hệ có họ tên cả hai người");
    display=await new ApiDisplayService(staff).Enrich("api/tai-khoan",await data.All("api/tai-khoan",false));
    Check(display.Any(x=>T(x,"tenDangNhap")=="phuhuynh"&&T(x,"loaiTaiKhoan")=="Phụ huynh"),"Phân loại tài khoản phụ huynh từ quan hệ thực tế");
    var publicOffer=await staff.Post("api/quan-ly/khuyen-mai",new{MaKhuyenMai="PUBLIC10",TenKhuyenMai="Ưu đãi sáng tạo 10%",TuDongApDung=true,KhoaApDungId=courseId,NhomKhoanThu="HOC_PHI",LoaiGiam="PHAN_TRAM",GiaTriGiam=10,TongLuot=100,BatDau=Stamp(BusinessClock.Now.AddDays(-1)),KetThuc=Stamp(BusinessClock.Now.AddDays(30)),TrangThai="HOAT_DONG"});
    await staff.Post("api/quan-ly/khuyen-mai",new{MaKhuyenMai="RIENG5",TenKhuyenMai="Mã nhập riêng 5%",TuDongApDung=false,KhoaApDungId=courseId,NhomKhoanThu="HOC_PHI",LoaiGiam="PHAN_TRAM",GiaTriGiam=5,TongLuot=100,BatDau=Stamp(BusinessClock.Now.AddDays(-1)),KetThuc=Stamp(BusinessClock.Now.AddDays(30)),TrangThai="HOAT_DONG"});
    var offers=await Web("GET","api/khuyen-mai");Check(offers.GetArrayLength()==1&&ArtApiClient.Get(offers[0],"giaTriGiam").GetDecimal()==10,"Web thấy ưu đãi khóa 10%, không công khai mã nhập riêng");
    var quote=await Web("POST","api/dang-ky/bao-gia",new{HocVienId=Id(student),LopHocId=classId,MaGiamHocPhi="RIENG5"});Check(ArtApiClient.Get(quote,"tongPhaiTra").GetDecimal()==85000,"Báo giá web áp đúng ưu đãi khóa 10% và mã riêng 5%");
    var created=await staff.Post("api/dang-ky",new{HocVienId=Id(student),LopHocId=classId,NguoiDangKyId=memberId,IdempotencyKey="WIN-INTEGRATION-0001",MaGiamHocPhi="RIENG5"});var enrollmentId=Id(created);
    var repeated=await staff.Post("api/dang-ky",new{HocVienId=Id(student),LopHocId=classId,NguoiDangKyId=memberId,IdempotencyKey="WIN-INTEGRATION-0001",MaGiamHocPhi="RIENG5"});Check(Id(repeated)==enrollmentId,"Retry ghi danh giữ khóa idempotency không tạo trùng");
    Check((await Web("GET","api/dang-ky/toi")).EnumerateArray().Any(x=>Id(x)==enrollmentId),"Đăng ký tại quầy qua WinForms hiện trong tài khoản web");
    var detail=await staff.Read("api/dang-ky/"+enrollmentId);Check(Id(ArtApiClient.Get(detail,"dangKy"))==enrollmentId,"Tra cứu ID đăng ký bằng API mới");
    var debt=await staff.Read($"api/dang-ky/{enrollmentId}/cong-no");var charge=ArtApiClient.Get(debt,"khoanThu")[0];
    Check(ArtApiClient.Get(charge,"conThieu").GetDecimal()==85000,"Công nợ sau ưu đãi khớp báo giá web");
    var receiptPayment=await staff.Post("api/giao-dich/thu",new{DangKyId=enrollmentId,KhoanThuId=Id(charge),SoTien=85000,PhuongThuc="TIEN_MAT",MaChungTu="PT-CHECK-01",IdempotencyKey="WIN-RECEIPT-0001"});
    var registrations=await Web("GET","api/dang-ky/toi");Check(registrations.EnumerateArray().Any(x=>Id(x)==enrollmentId&&T(x,"trangThai")=="DA_XAC_NHAN"),"WinForms thu đủ học phí, web chuyển sang đã xác nhận");
    var webStudent=await Web("POST","api/hoc-vien/toi",new{HoTen="Nguyễn Bảo An",NgaySinh="2010-05-12",QuanHe="ME",LienHeKhanCapTen="Nguyễn Thị Ánh",LienHeKhanCapSDT="0900000000",MucTieuHoc="Học vẽ"});
    var webRegistration=await Web("POST","api/dang-ky",new{HocVienId=Id(webStudent),LopHocId=classId,NguoiDangKyId=memberId,IdempotencyKey="WEB-INTEGRATION-0001"});
    var fromWeb=ArtApiClient.Get(await staff.Read("api/dang-ky/"+Id(webRegistration)),"dangKy");Check(Id(fromWeb)==Id(webRegistration),"Web ghi danh, WinForms đọc được bằng ID");
    Check(ApiDataService.Channel(fromWeb)=="Chưa được API ghi nhận","Xác nhận API hiện chưa lưu kênh online/offline");
    try{await staff.Read("api/dang-ky/quan-ly");throw new Exception("Endpoint unexpectedly available; update compatibility checks");}catch(ApiNotFoundException){Check(true,"Xác nhận thiếu endpoint tổng danh sách; WinForms dùng tra cứu ID");}
    // Local mock webhooks only: no request to SePay or a real bank is made.
    var paymentId=Id(webRegistration);var paymentConfig=app.Services.GetRequiredService<IConfiguration>();
    paymentConfig["SePay:Enabled"]="false";
    var offlineCheckout=await Web("POST",$"api/thanh-toan/{paymentId}/phien",new{});
    Check(!ArtApiClient.Get(offlineCheckout,"sanSang").GetBoolean(),"PAY01 Chưa cấu hình SePay: không tạo QR hoặc tự xác nhận tiền");
    paymentConfig["SePay:Enabled"]="true";
    var checkout=await Web("POST",$"api/thanh-toan/{paymentId}/phien",new{});var paymentCode=T(checkout,"maThanhToan");var expectedDue=ArtApiClient.Get(checkout,"conPhaiTra").GetDecimal();
    Check(paymentCode.StartsWith("BUI")&&ArtApiClient.Get(checkout,"sanSang").GetBoolean()&&expectedDue>0,"PAY02 Tạo QR với mã riêng và công nợ đã trừ ưu đãi từ server");
    Check(T(await Web("POST",$"api/thanh-toan/{paymentId}/phien",new{}),"maThanhToan")==paymentCode,"PAY03 Mở lại thanh toán dùng cùng phiên, không tạo thêm mã");
    var reportPath=$"api/thanh-toan/{paymentId}/bao-chuyen-khoan";
    await Web("POST",reportPath,new{});
    await Web("POST",reportPath,new{});
    var reported=await staff.Read("api/thanh-toan/bao-chuyen-khoan");
    var afterReport=await Web("GET",$"api/thanh-toan/{paymentId}/trang-thai");
    Check(reported.EnumerateArray().Count(x=>Id(x)==paymentId)==1&&
        !ArtApiClient.Get(afterReport,"daXacNhan").GetBoolean()&&ArtApiClient.Get(afterReport,"conPhaiTra").GetDecimal()==expectedDue,
        "Báo chuyển khoản hai lần chỉ có một phiếu cho thu ngân và không tự ghi nhận tiền");
    await Web("GET","api/thanh-toan/bao-chuyen-khoan",expected:HttpStatusCode.Forbidden);
    async Task<JsonElement> Hook(object body,string secret="local-only-payment-secret",HttpStatusCode expected=HttpStatusCode.OK){using var client=new HttpClient{BaseAddress=new Uri(address)};client.DefaultRequestHeaders.TryAddWithoutValidation("Authorization","Apikey "+secret);using var response=await client.PostAsJsonAsync("api/thanh-toan/sepay/webhook",body);var json=await response.Content.ReadAsStringAsync();if(response.StatusCode!=expected)throw new Exception("Webhook: "+response.StatusCode+" "+json);return string.IsNullOrWhiteSpace(json)?JsonSerializer.SerializeToElement(new{}):JsonDocument.Parse(json).RootElement.Clone();}
    var half=decimal.Floor(expectedDue/2);var event1=new{Id=910001,AccountNumber="0000000000",TransferType="in",TransferAmount=half,Content=paymentCode,ReferenceCode="MOCK01"};
    await Hook(event1,"wrong-key",HttpStatusCode.Unauthorized);
    Check(true,"PAY04 Webhook sai khóa bị từ chối trước khi ghi giao dịch");
    await Hook(new{Id=910002,AccountNumber="1111111111",TransferType="in",TransferAmount=half,Content=paymentCode},expected:HttpStatusCode.BadRequest);
    Check(true,"PAY05 Webhook sai tài khoản nhận không ghi nhận tiền");
    await Hook(new{Id=910003,AccountNumber="0000000000",TransferType="in",TransferAmount=1.5m,Content=paymentCode},expected:HttpStatusCode.BadRequest);
    Check(true,"PAY06 Số tiền VND có phần lẻ bị từ chối");
    await Hook(event1);await Hook(event1);
    var partial=await Web("GET",$"api/thanh-toan/{paymentId}/trang-thai");
    Check(!ArtApiClient.Get(partial,"daXacNhan").GetBoolean()&&ArtApiClient.Get(partial,"conPhaiTra").GetDecimal()==expectedDue-half,"PAY07 Trả thiếu chỉ giảm công nợ, không xác nhận vào lớp");
    await using(var db=new CheckDbContext(options))Check(await db.Set<SuKienSePay>().CountAsync(x=>x.MaGiaoDichSePay==910001)==1&&await db.GiaoDichTien.CountAsync(x=>x.MaChungTu=="SP910001")==1,"PAY08 Webhook lặp không ghi nhận tiền hai lần");
    await Hook(new{Id=910001,AccountNumber="0000000000",TransferType="in",TransferAmount=half+1,Content=paymentCode,ReferenceCode="MOCK01"},expected:HttpStatusCode.Conflict);
    Check(true,"PAY09 Cùng ID ngân hàng nhưng đổi số tiền bị chặn");
    await Hook(new{Id=910004,AccountNumber="0000000000",TransferType="in",TransferAmount=expectedDue-half,Content=paymentCode,ReferenceCode="MOCK02"});
    Check(ArtApiClient.Get(await Web("GET",$"api/thanh-toan/{paymentId}/trang-thai"),"daXacNhan").GetBoolean(),"PAY10 Đủ tiền xác nhận đăng ký qua cơ chế công nợ dùng chung web và WinForms");
    await Hook(new{Id=910005,AccountNumber="0000000000",TransferType="in",TransferAmount=100,Content=paymentCode,ReferenceCode="EXCESS"});
    await using(var db=new CheckDbContext(options))Check(await db.GiaoDichTien.AnyAsync(x=>x.MaChungTu=="SP910005"&&x.TrangThai=="CHO_DOI_CHIEU"&&x.KhoanThuId==null),"PAY11 Tiền thừa được lưu chờ đối chiếu, không tự phân bổ thêm");
    await Hook(new{Id=910006,AccountNumber="0000000000",TransferType="in",TransferAmount=100,Content="SAI NOI DUNG",ReferenceCode="UNKNOWN"});
    await using(var db=new CheckDbContext(options))Check(await db.Set<SuKienSePay>().AnyAsync(x=>x.MaGiaoDichSePay==910006&&x.DangKyId==null&&x.TrangThai=="CHO_DOI_CHIEU"),"PAY12 Sai mã thanh toán vẫn lưu sự kiện đối soát, không tự gán học viên");
    await Hook(new{Id=910007,AccountNumber="0000000000",TransferType="out",TransferAmount=100,Content=paymentCode});
    await using(var db=new CheckDbContext(options))Check(!await db.GiaoDichTien.AnyAsync(x=>x.MaChungTu=="SP910007"),"PAY13 Tiền ra không được tính thành tiền học phí vào");
    var lateStudent=await Web("POST","api/hoc-vien/toi",new{HoTen="Học viên thử hết hạn",NgaySinh="2011-01-01",QuanHe="ME",LienHeKhanCapTen="Nguyễn Thị Ánh",LienHeKhanCapSDT="0900000000"});
    var lateRegistration=await Web("POST","api/dang-ky",new{HocVienId=Id(lateStudent),LopHocId=classId,NguoiDangKyId=memberId,IdempotencyKey="PAY-LATE-0000001"});var lateId=Id(lateRegistration);
    var lateCheckout=await Web("POST",$"api/thanh-toan/{lateId}/phien",new{});var lateCode=T(lateCheckout,"maThanhToan");
    await using(var db=new CheckDbContext(options)){var row=await db.DangKy.SingleAsync(x=>x.Id==lateId);row.HanGiuCho=BusinessClock.Now.AddMinutes(-1);await db.SaveChangesAsync();}
    await Hook(new{Id=910008,AccountNumber="0000000000",TransferType="in",TransferAmount=100,Content=lateCode});
    Check(!ArtApiClient.Get(await Web("GET",$"api/thanh-toan/{lateId}/trang-thai"),"daXacNhan").GetBoolean(),"PAY14 Tiền đến sau hạn giữ chỗ không tự khôi phục đăng ký");
    using(var outsider=new HttpClient{BaseAddress=new Uri(address)}){using var denied=await outsider.GetAsync($"api/thanh-toan/{paymentId}/trang-thai");Check(denied.StatusCode==HttpStatusCode.Unauthorized,"PAY15 Chưa đăng nhập không đọc được phiếu thanh toán");}
    long freeClassId;
    await using(var db=new CheckDbContext(options)){
        var sourceCourse=await db.KhoaHoc.SingleAsync(x=>x.Id==courseId);var freeCourse=new KhoaHoc{LoaiKhoaHocId=sourceCourse.LoaiKhoaHocId,MaKhoa="FREE_TEST",TenKhoa="Trải nghiệm 0đ kiểm thử",PhienBan=1,CapDo="JUNIOR",SoBuoi=1,SoTietMoiBuoi=2,PhutMoiTiet=30,HocPhi=0,HieuLucTu=BusinessClock.Today,TrangThai="HOAT_DONG"};db.Add(freeCourse);await db.SaveChangesAsync();
        var first=BusinessClock.Today.AddDays(60).ToDateTime(new TimeOnly(9,0));var c=new LopHoc{KhoaHocId=freeCourse.Id,MaLop="FREE_TEST",TenLop="Trải nghiệm 0đ",NgayKhaiGiangDuKien=DateOnly.FromDateTime(first),SoBuoiKeHoach=1,LichHocDuKien="09:00–10:00",SiSoToiThieu=1,SiSoToiDa=10,HocPhiApDung=0,MoDangKyLuc=BusinessClock.Now.AddDays(-1),DongDangKyLuc=first.AddDays(-2),TrangThai="DANG_TUYEN_SINH"};db.Add(c);var l=new NoiDungBuoi{KhoaHocId=freeCourse.Id,ThuTu=1,ChuDe="Thử màu",NoiDung="Thử màu",SoTiet=2};db.Add(l);await db.SaveChangesAsync();freeClassId=c.Id;db.BuoiHoc.Add(new(){LopHocId=c.Id,NoiDungBuoiId=l.Id,PhongHocId=await db.PhongHoc.Select(x=>x.Id).FirstAsync(),ThuTuTrongLop=1,LanXepLich=1,BatDau=first,KetThuc=first.AddHours(1),SoTiet=2,PhutMoiTiet=30,TrangThai="DA_XEP_LICH"});await db.SaveChangesAsync();
    }
    var freeRegistration=await Web("POST","api/dang-ky",new{HocVienId=Id(lateStudent),LopHocId=freeClassId,NguoiDangKyId=memberId,IdempotencyKey="PAY-FREE-0000001"});var freeId=Id(freeRegistration);
    var freeView=await Web("POST",$"api/thanh-toan/{freeId}/phien",new{});
    Check(ArtApiClient.Get(freeView,"mienPhi").GetBoolean()&&ArtApiClient.Get(freeView,"daXacNhan").GetBoolean()&&ArtApiClient.Get(freeView,"conPhaiTra").GetDecimal()==0,"PAY16 Lớp 0đ xác nhận miễn phí và không yêu cầu quét QR");
    await using(var db=new CheckDbContext(options))Check(!await db.GiaoDichTien.AnyAsync(x=>x.DangKyId==freeId)&&!await db.Set<PhienThanhToan>().AnyAsync(x=>x.DangKyId==freeId),"PAY17 Lớp miễn phí không tạo giao dịch ngân hàng giả");

    var consultation=await Web("POST","api/tu-van",new{HoTen="Nguyễn Thị Ánh",Email="test@example.invalid",DienThoai="0900000000",ChuDe="Hỏi lịch học",NoiDung="Có lớp màu nước cuối tuần không?"});
    var tickets=await data.All("api/tu-van");var ticket=tickets.Single(x=>T(x,"maTraCuu")==T(consultation,"maTraCuu"));
    var thread=await staff.Read("api/tu-van/"+Id(ticket));
    Check(T(thread,"trangThai")=="DA_DOC","WinForms mở tư vấn từ web và đánh dấu đã đọc");
    var answer=new{NoiDung="Có lớp cuối tuần; trung tâm sẽ tư vấn lịch phù hợp.",RowVersion=T(thread,"rowVersion")};
    await staff.Post("api/tu-van/"+Id(ticket)+"/tra-loi",answer);
    var myTickets=await Web("GET","api/tu-van/toi");var mineTicket=myTickets.EnumerateArray().Single(x=>Id(x)==Id(ticket));
    var webThread=await Web("GET","api/tu-van/toi/"+Id(ticket));
    Check(T(mineTicket,"trangThai")=="DA_TRA_LOI"&&ArtApiClient.Get(webThread,"tinNhan").GetArrayLength()==2,"Phản hồi WinForms hiện đúng cuộc trao đổi trong tài khoản web");
    bool stale=false;try{await staff.Post("api/tu-van/"+Id(ticket)+"/tra-loi",answer);}catch(InvalidOperationException){stale=true;}
    Check(stale,"Chặn gửi phản hồi bằng RowVersion cũ");
    var notices=await Web("GET","api/thong-bao/toi");var notice=notices.EnumerateArray().Single(x=>T(x,"loai")=="TU_VAN");
    var noticeId=Id(notice);Check(T(notice,"noiDung").Contains(answer.NoiDung),"Trả lời tư vấn tạo thông báo web đúng thành viên");
    await Web("GET","api/thong-bao",expected:HttpStatusCode.Forbidden);
    await Web("POST","api/thong-bao/"+noticeId+"/da-doc",new{},HttpStatusCode.NoContent);
    await using(var notificationDb=new CheckDbContext(options)){
        Check(await notificationDb.ThongBao.CountAsync(x=>x.Loai=="TU_VAN")==1,"Trả lời bằng phiên bản cũ không tạo email trùng");
        Check((await notificationDb.ThongBao.SingleAsync(x=>x.Id==noticeId)).DaDocLuc!=null,"Đọc thông báo được lưu, học viên không xem được hàng đợi quản trị");
        notificationDb.ThongBao.Add(new(){ThanhVienId=null,Loai="TEST",TieuDe="Riêng tư",NoiDung="Không thuộc tài khoản",Email="private@example.invalid",TrangThaiEmail="KHONG_CO_EMAIL"});await notificationDb.SaveChangesAsync();
        var privateId=await notificationDb.ThongBao.Where(x=>x.Loai=="TEST").Select(x=>x.Id).SingleAsync();
        await Web("POST","api/thong-bao/"+privateId+"/da-doc",new{},HttpStatusCode.NotFound);
        var queued=await notificationDb.ThongBao.SingleAsync(x=>x.Id==noticeId);queued.Email="captured@example.test";queued.TrangThaiEmail="CHO_GUI";await notificationDb.SaveChangesAsync();
    }
    using(var worker=new NotificationWorker(app.Services.GetRequiredService<IServiceScopeFactory>(),Microsoft.Extensions.Logging.Abstractions.NullLogger<NotificationWorker>.Instance)){
        notificationMail.Fail=true;await worker.Deliver(default);
        await using(var notificationDb=new CheckDbContext(options)){var queued=await notificationDb.ThongBao.SingleAsync(x=>x.Id==noticeId);Check(queued.TrangThaiEmail=="CHO_GUI"&&queued.SoLanGui==1,"SMTP lỗi vẫn giữ thông báo web và lên lịch gửi lại");queued.ThuLaiLuc=BusinessClock.Now.AddSeconds(-1);await notificationDb.SaveChangesAsync();}
        notificationMail.Fail=false;await worker.Deliver(default);await worker.Deliver(default);
        await using(var notificationDb=new CheckDbContext(options))Check((await notificationDb.ThongBao.SingleAsync(x=>x.Id==noticeId)).TrangThaiEmail=="DA_GUI"&&notificationMail.Count==2,"Email gửi lại thành công và không tự gửi trùng sau khi hoàn tất");
    }
    await staff.Post("api/thong-bao/"+noticeId+"/da-goi",new{});
    await using(var notificationDb=new CheckDbContext(options))Check((await notificationDb.ThongBao.SingleAsync(x=>x.Id==noticeId)).DaGoiLuc!=null,"Nhân viên ghi nhận gọi điện riêng với trạng thái gửi email");
    var sessionToMove=(await staff.Read("api/lich-hoc")).EnumerateArray().First(x=>Id(x,"lopHocId")==classId);
    var movedStart=DateTime.Parse(T(sessionToMove,"batDau")).AddHours(1);
    await staff.Post("api/lich-hoc/buoi/"+Id(sessionToMove)+"/doi-lich",new{BatDau=Stamp(movedStart),PhongHocId=Id(sessionToMove,"phongHocId"),LyDo="Đổi giờ kiểm thử",RowVersion=T(sessionToMove,"rowVersion")});
    Check((await Web("GET","api/thong-bao/toi")).EnumerateArray().Any(x=>T(x,"loai")=="DOI_LICH"&&T(x,"noiDung").Contains("Đổi giờ kiểm thử")),"Đổi lịch từ WinForms/API tạo thông báo giờ cũ, giờ mới cho người đại diện học viên đã thanh toán");
    var latest=await staff.Read("api/tu-van/"+Id(ticket));await staff.Put("api/tu-van/"+Id(ticket)+"/trang-thai",new{TrangThai="DA_DONG",RowVersion=T(latest,"rowVersion")});
    Check(T(await Web("GET","api/tu-van/toi/"+Id(ticket)),"trangThai")=="DA_DONG","WinForms đóng tư vấn, web nhận trạng thái đã đóng");
    // Exercise the same POST/PUT/If-Match payloads used by WinForms catalog forms.
    foreach(var scenario in new (string Route,object Create,object Edit,string Key,string Expected)[]{
        ("giao-vien",new{MaGiaoVien="GVCRUD",HoTen="Giáo viên thử",DienThoai="0900000011",TrinhDo="Đại học",ChuyenMon="Màu nước",CapDoCoTheDay="BASIC",TrangThai="HOAT_DONG"},new{HoTen="Giáo viên đã sửa"},"hoTen","Giáo viên đã sửa"),
        ("phong-hoc",new{MaPhong="PCRUD",TenPhong="Phòng thử",SucChua=12,TrangThai="HOAT_DONG"},new{TenPhong="Phòng đã sửa"},"tenPhong","Phòng đã sửa"),
        ("hoa-cu",new{MaHoaCu="HCCRUD",TenHoaCu="Cọ thử",QuyCach="Số 2",DonViCoSo="Cái",NhomHoaCu="BAN_RIENG",GiaBanThamKhao=50000,MucDatHang=10,TrangThai="HOAT_DONG"},new{TenHoaCu="Cọ đã sửa"},"tenHoaCu","Cọ đã sửa"),
        ("nha-cung-cap",new{MaNCC="NCCCRUD",TenNCC="Nhà cung cấp thử",DienThoai="0900000012",TrangThai="HOAT_DONG"},new{TenNCC="Nhà cung cấp đã sửa"},"tenNCC","Nhà cung cấp đã sửa")
    }){
        var path="api/quan-ly/"+scenario.Route;var item=await staff.Post(path,scenario.Create);
        Check((await data.All(path)).Any(x=>Id(x)==Id(item)),"Thêm "+scenario.Route+" xuất hiện trong danh sách WinForms");
        var savedItem=await staff.Put(path+"/"+Id(item),scenario.Edit,T(item,"rowVersion"));
        Check(T(await staff.Read(path+"/"+Id(item)),scenario.Key)==scenario.Expected,"Sửa "+scenario.Route+" được lưu trên API");
        bool staleEdit=false;try{await staff.Put(path+"/"+Id(item),scenario.Edit,T(item,"rowVersion"));}catch(InvalidOperationException){staleEdit=true;}
        Check(staleEdit,"Chặn ghi đè phiên bản cũ của "+scenario.Route);
        await staff.Put(path+"/"+Id(item),new{TrangThai="NGUNG"},T(savedItem,"rowVersion"));
        Check(T(await staff.Read(path+"/"+Id(item)),"trangThai")=="NGUNG","Ngừng "+scenario.Route+" vẫn giữ bản ghi lịch sử");
    }
    var member=await staff.Read("api/quan-ly/thanh-vien/"+memberId);
    await staff.Put("api/quan-ly/thanh-vien/"+memberId,new{HoTen="Nguyễn Thị Ánh – đã cập nhật",DienThoai="0900000022"},T(member,"rowVersion"));
    var memberOnWeb=await Web("GET","api/tai-khoan/toi");Check(T(memberOnWeb,"hoTen").Contains("đã cập nhật")&&T(memberOnWeb,"dienThoai")=="0900000022","Sửa người liên hệ từ WinForms đồng bộ thông tin tài khoản web");
    var tempCourse=await staff.Post("api/quan-ly/khoa-hoc",new{LoaiKhoaHocId=Id((await data.All("api/quan-ly/loai-khoa-hoc")).First()),MaKhoa="CRUD",TenKhoa="Khóa thử",MucTieu="Quan sát và sáng tạo",PhienBan=1,CapDo="BASIC",TuoiToiThieu=6,SoBuoi=1,SoTietMoiBuoi=3,PhutMoiTiet=30,HocPhi=500000,HieuLucTu=BusinessClock.Today.ToString("yyyy-MM-dd"),TrangThai="NHAP"});
    Check(T(tempCourse,"maKhoa").StartsWith("KHOA-")&&T(tempCourse,"maKhoa")!="CRUD","API sinh mã khóa và bỏ qua mã nhập thủ công");
    var tempContent=await staff.Post("api/quan-ly/noi-dung-buoi",new{KhoaHocId=Id(tempCourse),ThuTu=1,ChuDe="Vẽ thử",NoiDung="Quan sát và vẽ",SoTiet=3});
    tempContent=await staff.Put("api/quan-ly/noi-dung-buoi/"+Id(tempContent),new{ChuDe="Buổi học đã sửa"},T(tempContent,"rowVersion"));
    tempCourse=await staff.Put("api/quan-ly/khoa-hoc/"+Id(tempCourse),new{TenKhoa="Khóa đã sửa",TrangThai="HOAT_DONG"},T(tempCourse,"rowVersion"));
    Check(T(await Web("GET","api/khoa-hoc/"+Id(tempCourse)),"tenKhoa")=="Khóa đã sửa","Thêm / sửa / công bố khóa từ WinForms hiển thị trên web");
    Check(T((await Web("GET","api/khoa-hoc/"+Id(tempCourse)+"/noi-dung"))[0],"chuDe")=="Buổi học đã sửa","Sửa nội dung buổi từ WinForms được web đọc lại");
    var draftClass=await staff.Post("api/quan-ly/lop-hoc",new{KhoaHocId=Id(tempCourse),MaLop="CRUDCLASS",TenLop="Lớp nháp thử",NgayKhaiGiangDuKien=BusinessClock.Today.AddDays(40).ToString("yyyy-MM-dd"),SoBuoiKeHoach=1,LichHocDuKien="Thứ Bảy 09:00",SiSoToiThieu=1,SiSoToiDa=10,HocPhiApDung=500000,MoDangKyLuc=Stamp(BusinessClock.Now),DongDangKyLuc=Stamp(BusinessClock.Now.AddDays(30)),TrangThai="NHAP"});
    Check(T(draftClass,"maLop").StartsWith("KHOA-L")&&T(draftClass,"maLop")!="CRUDCLASS","API sinh mã lớp theo tiền tố khóa và bỏ qua mã nhập thủ công");
    draftClass=await staff.Put("api/quan-ly/lop-hoc/"+Id(draftClass),new{TenLop="Lớp nháp đã sửa"},T(draftClass,"rowVersion"));
    Check(T(await staff.Read("api/quan-ly/lop-hoc/"+Id(draftClass)),"tenLop")=="Lớp nháp đã sửa","Thêm / sửa lớp từ WinForms được lưu");
    await Web("GET","api/lop-hoc/"+Id(draftClass),expected:HttpStatusCode.NotFound);Check(true,"Lớp nháp từ WinForms không bị công khai nhầm trên web");
    var scheduleTeacher=await staff.Post("api/quan-ly/giao-vien",new{MaGiaoVien="GVSCHEDULE",HoTen="Giáo viên xếp lịch",DienThoai="0900000033",TrinhDo="Đại học",ChuyenMon="Hội họa",CapDoCoTheDay="BASIC",TrangThai="HOAT_DONG"});
    await staff.Post("api/tai-khoan",new{TenDangNhap="teacher_name_test",MatKhau=password,HoTen="Tên nhân sự dự phòng",VaiTro="GIAO_VIEN",GiaoVienId=Id(scheduleTeacher),QuyenBoSung=0,Email="teacher-name@example.invalid"});
    using(var teacherClient=new ArtApiClient(baseUrl:address)){
        await teacherClient.Login("teacher_name_test",password);
        Check(T(await teacherClient.Read("api/tai-khoan/toi"),"hoTen")=="Giáo viên xếp lịch","Lời chào giáo viên dùng họ tên hồ sơ liên kết thay vì tên đăng nhập hoặc tên nhân sự dự phòng");
        var teacherUpdated=await staff.Put("api/quan-ly/giao-vien/"+Id(scheduleTeacher),new{HoTen="Nguyễn Thị Giáo Viên"},T(scheduleTeacher,"rowVersion"));
        Check(T(await teacherClient.Read("api/tai-khoan/toi"),"hoTen")=="Nguyễn Thị Giáo Viên","Sửa tên giáo viên được tài khoản web đọc lại ngay");
        var noticeTeacherId=Id(scheduleTeacher);
        await using(var db=new CheckDbContext(options)){
            var teachingSessions=await db.BuoiHoc.Where(x=>x.LopHocId==classId&&x.TrangThai=="DA_XEP_LICH").ToListAsync();
            foreach(var b in teachingSessions)db.GiangDayBuoi.Add(new(){BuoiHocId=b.Id,GiaoVienId=noticeTeacherId,VaiTro="CHINH",SoTietThucDay=b.SoTiet,NguoiDuyet=1});
            await db.SaveChangesAsync();
        }
        var recipients=await teacherClient.Read($"api/giao-vien/lop/{classId}/nguoi-nhan");
        Check(Id(recipients,"soHocVien")==2&&Id(recipients,"soNguoiNhan")==1,"Thông báo cả lớp chỉ lấy học viên đã xác nhận và gộp người đại diện trùng");
        await Web("GET",$"api/giao-vien/lop/{classId}/nguoi-nhan",expected:HttpStatusCode.Forbidden);
        var deniedClass=false;try{await teacherClient.Read($"api/giao-vien/lop/{Id(draftClass)}/nguoi-nhan");}catch(InvalidOperationException){deniedClass=true;}
        Check(deniedClass,"Giáo viên không đọc người nhận của lớp chưa được phân công; học viên không có quyền gửi cả lớp");
        var broadcastKey=Guid.NewGuid().ToString("N");var broadcastBody=new{TieuDe="Lưu ý lịch tuần tới",NoiDung="Mang theo bài thực hành để trao đổi tại lớp.",MaYeuCau=broadcastKey};
        await teacherClient.Post($"api/giao-vien/lop/{classId}/thong-bao",broadcastBody);await teacherClient.Post($"api/giao-vien/lop/{classId}/thong-bao",broadcastBody);
        await using(var db=new CheckDbContext(options))Check(await db.ThongBao.CountAsync(x=>x.Loai=="THONG_BAO_LOP")==1&&await db.Set<ThongBaoLop>().CountAsync()==1,"Gửi thông báo lớp có khóa chống trùng; bấm lại không tạo thêm email");
        Check((await Web("GET","api/thong-bao/toi")).EnumerateArray().Any(x=>T(x,"loai")=="THONG_BAO_LOP"),"Thông báo từ giáo viên hiện trong hộp thông báo người đại diện");
        var mismatched=false;try{await teacherClient.Post($"api/giao-vien/lop/{classId}/thong-bao",new{TieuDe="Nội dung khác",NoiDung="Không được dùng lại khóa gửi khác nội dung.",MaYeuCau=broadcastKey});}catch(InvalidOperationException){mismatched=true;}
        Check(mismatched,"Không tái sử dụng khóa gửi để thay nội dung thông báo đã ghi");
        var throttled=false;try{await teacherClient.Post($"api/giao-vien/lop/{classId}/thong-bao",new{TieuDe="Tin thứ hai",NoiDung="Không gửi dồn nhiều tin cho cùng lớp.",MaYeuCau=Guid.NewGuid().ToString("N")});}catch(InvalidOperationException){throttled=true;}
        Check(throttled,"Giới hạn gửi dồn thông báo mới; khóa retry cũ vẫn được nhận");
        var teaching=(await teacherClient.Read("api/hoc-tap/giao-vien/buoi-cua-toi")).EnumerateArray().OrderBy(x=>T(x,"batDau")).First();
        var newTime=DateTime.Parse(T(teaching,"batDau")).AddMinutes(30);var teachingId=Id(teaching);var teachingRoom=Id(teaching,"phongHocId");
        await Web("POST",$"api/giao-vien/buoi/{teachingId}/xep-bu",new{BatDau=Stamp(newTime),PhongHocId=teachingRoom,LyDo="Thử quyền học viên",RowVersion=T(teaching,"rowVersion")},HttpStatusCode.Forbidden);
        var rejectedRoom=false;try{await teacherClient.Post($"api/giao-vien/buoi/{teachingId}/xep-bu",new{BatDau=Stamp(newTime),PhongHocId=teachingRoom+10000,LyDo="Thử đổi phòng ngoài quyền giáo viên",RowVersion=T(teaching,"rowVersion")});}catch(InvalidOperationException){rejectedRoom=true;}
        await using(var db=new CheckDbContext(options))Check(rejectedRoom&&(await db.BuoiHoc.SingleAsync(x=>x.Id==teachingId)).TrangThai=="DA_XEP_LICH","Lịch không bị hủy nếu giáo viên yêu cầu đổi phòng ngoài quyền");
        var makeup=await teacherClient.Post($"api/giao-vien/buoi/{teachingId}/xep-bu",new{BatDau=Stamp(newTime),PhongHocId=teachingRoom,LyDo="Lớp nghỉ và học vào giờ thay thế",RowVersion=T(teaching,"rowVersion")});
        var makeupId=Id(makeup);
        await using(var db=new CheckDbContext(options)){
            Check((await db.BuoiHoc.SingleAsync(x=>x.Id==teachingId)).TrangThai=="DA_HUY"&&await db.BuoiHoc.CountAsync(x=>x.LopHocId==classId&&x.TrangThai=="DA_XEP_LICH")==2,"Xếp bù cả lớp thay buổi cũ, không tăng số buổi khóa");
            Check(await db.DiemDanh.AnyAsync(x=>x.BuoiHocId==makeupId&&x.DangKyId==enrollmentId&&x.TrangThai=="CHO_THAM_GIA"),"Điểm danh học viên đã thanh toán chuyển sang buổi thay thế");
            Check(await db.ThongBao.AnyAsync(x=>x.Loai=="DOI_LICH"&&x.NoiDung.Contains("Lớp nghỉ và học vào giờ thay thế")),"Xếp bù qua quyền giáo viên tự tạo thông báo lịch mới");
        }
        Check((await teacherClient.Read("api/hoc-tap/giao-vien/buoi-cua-toi")).EnumerateArray().Any(x=>Id(x)==makeupId&&Id(x,"buoiThayTheChoId")==teachingId),"Lịch giáo viên đánh dấu đúng buổi thay thế và ẩn buổi đã hủy");
    }
    var selectedSaturday=ClassPlanRules.NextDay(BusinessClock.Today.AddDays(40).ToDateTime(TimeOnly.MinValue),6).AddHours(9);
    var scheduleWorkflow=new ClassScheduleWorkflow(staff,Id(draftClass));
    await scheduleWorkflow.Save(new{PhongHocId=Id((await data.All("api/quan-ly/phong-hoc")).First()),GiaoVienId=Id(scheduleTeacher),BuoiDau=Stamp(selectedSaturday),CachTuan=1,PhutNghi=0,XacNhanDuChuyenMon=true},_=>{});
    var actualSchedule=JsonSerializer.SerializeToElement((await data.All("api/lich-hoc",false)).Where(x=>Id(x,"lopHocId")==Id(draftClass)).ToArray());
    var scheduledFirst=ArtApiClient.Get(actualSchedule[0],"batDau").GetDateTime();
    Check(actualSchedule.GetArrayLength()==1&&scheduledFirst.DayOfWeek==DayOfWeek.Saturday&&scheduledFirst.Hour==9,"Xếp lịch từ lựa chọn thứ/giờ tạo đúng buổi thực tế");
    draftClass=await staff.Read("api/quan-ly/lop-hoc/"+Id(draftClass));
    Check(T(draftClass,"trangThai")=="DANG_TUYEN_SINH","Xác nhận xếp lịch từ Win tự chuyển nháp thành Đang tuyển sinh trên SQL Server");
    var classOnWeb=await Web("GET","api/lop-hoc/"+Id(draftClass));
    Check(T(classOnWeb,"lichHocDuKien")==ClassPlanRules.Describe(6,new TimeSpan(9,0,0),90,1),"Web nhận đúng Thứ Bảy 09:00–10:30, khớp lịch điểm danh");
    await staff.Put("api/quan-ly/khoa-hoc/"+Id(tempCourse),new{TrangThai="NGUNG"},T(tempCourse,"rowVersion"));
    await Web("GET","api/khoa-hoc/"+Id(tempCourse),expected:HttpStatusCode.NotFound);Check(true,"Ngừng khóa trên WinForms: web không còn công khai khóa");
    var tempOffer=await staff.Post("api/quan-ly/khuyen-mai",new{MaKhuyenMai="CRUD_OFFER",TenKhuyenMai="Ưu đãi thử",TuDongApDung=true,KhoaApDungId=courseId,NhomKhoanThu="HOC_PHI",LoaiGiam="PHAN_TRAM",GiaTriGiam=5,TongLuot=100,BatDau=Stamp(BusinessClock.Now.AddDays(-1)),KetThuc=Stamp(BusinessClock.Now.AddDays(1)),TrangThai="HOAT_DONG"});
    tempOffer=await staff.Put("api/quan-ly/khuyen-mai/"+Id(tempOffer),new{GiaTriGiam=7},T(tempOffer,"rowVersion"));
    Check((await Web("GET","api/khuyen-mai")).EnumerateArray().Any(x=>T(x,"tenKhuyenMai")=="Ưu đãi thử"&&ArtApiClient.Get(x,"giaTriGiam").GetDecimal()==7),"Sửa ưu đãi từ WinForms được web cập nhật 7%");
    await staff.Put("api/quan-ly/khuyen-mai/"+Id(tempOffer),new{TrangThai="NGUNG"},T(tempOffer,"rowVersion"));
    Check(!(await Web("GET","api/khuyen-mai")).EnumerateArray().Any(x=>T(x,"tenKhuyenMai")=="Ưu đãi thử"),"Ngừng ưu đãi trên WinForms: biến mất khỏi web");
    var stoppedStudent=await staff.Read("api/quan-ly/hoc-vien/"+Id(student));
    await staff.Put("api/quan-ly/hoc-vien/"+Id(student),new{TrangThai="NGUNG"},T(stoppedStudent,"rowVersion"));
    Check((await Web("GET","api/hoc-vien/toi")).EnumerateArray().Any(x=>Id(x,"hocVienId")==Id(student)&&T(x,"trangThai")=="NGUNG"),"Ngừng học viên từ WinForms: web nhận trạng thái, không mất lịch sử lớp");
    Check((await Web("GET","api/dang-ky/toi")).EnumerateArray().Any(x=>Id(x)==enrollmentId),"Ngừng học viên không xóa đăng ký và học phí đã thu");
    await staff.Put("api/quan-ly/dai-dien/"+Id(link),new{HieuLucDen=Stamp(BusinessClock.Now),LaLienHeChinh=false},T(link,"rowVersion"));
    Check(!(await Web("GET","api/hoc-vien/toi")).EnumerateArray().Any(x=>Id(x,"hocVienId")==Id(student)),"Gỡ liên kết trên WinForms: web mất quyền với đúng hồ sơ");
    await staff.Post("api/tai-khoan",new{TenDangNhap="cashiercrud",MatKhau=password,VaiTro="THU_NGAN",HoTen="Thu ngân thử",Email="cashier@example.invalid",QuyenBoSung=0});
    await using(var db=new CheckDbContext(options))Check((await db.TaiKhoan.SingleAsync(x=>x.TenDangNhap=="cashiercrud")).Email=="cashier@example.invalid","Tạo tài khoản nhân viên từ Win lưu email để khôi phục mật khẩu");
    using(var cashier=new ArtApiClient(baseUrl:address)){
        await cashier.Login("cashiercrud",password);bool blocked=false;try{await cashier.Post("api/quan-ly/hoc-vien",new{MaHocVien="HVNO",HoTen="Không được lưu",NgaySinh="2000-01-01",LienHeKhanCapTen="Liên hệ",LienHeKhanCapSDT="0900000000",TrangThai="HOAT_DONG"});}catch(InvalidOperationException){blocked=true;}
        Check(blocked,"Thu ngân không được thêm học viên thay học vụ");
        var account=(await data.All("api/tai-khoan",false)).Single(x=>T(x,"tenDangNhap")=="cashiercrud");
        await staff.Put("api/tai-khoan/"+Id(account)+"/quyen",new{VaiTro="THU_NGAN",QuyenBoSung=0,HoatDong=false});
        bool revoked=false;try{await cashier.Read("api/tai-khoan/toi");}catch(ApiSessionExpiredException){revoked=true;}
        Check(revoked,"Khóa tài khoản trên WinForms thu hồi ngay phiên đang dùng");
    }
    await staff.Post("api/tai-khoan/doi-mat-khau",new{MatKhauCu=password,MatKhauMoi="WinFormsNew2026!"});
    bool expired=false;try{await staff.Read("api/tai-khoan/toi");}catch(ApiSessionExpiredException){expired=true;}
    Check(expired,"Đổi mật khẩu từ WinForms thu hồi phiên cũ");
    await staff.Login("root","WinFormsNew2026!");Check(T(await staff.Read("api/tai-khoan/toi"),"tenDangNhap")=="root","Đăng nhập lại thành công bằng mật khẩu mới");
    Check(recoveryMail.Count==0,"Tạo tài khoản và nghiệp vụ bình thường không gửi email xác minh hoặc chào mừng");
    web.DefaultRequestHeaders.Authorization=null;
    var unknown=await Web("POST","api/tai-khoan/quen-mat-khau",new{TenDangNhap="not_present"});
    var mismatch=await Web("POST","api/tai-khoan/quen-mat-khau",new{TenDangNhap="not_present_2"});
    Check(T(unknown,"thongBao")==T(mismatch,"thongBao")&&recoveryMail.Count==0,"Tên không tồn tại trả thông báo chung và không gửi thư");
    var forgot=await Web("POST","api/tai-khoan/quen-mat-khau",new{TenDangNhap="phuhuynh",Email="attacker@example.invalid"});var recoveryCode=recoveryMail.LastCode;
    Check(recoveryMail.Count==1&&recoveryCode.Length==6&&recoveryMail.LastRecipient=="parent@example.invalid","Mã 6 chữ số chỉ gửi email trong DB, không nhận địa chỉ khác từ client");
    Check(T(forgot,"emailNhanDaAn")=="par***@e***.invalid"&&!forgot.GetRawText().Contains("parent@example.invalid"),"Thông báo chỉ trả email che một phần, không trả địa chỉ đầy đủ");
    await using(var db=new CheckDbContext(options)){
        var token=await db.KhoiPhucTaiKhoan.SingleAsync();Check(token.MaBam!=recoveryCode&&token.MaBam.Length>64&&token.HetHanLuc>BusinessClock.Now,"Database lưu mã băm chậm có salt, không lưu OTP gốc");
    }
    var cooldown=await Web("POST","api/tai-khoan/quen-mat-khau",new{TenDangNhap="phuhuynh",Email="parent@example.invalid"});
    Check(recoveryMail.Count==1&&T(cooldown,"maYeuCau")==T(forgot,"maYeuCau"),"Trong 60 giây giữ đúng yêu cầu trước để mã đã nhận vẫn dùng được, không gửi thêm thư");
    var badCode=recoveryCode=="000000"?"111111":"000000";
    for(var attempt=0;attempt<5;attempt++)await Web("POST","api/tai-khoan/dat-lai-mat-khau",new{MaYeuCau=T(forgot,"maYeuCau"),Ma=badCode,MatKhauMoi="NewRecovery2026!"},HttpStatusCode.BadRequest);
    await Web("POST","api/tai-khoan/dat-lai-mat-khau",new{MaYeuCau=T(forgot,"maYeuCau"),Ma=recoveryCode,MatKhauMoi="NewRecovery2026!"},HttpStatusCode.BadRequest);Check(true,"Đủ 5 lần sai: mã bị vô hiệu dù nhập đúng sau đó");
    async Task AgeRecovery(){await using var db=new CheckDbContext(options);await db.KhoiPhucTaiKhoan.ExecuteUpdateAsync(s=>s.SetProperty(x=>x.CreatedAt,BusinessClock.Now.AddMinutes(-2)));}
    await AgeRecovery();var expiredRecovery=await Web("POST","api/tai-khoan/quen-mat-khau",new{TenDangNhap="phuhuynh",Email="parent@example.invalid"});var expiredCode=recoveryMail.LastCode;
    var expiredRecoveryId=T(expiredRecovery,"maYeuCau");
    await using(var db=new CheckDbContext(options))await db.KhoiPhucTaiKhoan.Where(x=>x.MaYeuCau==expiredRecoveryId).ExecuteUpdateAsync(s=>s.SetProperty(x=>x.HetHanLuc,BusinessClock.Now.AddMinutes(-1)));
    await Web("POST","api/tai-khoan/dat-lai-mat-khau",new{MaYeuCau=T(expiredRecovery,"maYeuCau"),Ma=expiredCode,MatKhauMoi="NewRecovery2026!"},HttpStatusCode.BadRequest);Check(true,"Mã quá hạn không đặt lại được mật khẩu");
    await AgeRecovery();var beforeNew=await Web("POST","api/tai-khoan/quen-mat-khau",new{TenDangNhap="phuhuynh",Email="parent@example.invalid"});var beforeNewCode=recoveryMail.LastCode;
    await AgeRecovery();var validRecovery=await Web("POST","api/tai-khoan/quen-mat-khau",new{TenDangNhap="phuhuynh",Email="parent@example.invalid"});var validCode=recoveryMail.LastCode;
    await Web("POST","api/tai-khoan/dat-lai-mat-khau",new{MaYeuCau=T(beforeNew,"maYeuCau"),Ma=beforeNewCode,MatKhauMoi="NewRecovery2026!"},HttpStatusCode.BadRequest);Check(true,"Gửi mã mới vô hiệu mã trước");
    var oldLogin=await Web("POST","api/tai-khoan/dang-nhap",new{TenDangNhap="phuhuynh",MatKhau=password});
    await Web("POST","api/tai-khoan/dat-lai-mat-khau",new{MaYeuCau=T(validRecovery,"maYeuCau"),Ma=validCode,MatKhauMoi="NewRecovery2026!"},HttpStatusCode.NoContent);
    await Web("POST","api/tai-khoan/dat-lai-mat-khau",new{MaYeuCau=T(validRecovery,"maYeuCau"),Ma=validCode,MatKhauMoi="OtherRecovery2026!"},HttpStatusCode.BadRequest);Check(true,"Đặt lại thành công; mã chỉ dùng một lần");
    await Web("POST","api/tai-khoan/dang-nhap",new{TenDangNhap="phuhuynh",MatKhau=password},HttpStatusCode.Unauthorized);
    var newLogin=await Web("POST","api/tai-khoan/dang-nhap",new{TenDangNhap="phuhuynh",MatKhau="NewRecovery2026!"});Check(T(newLogin,"accessToken").Length>0,"Mật khẩu cũ bị từ chối, mật khẩu mới đăng nhập được");
    web.DefaultRequestHeaders.Authorization=new AuthenticationHeaderValue("Bearer",T(oldLogin,"accessToken"));await Web("GET","api/tai-khoan/toi",expected:HttpStatusCode.Unauthorized);web.DefaultRequestHeaders.Authorization=null;Check(true,"Đặt lại mật khẩu thu hồi phiên web/WinForms cũ");
    recoveryMail.Configured=false;await Web("POST","api/tai-khoan/quen-mat-khau",new{TenDangNhap="phuhuynh",Email="parent@example.invalid"},HttpStatusCode.ServiceUnavailable);Check(true,"Chưa cấu hình Gmail: báo chưa bật gửi email, không giả vờ đã gửi");
    var emailTemplate=RecoveryEmailTemplate.Create("000000");
    Check(emailTemplate.Html.Contains("000000")&&emailTemplate.Html.Contains("cid:"+RecoveryEmailTemplate.LogoContentId),"Recovery HTML includes selectable code and embedded brand logo");
    Check(emailTemplate.PlainText.Contains("000000")&&emailTemplate.PlainText.Contains("10 phút")&&!emailTemplate.Subject.Contains("000000"),"Recovery has plain text fallback and keeps code out of subject");
    using(var emailLogo=RecoveryEmailTemplate.OpenLogo())Check(emailLogo.Length>0,"Recovery logo is bundled with API deployment");
    var invalidCodeRejected=false;try{RecoveryEmailTemplate.Create("<test>");}catch(ArgumentException){invalidCodeRejected=true;}
    Check(invalidCodeRejected,"Recovery template rejects non-digit code input");
    await BusinessIntegrityChecks.Run(staff,options,Check);
    await ClassInventoryChecks.Run(staff,app.Services,options,Check);
    var receipt=await staff.Read("api/giao-dich/phieu-thu/"+Id(receiptPayment));
    Check(T(receipt,"maChungTu")=="PT-CHECK-01"&&ArtApiClient.Get(receipt,"soTien").GetDecimal()==85000&&
        T(receipt,"tenHocVien")!=""&&T(receipt,"tenLop")!="","PT01 Phiếu thu đọc đúng số tiền, chứng từ, học viên và lớp");
    var receiptList=await staff.Read("api/giao-dich/phieu-thu");
    Check(receiptList.EnumerateArray().Count(x=>Id(x)==Id(receiptPayment))==1&&
        !receiptList.EnumerateArray().Any(x=>T(x,"maChungTu")=="SP910005"),"PT02 Có thể mở lại phiếu đã thu, không in khoản tiền đang đối chiếu");
    Console.WriteLine($"ALL {checks} WINFORMS/API INTEGRATION CHECKS PASSED ({(sql?"SQL Server":"SQLite temporary store")}).");
}
finally
{
    await app.StopAsync();await app.DisposeAsync();
    if(storeCreated){await using var db=new CheckDbContext(options);await db.Database.EnsureDeletedAsync();}
    Microsoft.Data.Sqlite.SqliteConnection.ClearAllPools();
    if(Directory.Exists(folder))Directory.Delete(folder,true);
}
sealed class CaptureNotificationMail:INotificationMail
{
    public int Count{get;private set;}public bool Fail{get;set;}
    public Task Send(ThongBao message,CancellationToken ct){Count++;if(Fail)throw new InvalidOperationException("SMTP giả lập tạm lỗi");return Task.CompletedTask;}
}
sealed class CaptureRecoveryMail:IRecoveryMail
{
    public int Count;public string LastCode="",LastRecipient="";public bool Configured=true;
    public void EnsureConfigured(){if(!Configured)throw new ApiError(503,"Chưa bật gửi email khôi phục.");}
    public Task Send(string recipient,string code,bool verify,CancellationToken ct){EnsureConfigured();Count++;LastCode=code;LastRecipient=recipient;return Task.CompletedTask;}
}
