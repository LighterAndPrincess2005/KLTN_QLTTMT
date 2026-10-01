using Microsoft.AspNetCore.Http;
using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Nodes;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using MyThuat.Api.Controllers;
using MyThuat.Api.Contracts;
using MyThuat.Api.Data;
using MyThuat.Api.Models;
using MyThuat.Api.Security;
using MyThuat.Api.Services;
using MyThuat.Checks;

var folder=Path.Combine(Path.GetTempPath(),"MyThuatChecks-"+Guid.NewGuid().ToString("N"));Directory.CreateDirectory(folder);
var sqlMode=args.Contains("--sqlserver");
var sqlDatabase="MyThuatChecks_"+Guid.NewGuid().ToString("N");
var options=sqlMode
    ?new DbContextOptionsBuilder<MyThuatDbContext>().UseSqlServer("Server=(localdb)\\MSSQLLocalDB;Database="+sqlDatabase+";Trusted_Connection=True;TrustServerCertificate=True").Options
    :new DbContextOptionsBuilder<MyThuatDbContext>().UseSqlite("Data Source="+Path.Combine(folder,"checks.db")).Options;
var builder=WebApplication.CreateBuilder(new WebApplicationOptions {EnvironmentName="Development",ApplicationName=typeof(AuthController).Assembly.FullName});
builder.WebHost.UseUrls("http://127.0.0.1:5199");
builder.Logging.ClearProviders();
builder.Services.AddControllers().AddApplicationPart(typeof(AuthController).Assembly);
builder.Services.AddScoped<MyThuatDbContext>(_=>new CheckDbContext(options));
builder.Services.AddSingleton<IDataProtectionProvider>(new EphemeralDataProtectionProvider());
builder.Services.AddSingleton<AccessTokens>();builder.Services.AddScoped<IPasswordHasher<TaiKhoan>,PasswordHasher<TaiKhoan>>();
builder.Services.AddHttpContextAccessor();
builder.Services.AddScoped<AuditService>();builder.Services.AddScoped<AccessService>();builder.Services.AddScoped<PolicyService>();
builder.Services.AddScoped<ManagementValidation>();builder.Services.AddScoped<ManagementService>();builder.Services.AddScoped<ScheduleService>();
builder.Services.AddScoped<EnrollmentService>();builder.Services.AddScoped<FinanceService>();builder.Services.AddScoped<LearningService>();
builder.Services.AddScoped<ChangeService>();builder.Services.AddScoped<InventoryService>();builder.Services.AddScoped<ClassLifecycleService>();
builder.Services.AddAuthentication("Bearer").AddScheme<AuthenticationSchemeOptions,BearerAuthenticationHandler>("Bearer",_=>{});
builder.Services.AddAuthorization(o=>{
    o.FallbackPolicy=new AuthorizationPolicyBuilder().RequireAuthenticatedUser().Build();
    foreach(var p in Enum.GetValues<Permission>().Where(x=>x!=Permission.None))o.AddPolicy(p.ToString(),b=>b.RequireAuthenticatedUser().RequireAssertion(c=>
        p is Permission.TaiKhoan or Permission.NhatKy?c.User.IsInRole("QUAN_TRI"):c.User.Has(p)));
});
builder.Services.AddRateLimiter(o=>o.AddFixedWindowLimiter("login",v=>{v.PermitLimit=1000;v.Window=TimeSpan.FromMinutes(1);}));
var app=builder.Build();
app.Use(async(ctx,next)=>{try{await next(ctx);}catch(Exception ex){ctx.Response.StatusCode=ex is ApiError e?e.Status:ex is DbUpdateException?409:500;await ctx.Response.WriteAsJsonAsync(new{error=ex.Message});}});
app.UseRouting();app.UseRateLimiter();app.UseAuthentication();app.UseAuthorization();app.MapControllers();
var storeCreated=false;
var client=new HttpClient{BaseAddress=new Uri("http://127.0.0.1:5199")};
var count=0;string? adminToken=null;
async Task<JsonNode> Call(string method,string path,object? body=null,int expected=200,string? token=null,string? etag=null)
{
    using var req=new HttpRequestMessage(new HttpMethod(method),path);
    if(body is not null)req.Content=JsonContent.Create(body);
    if(token is not null)req.Headers.Authorization=new AuthenticationHeaderValue("Bearer",token);
    if(etag is not null)req.Headers.TryAddWithoutValidation("If-Match",etag);
    using var response=await client.SendAsync(req);
    var text=await response.Content.ReadAsStringAsync();
    if((int)response.StatusCode!=expected)throw new Exception($"{method} {path}: expected {expected}, got {(int)response.StatusCode}: {text}");
    count++;Console.WriteLine($"PASS {count:00}: {method} {path} -> {expected}");
    return string.IsNullOrEmpty(text)?new JsonObject():JsonNode.Parse(text)!;
}
static long Id(JsonNode node)=>(node["Id"]??node["id"])!.GetValue<long>();
static string Version(JsonNode node)=>(node["RowVersion"]??node["rowVersion"])!.GetValue<string>();
async Task<string> Login(string name)=> (await Call("POST","/api/tai-khoan/dang-nhap",new LoginRequest(name,"MyThuatTest2026!")))["accessToken"]!.GetValue<string>();
long teacherId,otherTeacher,memberId,studentId,secondStudent,courseId,roomId,room2,classA,classB,materialId,toolId,supplierId;
try
{
    await using(var db=new CheckDbContext(options))await db.Database.EnsureCreatedAsync();
    storeCreated=true;
    await app.StartAsync();
    await Call("GET","/api/quan-ly/hoc-vien",expected:401);
    await Call("POST","/api/tai-khoan/thiet-lap",new AdminSetupRequest("root","MyThuatTest2026!","Quản trị kiểm thử"));
    await Call("POST","/api/tai-khoan/thiet-lap",new AdminSetupRequest("again","MyThuatTest2026!","Không được tạo"),409);
    adminToken=await Login("root");
    var ruleList=await Call("GET","/api/quan-ly/quy-dinh",token:adminToken);
    var rule=ruleList["duLieu"]![0]!;
    await Call("PUT","/api/quan-ly/quy-dinh/"+Id(rule),new{TrangThai="HOAT_DONG"},token:adminToken,etag:Version(rule));
    await Call("POST","/api/quan-ly/hoc-vien",new{PasswordHash="not-allowed"},400,adminToken);
    await using(var db=new CheckDbContext(options))
    {
        var teacher=new GiaoVien{MaGiaoVien="GV1",HoTen="Giáo viên 1",DienThoai="0900000001",TrinhDo="Mỹ thuật",ChuyenMon="Vẽ",CapDoCoTheDay="CO_BAN",TrangThai="HOAT_DONG"};
        var teacher2=new GiaoVien{MaGiaoVien="GV2",HoTen="Giáo viên 2",DienThoai="0900000002",TrinhDo="Mỹ thuật",ChuyenMon="Vẽ",CapDoCoTheDay="CO_BAN",TrangThai="HOAT_DONG"};
        var member=new ThanhVien{MaThanhVien="TV1",HoTen="Người học",DienThoai="0900000003",XacNhanThanhVienLuc=BusinessClock.Now,TrangThai="HOAT_DONG"};
        var student=new HocVien{MaHocVien="HV1",HoTen="Học viên 1",NgaySinh=BusinessClock.Today.AddYears(-20),LienHeKhanCapTen="Liên hệ",LienHeKhanCapSDT="0900000003",TrangThai="HOAT_DONG"};
        var student2=new HocVien{MaHocVien="HV2",HoTen="Học viên 2",NgaySinh=BusinessClock.Today.AddYears(-20),LienHeKhanCapTen="Liên hệ",LienHeKhanCapSDT="0900000004",TrangThai="HOAT_DONG"};
        var room=new PhongHoc{MaPhong="P1",TenPhong="Phòng 1",SucChua=2,TrangThai="HOAT_DONG"};
        var roomOther=new PhongHoc{MaPhong="P2",TenPhong="Phòng 2",SucChua=2,TrangThai="HOAT_DONG"};
        var type=new LoaiKhoaHoc{MaLoai="CORE",TenLoai="Core",TrangThai="HOAT_DONG"};
        var material=new HoaCu{MaHoaCu="GIAY",TenHoaCu="Giấy",QuyCach="A4",DonViCoSo="Tờ",NhomHoaCu="TIEU_HAO",TrangThai="HOAT_DONG"};
        var tool=new HoaCu{MaHoaCu="CO",TenHoaCu="Cọ",QuyCach="Số 2",DonViCoSo="Cái",NhomHoaCu="DUNG_CHUNG",TrangThai="HOAT_DONG"};
        var supplier=new NhaCungCap{MaNCC="NCC1",TenNCC="Nhà cung cấp",DienThoai="0900000005",TrangThai="HOAT_DONG"};
        db.AddRange(teacher,teacher2,member,student,student2,room,roomOther,type,material,tool,supplier);await db.SaveChangesAsync();
        teacherId=teacher.Id;otherTeacher=teacher2.Id;memberId=member.Id;studentId=student.Id;secondStudent=student2.Id;
        roomId=room.Id;room2=roomOther.Id;materialId=material.Id;toolId=tool.Id;supplierId=supplier.Id;
        foreach(var hv in new[]{student,student2})
        {
            hv.CapDoDeXuat="CO_BAN";hv.NgayDanhGiaDauVao=BusinessClock.Today;hv.GiaoVienDanhGia=teacherId;
            db.DaiDienHocVien.Add(new(){ThanhVienId=member.Id,HocVienId=hv.Id,QuanHe="TU_BAN_THAN",QuyenDaiDien="DAY_DU",LaLienHeChinh=true,HieuLucTu=BusinessClock.Now.AddMinutes(-1),XacNhanLuc=BusinessClock.Now});
        }
        var course=new KhoaHoc{LoaiKhoaHocId=type.Id,MaKhoa="KH1",TenKhoa="Vẽ cơ bản",PhienBan=1,CapDo="CO_BAN",MucTieu="Luyện tập",SoBuoi=4,SoTietMoiBuoi=4,PhutMoiTiet=30,HocPhi=100000,HieuLucTu=BusinessClock.Today,TrangThai="HOAT_DONG"};
        db.KhoaHoc.Add(course);await db.SaveChangesAsync();courseId=course.Id;
        for(short i=1;i<=4;i++)db.NoiDungBuoi.Add(new(){KhoaHocId=course.Id,ThuTu=i,ChuDe="Buổi "+i,NoiDung="Vẽ",SoTiet=4});
        var start=BusinessClock.Today.AddDays(14).ToDateTime(new TimeOnly(10,0));
        var a=new LopHoc{KhoaHocId=course.Id,MaLop="A",TenLop="Lớp A",NgayKhaiGiangDuKien=DateOnly.FromDateTime(start),SoBuoiKeHoach=4,LichHocDuKien="Hàng tuần",SiSoToiThieu=1,SiSoToiDa=1,HocPhiApDung=100000,MoDangKyLuc=BusinessClock.Now.AddDays(-1),DongDangKyLuc=start.AddHours(-48),TrangThai="NHAP"};
        var b=new LopHoc{KhoaHocId=course.Id,MaLop="B",TenLop="Lớp B",NgayKhaiGiangDuKien=DateOnly.FromDateTime(start),SoBuoiKeHoach=4,LichHocDuKien="Hàng tuần",SiSoToiThieu=1,SiSoToiDa=1,HocPhiApDung=100000,MoDangKyLuc=BusinessClock.Now.AddDays(-1),DongDangKyLuc=start.AddHours(-48),TrangThai="NHAP"};
        db.LopHoc.AddRange(a,b);await db.SaveChangesAsync();classA=a.Id;classB=b.Id;
    }
    await Call("POST","/api/tai-khoan",new StaffAccountRequest("member","MyThuatTest2026!","THANH_VIEN","Thành viên",null,memberId),token:adminToken);
    await Call("POST","/api/tai-khoan",new StaffAccountRequest("teacher","MyThuatTest2026!","GIAO_VIEN","Giáo viên",teacherId,null),token:adminToken);
    await Call("POST","/api/tai-khoan",new StaffAccountRequest("cashier","MyThuatTest2026!","THU_NGAN","Thu ngân",null,null),token:adminToken);
    var memberToken=await Login("member");var teacherToken=await Login("teacher");var cashierToken=await Login("cashier");
    await Call("GET","/api/quan-ly/hoc-vien",expected:403,token:memberToken);
    await Call("GET","/api/quan-ly/hoc-vien",expected:403,token:cashierToken);
    await Call("GET","/api/tai-khoan",expected:403,token:teacherToken);
    await Call("PUT","/api/tai-khoan/1/quyen",new AccountRoleRequest("HOC_VU",0,true),409,adminToken);
    var first=BusinessClock.Today.AddDays(14).ToDateTime(new TimeOnly(10,0));
    var listA=await Call("POST",$"/api/lich-hoc/lop/{classA}/sinh-lich",new GenerateScheduleRequest(roomId,teacherId,first,XacNhanDuChuyenMon:true),token:adminToken);
    await Call("POST",$"/api/lich-hoc/lop/{classB}/sinh-lich",new GenerateScheduleRequest(roomId,otherTeacher,first,XacNhanDuChuyenMon:true),409,adminToken);
    var listB=await Call("POST",$"/api/lich-hoc/lop/{classB}/sinh-lich",new GenerateScheduleRequest(room2,otherTeacher,first,XacNhanDuChuyenMon:true),token:adminToken);
    foreach(var classId in new[]{classA,classB})
    {
        var cls=await Call("GET","/api/quan-ly/lop-hoc/"+classId,token:adminToken);
        await Call("PUT","/api/quan-ly/lop-hoc/"+classId,new{TrangThai="DANG_TUYEN_SINH"},token:adminToken,etag:Version(cls));
        await Call("PUT","/api/quan-ly/lop-hoc/"+classId,new{TenLop="stale"},409,adminToken,Version(cls));
    }
    await Call("GET",$"/api/hoc-tap/buoi/{Id(listB[0]!)}/danh-sach",expected:403,token:teacherToken);
    var request=new EnrollRequest(studentId,classA,memberId,"enroll-key-0001");
    var enrollment=await Call("POST","/api/dang-ky",request,token:memberToken);var enrollmentId=Id(enrollment);
    var replay=await Call("POST","/api/dang-ky",request,token:memberToken);if(Id(replay)!=enrollmentId)throw new Exception("Idempotency failed");
    await Call("POST","/api/dang-ky",request with{HocVienId=secondStudent},409,memberToken);
    await Call("POST","/api/dang-ky",request with{HocVienId=secondStudent,IdempotencyKey="enroll-key-0002"},409,memberToken);
    var detail=await Call("GET","/api/dang-ky/"+enrollmentId,token:memberToken);var charge=Id(detail["khoanThu"]![0]!);
    var partial=new ReceiptRequest(enrollmentId,charge,40000,"TIEN_MAT","CT01",null,"receive-0001");
    await Call("POST","/api/giao-dich/thu",partial,expected:403,token:memberToken);
    await Call("POST","/api/giao-dich/thu",partial,token:cashierToken);
    await Call("POST","/api/giao-dich/thu",partial,token:cashierToken);
    var debt=await Call("GET",$"/api/dang-ky/{enrollmentId}/cong-no",token:memberToken);
    if(debt["trangThai"]!.GetValue<string>()!="CHO_THANH_TOAN")throw new Exception("Partial payment confirmed admission");
    var cash=await Call("POST","/api/giao-dich/thu",partial with{SoTien=60000,MaChungTu="CT02",IdempotencyKey="receive-0002"},token:cashierToken);
    debt=await Call("GET",$"/api/dang-ky/{enrollmentId}/cong-no",token:memberToken);
    if(debt["trangThai"]!.GetValue<string>()!="DA_XAC_NHAN")throw new Exception("Full payment did not confirm");
    var excess=await Call("POST","/api/giao-dich/thu",partial with{SoTien=100,MaChungTu="CT03",IdempotencyKey="receive-excess"},token:cashierToken);
    if(excess["TrangThai"]!.GetValue<string>()!="CHO_DOI_CHIEU")throw new Exception("Excess payment was allocated");
    await Call("POST","/api/giao-dich/hoan",new RefundRequest(Id(excess),101,"HOAN03","refund-excess-bad"),409,cashierToken);
    await Call("POST","/api/giao-dich/hoan",new RefundRequest(Id(excess),100,"HOAN03","refund-excess-ok"),token:cashierToken);
    var change=await Call("POST","/api/thay-doi",new ChangeRequest(enrollmentId,"HUY","NGUOI_HOC","Đổi kế hoạch","cancel-key-01"),token:memberToken);
    await Call("POST",$"/api/thay-doi/{Id(change)}/duyet",new ChangeDecision(true,"Đồng ý"),403,cashierToken);
    var approved=await Call("POST",$"/api/thay-doi/{Id(change)}/duyet",new ChangeDecision(true,"Đồng ý"),token:adminToken);
    if(approved["SoTienHoanDuKien"]!.GetValue<decimal>()!=100000)throw new Exception("Refund proposal wrong");
    await Call("POST","/api/giao-dich/hoan",new RefundRequest(Id(cash),60001,"HOAN02","refund-too-much",Id(change)),409,cashierToken);
    await Call("POST","/api/giao-dich/hoan",new RefundRequest(Id(cash),60000,"HOAN02","refund-ok",Id(change)),token:cashierToken);
    var second=await Call("POST","/api/dang-ky",request with{IdempotencyKey="enroll-again-01"},token:memberToken);var secondId=Id(second);
    detail=await Call("GET","/api/dang-ky/"+secondId,token:memberToken);var charge2=Id(detail["khoanThu"]![0]!);
    await Call("POST","/api/giao-dich/thu",new ReceiptRequest(secondId,charge2,100000,"TIEN_MAT","CT04",null,"receive-next-01"),token:cashierToken);
    var transfer=await Call("POST","/api/thay-doi",new ChangeRequest(secondId,"CHUYEN","NGUOI_HOC","Đổi lớp","transfer-key-01",classB),token:memberToken);
    var transferApproved=await Call("POST",$"/api/thay-doi/{Id(transfer)}/duyet",new ChangeDecision(true,"Đồng ý chuyển"),token:adminToken);
    var finalized=await Call("POST",$"/api/thay-doi/{Id(transfer)}/hoan-tat-chuyen",token:adminToken);
    if(finalized["GiaTriChuyenThucTe"]!.GetValue<decimal>()!=100000)throw new Exception("Transfer credit wrong");
    var destination=transferApproved["DangKyDichId"]!.GetValue<long>();
    debt=await Call("GET",$"/api/dang-ky/{destination}/cong-no",token:memberToken);
    if(debt["khoanThu"]![0]!["conThieu"]!.GetValue<decimal>()!=0)throw new Exception("Transfer debt wrong");
    var sessionB=Id(listB[0]!);var sessionA=Id(listA[0]!);
    long attendanceId;
    await using(var db=new CheckDbContext(options))
    {
        var a=await db.DiemDanh.SingleAsync(x=>x.DangKyId==destination&&x.BuoiHocId==sessionB);attendanceId=a.Id;
    }
    await Call("POST",$"/api/hoc-tap/diem-danh/{attendanceId}/bao-nghi",new AbsenceRequest("Xin nghỉ"),token:memberToken);
    await using(var db=new CheckDbContext(options))
    {
        var b=await db.BuoiHoc.SingleAsync(x=>x.Id==sessionB);var now=BusinessClock.Now;b.BatDau=now.AddHours(-2);b.KetThuc=now;
        var a=await db.DiemDanh.SingleAsync(x=>x.Id==attendanceId);a.BaoNghiLuc=b.BatDau.AddHours(-13);await db.SaveChangesAsync();
    }
    var roster=await Call("GET",$"/api/hoc-tap/buoi/{sessionB}/danh-sach",token:adminToken);
    await Call("PUT",$"/api/hoc-tap/diem-danh/{attendanceId}",new AttendRequest("VANG",0,null,"Vắng",null,Version(roster[0]!)),token:adminToken);
    await Call("POST",$"/api/hoc-tap/buoi/{sessionB}/hoan-thanh",expected:204,token:adminToken);
    await Call("POST","/api/hoc-tap/hoc-bu",new MakeupRequest(attendanceId,sessionA),token:adminToken);
    await Call("POST","/api/hoc-tap/hoc-bu",new MakeupRequest(attendanceId,sessionA),409,adminToken);
    var profileId=second["HoSoTheoHocId"]!.GetValue<long>();
    await Call("POST","/api/hoc-tap/ket-qua",new ResultRequest(profileId,10,"Đạt","https://example.test/bai"),400,adminToken);
    var feedback=await Call("POST","/api/phan-hoi",new FeedbackRequest(destination,"Cần hỗ trợ"),token:memberToken);
    await Call("POST",$"/api/phan-hoi/{Id(feedback)}/tra-loi",new FeedbackReply("Đã tiếp nhận"),token:adminToken);
    async Task<JsonNode> Slip(string code,string type,long material,decimal qty,long? reference=null)=>await Call("POST","/api/kho/phieu",
        new InventoryRequest(code,type,[new InventoryLine(material,qty,1000,"L1",null,reference)],NhaCungCapId:supplierId),token:adminToken);
    async Task<JsonNode> PostSlip(JsonNode s,int status=200)=>await Call("POST",$"/api/kho/phieu/{Id(s)}/duyet",new InventoryApproval(Version(s)),status,adminToken);
    await PostSlip(await Slip("NHAP01","NHAP",materialId,5));
    await PostSlip(await Slip("XUAT01","XUAT_TIEU_HAO",materialId,6),409);
    await PostSlip(await Slip("XUAT02","XUAT_TIEU_HAO",materialId,3));
    await PostSlip(await Slip("NHAP02","NHAP",toolId,5));
    var lend=await PostSlip(await Slip("CAP01","CAP_DUNG_CHUNG",toolId,3));
    var lendDetail=await Call("GET",$"/api/kho/phieu/{Id(lend)}",token:adminToken);var originalLine=Id(lendDetail["chiTiet"]![0]!);
    await PostSlip(await Slip("TRA01","TRA_DUNG_CHUNG",toolId,4,originalLine),409);
    await PostSlip(await Slip("TRA02","TRA_DUNG_CHUNG",toolId,1,originalLine));
    await PostSlip(await Slip("HONG01","HONG_MAT",toolId,1,originalLine));
    var stock=await Call("GET","/api/kho/ton",token:adminToken);
    var toolStock=stock.AsArray().Single(x=>x!["id"]!.GetValue<long>()==toolId)!;
    if(toolStock["taiKho"]!.GetValue<decimal>()!=3||toolStock["dangMuon"]!.GetValue<decimal>()!=1)throw new Exception("Borrow/return/loss stock wrong");
    await Call("GET","/api/bao-cao/tong-quan",token:adminToken);
    await Call("GET","/api/bao-cao/nhat-ky",expected:403,token:memberToken);
    await Call("POST","/api/tai-khoan/dang-xuat",expected:204,token:memberToken);
    await Call("GET","/api/tai-khoan/toi",expected:401,token:memberToken);
    var fresh=await Login("member");
    await Call("POST","/api/tai-khoan/doi-mat-khau",new PasswordRequest("MyThuatTest2026!","MyThuatChanged2026!"),204,fresh);
    await Call("GET","/api/tai-khoan/toi",expected:401,token:fresh);
    await using(var db=new CheckDbContext(options))
    {
        if(await db.TaiKhoan.AnyAsync(x=>x.PasswordHash=="MyThuatTest2026!"))throw new Exception("Plain text password stored");
        if(await db.GiaoDichTien.CountAsync(x=>x.IdempotencyKey==RequestKeys.Key(4,"receive-0001"))>1)throw new Exception("Duplicate receipt");
        if(await db.NhatKyThayDoi.AnyAsync(x=>x.SauJson!=null&&x.SauJson.Contains("MyThuatTest")))throw new Exception("Password leaked to audit");
    }
    Console.WriteLine($"ALL {count} HTTP CASES PASSED ({(sqlMode?"SQL Server LocalDB":"SQLite")} test store).");
    await File.WriteAllTextAsync("Checks.result.txt",$"PASS: {count} HTTP cases. Provider: {(sqlMode?"SQL Server LocalDB":"SQLite")}. Concurrent SQL Server races require the separate stress cases.");
}
catch(Exception ex){ Console.Error.WriteLine(ex); Environment.ExitCode=1; }
finally
{
    await app.StopAsync();await app.DisposeAsync();client.Dispose();
    if(sqlMode && storeCreated)
    {
        // This name is generated above for the disposable test store, never MyThuatDb.
        if(sqlDatabase.StartsWith("MyThuatChecks_",StringComparison.Ordinal)&&sqlDatabase.Length==46)
        {
            await using var disposable=new CheckDbContext(options);
            await disposable.Database.EnsureDeletedAsync();
        }
    }
    else Microsoft.Data.Sqlite.SqliteConnection.ClearAllPools();
    // Only the explicitly generated test directory is removed.
    if(folder.StartsWith(Path.Combine(Path.GetTempPath(),"MyThuatChecks-"),StringComparison.OrdinalIgnoreCase))Directory.Delete(folder,true);
}
