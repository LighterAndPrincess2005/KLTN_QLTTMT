using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.OpenApi.Models;
using MyThuat.Api.Controllers;
using MyThuat.Api.Models;
using Swashbuckle.AspNetCore.SwaggerGen;
namespace MyThuat.Api.Documentation;
public sealed class ManagementOperationFilter:IOperationFilter
{
    public void Apply(OpenApiOperation operation,OperationFilterContext context)
    {
        if(context.ApiDescription.ActionDescriptor.EndpointMetadata.OfType<IAllowAnonymous>().Any())operation.Security=[];
        if(context.ApiDescription.ActionDescriptor is not ControllerActionDescriptor action)return;
        var parent=action.ControllerTypeInfo.BaseType;
        if(parent is null||!parent.IsGenericType||parent.GetGenericTypeDefinition()!=typeof(ManagementControllerBase<>))return;
        if(context.ApiDescription.HttpMethod is not ("POST" or "PUT"))return;
        var type=parent.GetGenericArguments()[0];
        var properties=type.GetProperties().Where(p=>p.PropertyType.IsValueType||p.PropertyType==typeof(string)||p.PropertyType==typeof(byte[]))
            .Where(p=>p.Name is not ("Id" or "CreatedAt" or "UpdatedAt" or "RowVersion" or "GiaoVienDanhGia"))
            .Where(p=>!(p.Name.StartsWith("Nguoi")&&(p.PropertyType==typeof(long)||p.PropertyType==typeof(long?)))).ToArray();
        var schema=new OpenApiSchema {Type="object",AdditionalPropertiesAllowed=false,Properties=new Dictionary<string,OpenApiSchema>()};
        foreach(var p in properties)
        {
            var t=Nullable.GetUnderlyingType(p.PropertyType)??p.PropertyType;
            var nullable=Nullable.GetUnderlyingType(p.PropertyType)!=null||t==typeof(string);
            schema.Properties[p.Name]=new OpenApiSchema {
                Type=t==typeof(string)||t==typeof(DateOnly)||t==typeof(DateTime)?"string":t==typeof(bool)?"boolean":t==typeof(decimal)?"number":"integer",
                Format=t==typeof(long)?"int64":t==typeof(DateOnly)?"date":t==typeof(DateTime)?"date-time":null,Nullable=nullable,
                Description=p.Name=="TrangThai"?"Khóa/lớp/quy định mới dùng NHAP; hồ sơ dùng HOAT_DONG hoặc CHO_XAC_NHAN.":p.Name.EndsWith("Id")?"Mã tham chiếu hồ sơ đã tồn tại.":null};
        }
        var name="Nhap"+type.Name;
        context.SchemaRepository.Schemas.TryAdd(name,schema);
        operation.RequestBody=new OpenApiRequestBody {Required=true,Content=new Dictionary<string,OpenApiMediaType>{
            ["application/json"]=new(){Schema=new OpenApiSchema{Reference=new OpenApiReference{Type=ReferenceType.Schema,Id=name}}}}};
        if(context.ApiDescription.HttpMethod=="PUT")
            operation.Parameters.Add(new OpenApiParameter {Name="If-Match",In=ParameterLocation.Header,Required=true,
                Description="RowVersion Base64 lấy từ GET quản lý. Có thể gửi kèm dấu ngoặc kép.",Schema=new(){Type="string"}});
        operation.Description="Chỉ nhận các thuộc tính được liệt kê. Không xóa lịch sử; ngừng hồ sơ qua TrangThai. Người thao tác và thời điểm do server ghi.";
    }
}
