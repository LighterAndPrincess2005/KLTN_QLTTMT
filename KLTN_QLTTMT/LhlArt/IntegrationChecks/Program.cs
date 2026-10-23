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
builder.Services.AddScoped<LearningService>();builder.Services.AddScoped<ChangeService>();builder.Services.AddScoped<InventoryService>();builder.Services.AddScoped<ClassLifecycleService>();
builder.Services.AddAuthentication("Bearer").AddScheme<AuthenticationSchemeOptions,BearerAuthenticationHandler>("Bearer",_=>{});
builder.Services.AddAuthorization(o=>{
    o.FallbackPolicy=new AuthorizationPolicyBuilder().RequireAuthenticatedUser().Build();
    foreach(var p in Enum.GetValues<Permission>().Where(x=>x!=Permission.None))o.AddPolicy(p.ToString(),b=>b.RequireAuthenticatedUser().RequireAssertion(c=>p is Permission.TaiKhoan or Permission.NhatKy?c.User.IsInRole("QUAN_TRI"):c.User.Has(p)));
});
builder.Services.AddRateLimiter(o=>{o.AddFixedWindowLimiter("login",v=>{v.PermitLimit=1000;v.Window=TimeSpan.FromMinutes(1);});o.AddFixedWindowLimiter("consultation",v=>{v.PermitLimit=1000;v.Window=TimeSpan.FromMinutes(1);});});
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
    await staff.Post("api/giao-dich/thu",new{DangKyId=enrollmentId,KhoanThuId=Id(charge),SoTien=85000,PhuongThuc="TIEN_MAT",MaChungTu="PT-CHECK-01",IdempotencyKey="WIN-RECEIPT-0001"});
    var registrations=await Web("GET","api/dang-ky/toi");Check(registrations.EnumerateArray().Any(x=>Id(x)==enrollmentId&&T(x,"trangThai")=="DA_XAC_NHAN"),"WinForms thu đủ học phí, web chuyển sang đã xác nhận");
    var webStudent=await Web("POST","api/hoc-vien/toi",new{HoTen="Nguyễn Bảo An",NgaySinh="2010-05-12",QuanHe="ME",LienHeKhanCapTen="Nguyễn Thị Ánh",LienHeKhanCapSDT="0900000000",MucTieuHoc="Học vẽ"});
    var webRegistration=await Web("POST","api/dang-ky",new{HocVienId=Id(webStudent),LopHocId=classId,NguoiDangKyId=memberId,IdempotencyKey="WEB-INTEGRATION-0001"});
    var fromWeb=ArtApiClient.Get(await staff.Read("api/dang-ky/"+Id(webRegistration)),"dangKy");Check(Id(fromWeb)==Id(webRegistration),"Web ghi danh, WinForms đọc được bằng ID");
    Check(ApiDataService.Channel(fromWeb)=="Chưa được API ghi nhận","Xác nhận API hiện chưa lưu kênh online/offline");
    try{await staff.Read("api/dang-ky/quan-ly");throw new Exception("Endpoint unexpectedly available; update compatibility checks");}catch(ApiNotFoundException){Check(true,"Xác nhận thiếu endpoint tổng danh sách; WinForms dùng tra cứu ID");}
    await staff.Put("api/quan-ly/dai-dien/"+Id(link),new{HieuLucDen=Stamp(BusinessClock.Now),LaLienHeChinh=false},T(link,"rowVersion"));
    Check(!(await Web("GET","api/hoc-vien/toi")).EnumerateArray().Any(x=>Id(x,"hocVienId")==Id(student)),"Gỡ liên kết trên WinForms: web mất quyền với đúng hồ sơ");
    Console.WriteLine($"ALL {checks} WINFORMS/API INTEGRATION CHECKS PASSED ({(sql?"SQL Server":"SQLite temporary store")}).");
}
finally
{
    await app.StopAsync();await app.DisposeAsync();
    if(storeCreated){await using var db=new CheckDbContext(options);await db.Database.EnsureDeletedAsync();}
    Microsoft.Data.Sqlite.SqliteConnection.ClearAllPools();
    if(Directory.Exists(folder))Directory.Delete(folder,true);
}
