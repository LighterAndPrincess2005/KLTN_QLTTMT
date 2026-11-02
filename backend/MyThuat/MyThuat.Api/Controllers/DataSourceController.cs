using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MyThuat.Api.Data;

namespace MyThuat.Api.Controllers;

[ApiController, Authorize(Roles="QUAN_TRI"), Route("api/he-thong/nguon-du-lieu")]
public sealed class DataSourceController(MyThuatDbContext db) : ControllerBase
{
    [HttpGet]
    public async Task<object> Get(CancellationToken ct)
    {
        var source = db.Database.GetDbConnection();
        return new
        {
            MaySql = source.DataSource, TenDatabase = source.Database,
            ThanhVien = await db.ThanhVien.CountAsync(ct), HocVien = await db.HocVien.CountAsync(ct),
            GiaoVien = await db.GiaoVien.CountAsync(ct), KhoaHoc = await db.KhoaHoc.CountAsync(ct),
            LopHoc = await db.LopHoc.CountAsync(ct), BuoiHoc = await db.BuoiHoc.CountAsync(ct),
            DangKy = await db.DangKy.CountAsync(ct), TaiKhoan = await db.TaiKhoan.CountAsync(ct)
        };
    }
}
