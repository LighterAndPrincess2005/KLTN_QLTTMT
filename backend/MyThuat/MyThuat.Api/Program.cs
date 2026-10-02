using System.Net;
using System.Threading.RateLimiting;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Models;
using MyThuat.Api.Data;
using MyThuat.Api.Models;
using MyThuat.Api.Security;
using MyThuat.Api.Services;

var builder=WebApplication.CreateBuilder(args);
builder.Configuration.AddJsonFile("appsettings.Local.json",optional:true,reloadOnChange:true);
builder.Services.AddControllers().AddJsonOptions(o=>o.JsonSerializerOptions.MaxDepth=32);
builder.Services.AddProblemDetails();
builder.Services.AddHttpContextAccessor();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(o=>
{
    o.OperationFilter<MyThuat.Api.Documentation.ManagementOperationFilter>();
    o.SwaggerDoc("v1",new OpenApiInfo {Title="Trung tâm Mỹ thuật API",Version="v1"});
    o.AddSecurityDefinition("Bearer",new OpenApiSecurityScheme {Type=SecuritySchemeType.Http,Scheme="bearer",
        Description="Đăng nhập bằng /api/tai-khoan/dang-nhap, dán AccessToken vào đây."});
    o.AddSecurityRequirement(new OpenApiSecurityRequirement {
        [new OpenApiSecurityScheme {Reference=new OpenApiReference {Type=ReferenceType.SecurityScheme,Id="Bearer"}}]=Array.Empty<string>()});
});
builder.Services.AddDbContext<MyThuatDbContext>(o=>o.UseSqlServer(builder.Configuration.GetConnectionString("MyThuat")
    ??throw new InvalidOperationException("Thiếu chuỗi kết nối MyThuat.")));
var local=Path.Combine(builder.Environment.ContentRootPath,".local");Directory.CreateDirectory(local);
var protection=builder.Services.AddDataProtection().SetApplicationName("MyThuat.Api")
    .PersistKeysToFileSystem(new DirectoryInfo(Path.Combine(local,"keys")));
if(OperatingSystem.IsWindows())protection.ProtectKeysWithDpapi();
builder.Services.AddSingleton<AccessTokens>();
builder.Services.AddScoped<IPasswordHasher<TaiKhoan>,PasswordHasher<TaiKhoan>>();
builder.Services.AddAuthentication("Bearer").AddScheme<AuthenticationSchemeOptions,BearerAuthenticationHandler>("Bearer",_=>{});
builder.Services.AddAuthorization(o=>
{
    o.FallbackPolicy=new AuthorizationPolicyBuilder().RequireAuthenticatedUser().Build();
    foreach(var permission in Enum.GetValues<Permission>().Where(x=>x!=Permission.None))
        o.AddPolicy(permission.ToString(),p=>p.RequireAuthenticatedUser().RequireAssertion(c=>
            permission is Permission.TaiKhoan or Permission.NhatKy ? c.User.IsInRole("QUAN_TRI") : c.User.Has(permission)));
});
builder.Services.AddRateLimiter(o=>
{
    o.RejectionStatusCode=429;
    o.AddPolicy("login",ctx=>RateLimitPartition.GetFixedWindowLimiter(ctx.Connection.RemoteIpAddress?.ToString()??"unknown",
        _=>new FixedWindowRateLimiterOptions {PermitLimit=10,Window=TimeSpan.FromMinutes(1),QueueLimit=0}));
});
var origins=builder.Configuration.GetSection("WebOrigins").Get<string[]>()??[];
builder.Services.AddCors(o=>o.AddPolicy("Web",p=>{
    if(origins.Length>0)p.WithOrigins(origins).AllowAnyHeader().AllowAnyMethod();
}));
builder.Services.AddScoped<DanhMucService>();
builder.Services.AddScoped<AuditService>();
builder.Services.AddScoped<AccessService>();
builder.Services.AddScoped<PolicyService>();
builder.Services.AddScoped<ManagementService>();
builder.Services.AddScoped<ManagementValidation>();
builder.Services.AddScoped<ScheduleService>();
builder.Services.AddScoped<EnrollmentService>();
builder.Services.AddScoped<FinanceService>();
builder.Services.AddScoped<LearningService>();
builder.Services.AddScoped<ChangeService>();
builder.Services.AddScoped<InventoryService>();
builder.Services.AddScoped<ClassLifecycleService>();
builder.Services.AddHostedService<ExpiryWorker>();


