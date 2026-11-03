using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MyThuat.Api.Contracts;
using MyThuat.Api.Data;
using MyThuat.Api.Models;
using MyThuat.Api.Services;
namespace MyThuat.Api.Controllers;
[ApiController,Authorize(Policy="ThuTien"),Route("api/giao-dich")]
public sealed class GiaoDichController(FinanceService service,MyThuatDbContext db):ControllerBase
{
    [HttpPost("thu")]public Task<object> Receive(ReceiptRequest r,CancellationToken ct)=>service.Receive(r,ct);
    [HttpPost("hoan")]public Task<object> Refund(RefundRequest r,CancellationToken ct)=>service.Refund(r,ct);
    [HttpGet("phieu-thu")]
    public async Task<object> Receipts(CancellationToken ct) => await ReceiptRows(ConfirmedReceipts()
        .OrderByDescending(x=>x.ThoiDiem).ThenByDescending(x=>x.Id).Take(500)).ToListAsync(ct);

    [HttpGet("phieu-thu/{id:long:min(1)}")]
    public async Task<object> Receipt(long id,CancellationToken ct) =>
        await ReceiptRows(ConfirmedReceipts().Where(x=>x.Id==id)).SingleOrDefaultAsync(ct)
            ??throw new ApiError(404,"Không có phiếu thu đã xác nhận.");

    private IQueryable<GiaoDichTien> ConfirmedReceipts() =>
        db.GiaoDichTien.AsNoTracking()
            .Where(x=>x.LoaiGiaoDich=="THU"&&x.TrangThai=="DA_XAC_NHAN"&&x.KhoanThuId!=null);
    private static IQueryable<ReceiptRow> ReceiptRows(IQueryable<GiaoDichTien> query) =>
        query.Select(x=>new ReceiptRow(x.Id,x.MaChungTu,x.ThoiDiem,x.SoTien,x.PhuongThuc,x.MaThamChieu,
            x.DangKy!.MaDangKy,x.DangKy.HoSoTheoHoc!.HocVien!.HoTen,x.DangKy.LopHoc!.TenLop,x.KhoanThu!.MoTaChot));

    private sealed record ReceiptRow(long Id,string? MaChungTu,DateTime ThoiDiem,decimal SoTien,string PhuongThuc,
        string? MaThamChieu,string MaDangKy,string TenHocVien,string TenLop,string NoiDungThu);
    [HttpGet("cho-doi-chieu")]public async Task<object> Pending(CancellationToken ct)=>
        (await db.GiaoDichTien.AsNoTracking().Where(x=>x.TrangThai=="CHO_DOI_CHIEU").OrderBy(x=>x.ThoiDiem).Take(500).ToListAsync(ct)).Select(ManagementService.Scalars);
}
