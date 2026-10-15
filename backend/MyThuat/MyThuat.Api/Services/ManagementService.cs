using System.Data;
using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using MyThuat.Api.Data;
using MyThuat.Api.Models;
namespace MyThuat.Api.Services;
public sealed class ManagementService(MyThuatDbContext db, AuditService audit, AccessService access, ManagementValidation validator)
{
    private static readonly JsonSerializerOptions JsonOptions=new() {PropertyNameCaseInsensitive=true};
    public static Dictionary<string,object?> Scalars<T>(T item) where T:EntityBase => typeof(T).GetProperties()
        .Where(p=>p.PropertyType.IsValueType||p.PropertyType==typeof(string)||p.PropertyType==typeof(byte[]))
        .ToDictionary(p=>p.Name,p=>p.GetValue(item));

    public async Task<object> List<T>(int trang,int kichThuoc,CancellationToken ct) where T:EntityBase
    {
        var query=db.Set<T>().AsNoTracking();
        var total=await query.CountAsync(ct);
        var rows=await query.OrderBy(x=>x.Id).Skip((trang-1)*kichThuoc).Take(kichThuoc).ToListAsync(ct);
        return new {Trang=trang,KichThuoc=kichThuoc,TongSo=total,DuLieu=rows.Select(Scalars)};
    }
    public async Task<object> Get<T>(long id,CancellationToken ct) where T:EntityBase => Scalars(
        await db.Set<T>().AsNoTracking().SingleOrDefaultAsync(x=>x.Id==id,ct)??throw new ApiError(404,"Không tìm thấy dữ liệu."));

    public async Task<object> Save<T>(long? id,JsonElement body,string? version,CancellationToken ct) where T:EntityBase,new()
    {
        await using var tx=await db.Database.BeginTransactionAsync(IsolationLevel.Serializable,ct);
        var item=id is long key ? await db.Set<T>().SingleOrDefaultAsync(x=>x.Id==key,ct)
            ??throw new ApiError(404,"Không tìm thấy dữ liệu.") : new T();
        var before=Scalars(item);
        if(id.HasValue)
        {
            ApiError.Require(!string.IsNullOrEmpty(version),"Cần gửi If-Match chứa RowVersion dạng Base64.",428);
            byte[] bytes;
            try {bytes=Convert.FromBase64String(version!.Trim('"'));}catch(FormatException){throw new ApiError(400,"If-Match không hợp lệ.");}
            ApiError.Require(item.RowVersion.SequenceEqual(bytes),"Dữ liệu đã thay đổi, hãy tải lại.",409);
            db.Entry(item).Property(x=>x.RowVersion).OriginalValue=bytes;
        }
        ApiError.Require(body.ValueKind==JsonValueKind.Object,"Dữ liệu cần là một object JSON.");
        var model=db.Model.FindEntityType(typeof(T))!;
        ApiError.Require(body.EnumerateObject().Select(x=>x.Name).Distinct(StringComparer.OrdinalIgnoreCase).Count()==body.EnumerateObject().Count(),"Không lặp tên thuộc tính.");
        foreach(var field in body.EnumerateObject())
        {
            var prop=model.GetProperties().SingleOrDefault(p=>p.Name.Equals(field.Name,StringComparison.OrdinalIgnoreCase));
            ApiError.Require(prop is not null && prop.Name is not ("Id" or "CreatedAt" or "UpdatedAt" or "RowVersion"),"Thuộc tính không được phép: "+field.Name);
            ApiError.Require(!(prop!.Name.StartsWith("Nguoi")&&(prop.ClrType==typeof(long)||prop.ClrType==typeof(long?))) && prop.Name!="GiaoVienDanhGia","Người xử lý được xác định từ tài khoản đăng nhập.");
            object? value;
            try {value=field.Value.Deserialize(prop.ClrType,JsonOptions);}
            catch(JsonException){throw new ApiError(400,"Kiểu dữ liệu không đúng: "+field.Name);}
            prop.PropertyInfo!.SetValue(item,value);
        }
        foreach(var prop in model.GetProperties())
        {
            var val=prop.PropertyInfo!.GetValue(item);
            if(prop.Name is "Id" or "CreatedAt" or "UpdatedAt" or "RowVersion")continue;
            if(prop.Name.StartsWith("Nguoi")&&!prop.IsNullable&&prop.ClrType==typeof(long))
                prop.PropertyInfo.SetValue(item,access.User.SecurityId());
            if(!prop.IsNullable&&prop.ClrType==typeof(string))ApiError.Require(!string.IsNullOrWhiteSpace(val as string),"Thiếu trường "+prop.Name);
            if(val is string text)
            {
                var sql=prop.GetColumnType()??"";
                var match=System.Text.RegularExpressions.Regex.Match(sql,@"\((\d+)\)");
                if(match.Success)ApiError.Require(text.Length<=int.Parse(match.Groups[1].Value),"Trường quá dài: "+prop.Name);
            }
            if(val is decimal amount)
            {
                var scale=System.Text.RegularExpressions.Regex.Match(prop.GetColumnType()??"",@"," + @"(\d+)\)");
                if(scale.Success)ApiError.Require(decimal.Round(amount,int.Parse(scale.Groups[1].Value))==amount,"Số chữ số thập phân không hợp lệ: "+prop.Name);
            }
            if(val is DateOnly date)ApiError.Require(date.Year>=1900,"Ngày không hợp lệ: "+prop.Name);
            if(val is DateTime dt)ApiError.Require(dt.Year>=1900&&dt.Kind!=DateTimeKind.Utc,"Ngày giờ cần dùng giờ Việt Nam, không thêm Z: "+prop.Name);
        }
        await validator.Check(item,id.HasValue,before,ct);
        if(!id.HasValue)db.Set<T>().Add(item);
        await db.SaveChangesAsync(ct);
        audit.Add(typeof(T).Name,item.Id,id.HasValue?"CAP_NHAT":"TAO",new {Truong=body.EnumerateObject().Select(x=>x.Name).ToArray()});
        await db.SaveChangesAsync(ct);await tx.CommitAsync(ct);
        return Scalars(item);
    }
}
internal static class PrincipalHelpers
{
    public static long SecurityId(this System.Security.Claims.ClaimsPrincipal user)=>MyThuat.Api.Security.Permissions.AccountId(user);
}