var app=builder.Build();
if(args.Contains("--seed-demo"))
{
    using var scope=app.Services.CreateScope();
    await DemoData.Seed(scope.ServiceProvider.GetRequiredService<MyThuatDbContext>(),app.Environment.ContentRootPath);return;
}
if(args.Contains("--export-schema"))
{
    using var scope=app.Services.CreateScope();var db=scope.ServiceProvider.GetRequiredService<MyThuatDbContext>();
    Directory.CreateDirectory("Database");await File.WriteAllTextAsync(Path.Combine(app.Environment.ContentRootPath,"Database/002_SchemaHoanThien.sql"),db.Database.GenerateCreateScript());
    Console.WriteLine("Đã xuất schema. Chưa thay đổi database.");return;
}
if(args.Contains("--initialize"))
{
    using var scope=app.Services.CreateScope();var db=scope.ServiceProvider.GetRequiredService<MyThuatDbContext>();
    await db.Database.EnsureCreatedAsync();
    // Existing databases are never deleted or overwritten. Verify the required model columns.
    _=await db.TaiKhoan.AsNoTracking().Select(x=>new {x.Id,x.RowVersion,x.CreatedAt,x.UpdatedAt}).Take(1).ToListAsync();
    _=await db.YeuCauThayDoi.AsNoTracking().Select(x=>new {x.RequestFingerprint,x.PhuongAnHoanJson}).Take(1).ToListAsync();
    Console.WriteLine("Database sẵn sàng. Tạo tài khoản quản trị lần đầu trên trang chủ API.");return;
}
app.UseExceptionHandler(handler=>handler.Run(async ctx=>
{
    var error=ctx.Features.Get<IExceptionHandlerFeature>()!.Error;
    var status=error is ApiError known?known.Status : error is DbUpdateConcurrencyException or DbUpdateException?409:500;
    var title=error is ApiError apiError?apiError.Message:status==409?"Dữ liệu đã thay đổi hoặc vi phạm ràng buộc. Tải lại và kiểm tra thông tin.":"Không xử lý được yêu cầu. Kiểm tra kết nối database và nhật ký máy chủ.";
    ctx.Response.StatusCode=status;
    await Results.Problem(statusCode:status,title:title,extensions:new Dictionary<string,object?>{{"traceId",ctx.TraceIdentifier}}).ExecuteAsync(ctx);
}));
app.UseCors("Web");
app.UseRateLimiter();
if(app.Environment.IsDevelopment())
{
    app.UseSwagger();app.UseSwaggerUI(o=>o.EnablePersistAuthorization());
}
app.UseAuthentication();
app.UseAuthorization();
if(app.Environment.IsDevelopment())
{
    app.MapGet("/",()=>Results.Content(File.ReadAllText(Path.Combine(app.Environment.ContentRootPath,"LocalUi/index.html")),"text/html; charset=utf-8"))
        .AllowAnonymous().ExcludeFromDescription();
}
else app.UseHttpsRedirection();
app.MapGet("/health",()=>Results.Ok(new {status="ok",service="MyThuat.Api"})).AllowAnonymous().WithTags("Hệ thống");
var shutdownToken=Convert.ToHexString(System.Security.Cryptography.RandomNumberGenerator.GetBytes(32));
await File.WriteAllTextAsync(Path.Combine(local,"stop-token"),shutdownToken);
app.MapPost("/local/stop",(HttpContext ctx,IHostApplicationLifetime lifetime)=>
{
    if(ctx.Connection.RemoteIpAddress is not { } ip || !IPAddress.IsLoopback(ip)
        || !System.Security.Cryptography.CryptographicOperations.FixedTimeEquals(
            System.Text.Encoding.UTF8.GetBytes(ctx.Request.Headers["X-Local-Token"].ToString()),
            System.Text.Encoding.UTF8.GetBytes(shutdownToken)))return Results.Unauthorized();
    lifetime.StopApplication();return Results.Ok();
}).AllowAnonymous().ExcludeFromDescription();
app.MapControllers();app.Run();
