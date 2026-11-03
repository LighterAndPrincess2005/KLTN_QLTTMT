using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MyThuat.Api.Contracts;
using MyThuat.Api.Data;
using MyThuat.Api.Services;
namespace MyThuat.Api.Controllers;
[ApiController,Authorize(Policy="Kho"),Route("api/kho")]
public sealed class KhoController(InventoryService service,MyThuatDbContext db,AuditService audit):ControllerBase
{
    [HttpPost("phieu")]public Task<object> Create(InventoryRequest r,CancellationToken ct)=>service.Create(r,ct);
    [HttpPost("phieu/{id:long:min(1)}/duyet")]public Task<object> Approve(long id,InventoryApproval r,CancellationToken ct)=>service.Approve(id,r,ct);
    [HttpGet("ton")]public Task<object> Stock(CancellationToken ct)=>service.Stock(ct);
    [HttpGet("phieu")]public async Task<object> List(CancellationToken ct)=>
        (await db.PhieuKho.AsNoTracking().Include(x=>x.LopHoc).OrderByDescending(x=>x.LapLuc).Take(500).ToListAsync(ct))
        .Select(x=>{var row=ManagementService.Scalars(x);row["TenLop"]=x.LopHoc?.TenLop;return row;});
    [HttpGet("phieu/{id:long:min(1)}")]
    public async Task<object> Detail(long id,CancellationToken ct)
    {
        var slip=await db.PhieuKho.AsNoTracking().SingleOrDefaultAsync(x=>x.Id==id,ct)??throw new ApiError(404,"Không có phiếu.");
        var lines=await db.ChiTietPhieuKho.AsNoTracking().Include(x=>x.HoaCu).Where(x=>x.PhieuKhoId==id).ToListAsync(ct);
        return new {Phieu=ManagementService.Scalars(slip),ChiTiet=lines.Select(x=>
        {
            var row=ManagementService.Scalars(x);row["TenHoaCu"]=x.HoaCu?.TenHoaCu;row["DonViCoSo"]=x.HoaCu?.DonViCoSo;
            return row;
        })};
    }
    [HttpPost("phieu/{id:long:min(1)}/huy-du-tru")]
    public async Task<IActionResult> Release(long id,InventoryApproval r,CancellationToken ct)
    {
        var slip=await db.PhieuKho.SingleOrDefaultAsync(x=>x.Id==id,ct)??throw new ApiError(404,"Không có phiếu.");
        ApiError.Require(slip.LopHocId is null,"Dự trữ tự động được giải phóng theo trạng thái và hạn tuyển sinh của lớp.",409);
        ApiError.Require(slip.LoaiPhieu=="DU_TRU"&&slip.TrangThai=="DA_DUYET"&&Convert.ToBase64String(slip.RowVersion)==r.RowVersion,"Dự trữ đã thay đổi hoặc không còn hiệu lực.",409);
        slip.TrangThai="DA_HUY";audit.Add("PhieuKho",id,"GIAI_PHONG_DU_TRU");await db.SaveChangesAsync(ct);return NoContent();
    }
}
