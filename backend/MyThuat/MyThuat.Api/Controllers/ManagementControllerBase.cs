using System.ComponentModel.DataAnnotations;
using System.Text.Json;
using Microsoft.AspNetCore.Mvc;
using MyThuat.Api.Models;
using MyThuat.Api.Services;
namespace MyThuat.Api.Controllers;
public abstract class ManagementControllerBase<T>(ManagementService service):ControllerBase where T:EntityBase,new()
{
    [HttpGet] public Task<object> List([Range(1,1000000)]int trang=1,[Range(1,100)]int kichThuoc=20,CancellationToken ct=default)
        =>service.List<T>(trang,kichThuoc,ct);
    [HttpGet("{id:long:min(1)}")] public Task<object> Get(long id,CancellationToken ct)=>service.Get<T>(id,ct);
    [HttpPost] public Task<object> Create([FromBody]JsonElement body,CancellationToken ct)=>service.Save<T>(null,body,null,ct);
    [HttpPut("{id:long:min(1)}")] public Task<object> Update(long id,[FromBody]JsonElement body,CancellationToken ct)
        =>service.Save<T>(id,body,Request.Headers.IfMatch.ToString(),ct);
}
