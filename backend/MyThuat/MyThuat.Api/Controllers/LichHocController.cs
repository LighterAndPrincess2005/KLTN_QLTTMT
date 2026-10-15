using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MyThuat.Api.Contracts;
using MyThuat.Api.Data;
using MyThuat.Api.Security;
using MyThuat.Api.Services;
namespace MyThuat.Api.Controllers;
[ApiController,Authorize,Route("api/lich-hoc")]
public sealed class LichHocController(MyThuatDbContext db,ScheduleService service):ControllerBase
{
    [HttpGet]
    public async Task<object> List(CancellationToken ct)
    {
        var query=db.BuoiHoc.AsNoTracking().Where(x=>x.TrangThai!="DA_HUY");
        if(!User.Has(Permission.DaoTao))
        {
            if(User.TeacherId() is long teacher)query=query.Where(x=>db.GiangDayBuoi.Any(y=>y.BuoiHocId==x.Id&&y.GiaoVienId==teacher));
            else if(User.MemberId() is long member)
                query=query.Where(x=>db.DiemDanh.Any(y=>y.BuoiHocId==x.Id&&y.TrangThai!="HUY_BO"
                    &&db.DaiDienHocVien.Any(d=>d.HocVienId==y.DangKy!.HoSoTheoHoc!.HocVienId&&d.ThanhVienId==member
                        &&d.XacNhanLuc!=null&&d.HieuLucTu<=BusinessClock.Now&&(d.HieuLucDen==null||d.HieuLucDen>BusinessClock.Now)
                        &&(d.QuyenDaiDien=="DAY_DU"||d.QuyenDaiDien=="XEM_KET_QUA"))));
            else throw new ApiError(403,"Không có quyền xem lịch.");
        }
        var rows=await query.OrderBy(x=>x.BatDau).Take(500).ToListAsync(ct);return rows.Select(ManagementService.Scalars);
    }
    [Authorize(Policy="DaoTao"),HttpPost("lop/{id:long:min(1)}/sinh-lich")]
    public Task<object> Generate(long id,GenerateScheduleRequest r,CancellationToken ct)=>service.Generate(id,r,ct);
    [Authorize(Policy="DaoTao"),HttpPost("lop/{id:long:min(1)}/phan-cong")]
    public Task<object> Assign(long id,AssignTeacherRequest r,CancellationToken ct)=>service.Assign(id,r,ct);
    [Authorize(Policy="DaoTao"),HttpPost("buoi/{id:long:min(1)}/thay-giao-vien")]
    public Task<object> Substitute(long id,SubstituteRequest r,CancellationToken ct)=>service.Substitute(id,r,ct);
    [Authorize(Policy="DaoTao"),HttpPost("buoi/{id:long:min(1)}/doi-lich")]
    public Task<object> Move(long id,RescheduleRequest r,CancellationToken ct)=>service.Move(id,r,ct);
}
