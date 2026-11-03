using System.Data;
using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using MyThuat.Api.Data;
using MyThuat.Api.Models;
namespace MyThuat.Api.Services;
public sealed class ManagementService(MyThuatDbContext db, AuditService audit, AccessService access,
    ManagementValidation validator, EnrollmentService enrollments, ClassInventoryService classInventory)
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
        var item=id is long key ? typeof(T)==typeof(LopHoc) ? (T)(EntityBase)await enrollments.LockClass(key,ct)
            : await db.Set<T>().SingleOrDefaultAsync(x=>x.Id==key,ct)
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
            if(typeof(T)==typeof(KhoaHoc)&&field.Name.Equals("TienToMa",StringComparison.OrdinalIgnoreCase))
            {
                ApiError.Require(!id.HasValue,"Không đổi tiền tố của khóa đã cấp mã.");
                ApiError.Require(field.Value.ValueKind==JsonValueKind.String,"Tiền tố mã khóa cần là chuỗi.");
                continue;
            }
            var prop=model.GetProperties().SingleOrDefault(p=>p.Name.Equals(field.Name,StringComparison.OrdinalIgnoreCase));
            ApiError.Require(prop is not null && prop.Name is not ("Id" or "CreatedAt" or "UpdatedAt" or "RowVersion"),"Thuộc tính không được phép: "+field.Name);
            ApiError.Require(!(prop!.Name.StartsWith("Nguoi")&&(prop.ClrType==typeof(long)||prop.ClrType==typeof(long?))) && prop.Name!="GiaoVienDanhGia","Người xử lý được xác định từ tài khoản đăng nhập.");
            object? value;
            try {value=field.Value.Deserialize(prop.ClrType,JsonOptions);}
            catch(JsonException){throw new ApiError(400,"Kiểu dữ liệu không đúng: "+field.Name);}
            prop.PropertyInfo!.SetValue(item,value);
        }
        if(id.HasValue)
        {
            // Public codes stay stable so records and history keep their identity.
            var code = item switch
            {
                LoaiKhoaHoc x => ("MaLoai", x.MaLoai), KhoaHoc x => ("MaKhoa", x.MaKhoa),
                LopHoc x => ("MaLop", x.MaLop), GiaoVien x => ("MaGiaoVien", x.MaGiaoVien),
                PhongHoc x => ("MaPhong", x.MaPhong), ThanhVien x => ("MaThanhVien", x.MaThanhVien),
                HocVien x => ("MaHocVien", x.MaHocVien), HoaCu x => ("MaHoaCu", x.MaHoaCu),
                NhaCungCap x => ("MaNCC", x.MaNCC), _ => ("", "")
            };
            if(code.Item1.Length>0)ApiError.Require(Equals(before[code.Item1],code.Item2),"Mã đã cấp không được sửa.");
        }
        else
        {
            // The serializable transaction protects the sequence during concurrent creates.
            switch(item)
            {
                case LoaiKhoaHoc x: x.MaLoai=await CodeGenerator.NextAsync(db,db.LoaiKhoaHoc,c=>c.MaLoai,"LK-",ct);break;
                case KhoaHoc x:
                    var prefix=body.EnumerateObject().FirstOrDefault(p=>p.Name.Equals("TienToMa",StringComparison.OrdinalIgnoreCase));
                    x.MaKhoa=x.KhoaGocId is long root
                        ? (await db.KhoaHoc.AsNoTracking().SingleAsync(c=>c.Id==root,ct)).MaKhoa
                        : await CodeGenerator.NextAsync(db,db.KhoaHoc,c=>c.MaKhoa,CodeGenerator.CoursePrefix(prefix.Value.ValueKind==JsonValueKind.Undefined?"KHOA":prefix.Value.GetString()??"")+"-",ct);
                    x.HieuLucTu=BusinessClock.Today;break;
                case LopHoc x:
                    var course=await db.KhoaHoc.AsNoTracking().SingleOrDefaultAsync(c=>c.Id==x.KhoaHocId,ct)??throw new ApiError(400,"Khóa không tồn tại.");
                    x.MaLop=await CodeGenerator.NextAsync(db,db.LopHoc,c=>c.MaLop,CodeGenerator.ClassPrefix(course),ct);break;
                case GiaoVien x: x.MaGiaoVien=await CodeGenerator.NextAsync(db,db.GiaoVien,c=>c.MaGiaoVien,"GV-",ct);break;
                case PhongHoc x: x.MaPhong=await CodeGenerator.NextAsync(db,db.PhongHoc,c=>c.MaPhong,"PH-",ct);break;
                case ThanhVien x: x.MaThanhVien=await CodeGenerator.NextAsync(db,db.ThanhVien,c=>c.MaThanhVien,"TV-",ct);break;
                case HocVien x: x.MaHocVien=await CodeGenerator.NextAsync(db,db.HocVien,c=>c.MaHocVien,"HV-",ct);break;
                case HoaCu x: x.MaHoaCu=await CodeGenerator.NextAsync(db,db.HoaCu,c=>c.MaHoaCu,"HC-",ct);break;
                case NhaCungCap x: x.MaNCC=await CodeGenerator.NextAsync(db,db.NhaCungCap,c=>c.MaNCC,"NCC-",ct);break;
            }
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
        if (item is LopHoc cls && id.HasValue && !Equals(before["TrangThai"], cls.TrangThai))
        {
            if (cls.TrangThai == "DANG_TUYEN_SINH")
                await classInventory.Reserve(cls, access.User.SecurityId(), ct);
            if (cls.TrangThai == "DANG_HOC")
                await classInventory.Issue(cls, access.User.SecurityId(), ct);
        }
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
