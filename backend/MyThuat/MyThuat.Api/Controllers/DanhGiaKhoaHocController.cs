using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MyThuat.Api.Data;
using MyThuat.Api.Models;
using MyThuat.Api.Security;
using MyThuat.Api.Services;

namespace MyThuat.Api.Controllers;

[ApiController, Authorize, Route("api/danh-gia-khoa")]
public sealed class DanhGiaKhoaHocController(MyThuatDbContext db, AccessService access, AuditService audit) : ControllerBase
{
    const string Admin = "QUAN_TRI";
    IQueryable<DanhGiaKhoaHoc> Reviews => db.Set<DanhGiaKhoaHoc>();
    static string Photo(long id) => $"/api/danh-gia-khoa/{id}/anh";

    [HttpGet("cua-toi/{profileId:long}")]
    public async Task<object> Mine(long profileId, CancellationToken ct)
    {
        var member = await CheckProfile(profileId, ct);
        var review = await Reviews.Where(x => x.HoSoTheoHocId == profileId)
            .Select(x => new { x.TrangThai, x.GhiChuDuyet, CoTheHienTen = x.NguoiGuiId == member && x.DongYCongKhai, x.TenNguoiDangCongKhai }).SingleOrDefaultAsync(ct);
        return new { review };
    }

    [HttpPost, RequestSizeLimit(6 * 1024 * 1024)]
    public async Task<object> Submit([FromForm] long profileId, [FromForm] int soSao, [FromForm] string noiDung,
        [FromForm] bool dongYCongKhai, [FromForm] bool hienTen, IFormFile? anh, CancellationToken ct)
    {
        var member = await CheckProfile(profileId, ct);
        ApiError.Require(soSao is >= 1 and <= 5 && noiDung.Trim().Length is > 0 and <= 2000, "Chọn số sao và nhập tối đa 2.000 ký tự.");
        var row = new DanhGiaKhoaHoc { HoSoTheoHocId = profileId, NguoiGuiId = member, SoSao = soSao,
            NoiDung = noiDung.Trim(), DongYCongKhai = dongYCongKhai, TrangThai = dongYCongKhai ? "CHO_DUYET" : "RIENG_TU" };
        if (dongYCongKhai && hienTen)
            row.TenNguoiDangCongKhai = await db.ThanhVien.Where(x => x.Id == member).Select(x => x.HoTen).SingleAsync(ct);
        if (anh is not null)
        {
            ApiError.Require(anh.Length is > 0 and <= 5 * 1024 * 1024, "Ảnh tối đa 5 MB.");
            using var stream = new MemoryStream();
            await anh.CopyToAsync(stream, ct);
            row.Anh = stream.ToArray();
            row.LoaiAnh = TepCongKhaiController.ImageExtension(row.Anh);
            ApiError.Require(row.LoaiAnh is "png" or "jpg", "Chỉ nhận ảnh PNG hoặc JPEG.");
        }

        ApiError.Require(!await Reviews.AnyAsync(x => x.HoSoTheoHocId == profileId, ct), "Hồ sơ này đã gửi đánh giá khóa học.", 409);
        db.Add(row);
        try { await db.SaveChangesAsync(ct); }
        catch (DbUpdateException ex) when (ex.InnerException is Microsoft.Data.SqlClient.SqlException { Number: 2601 or 2627 })
        { throw new ApiError(409, "Hồ sơ này đã gửi đánh giá khóa học."); }
        return new { row.TrangThai, CoTheHienTen = row.DongYCongKhai, row.TenNguoiDangCongKhai };
    }

    public sealed record NameConsent(bool HienTen);

    [HttpPut("cua-toi/{profileId:long}/ten")]
    public async Task<object> SetPublicName(long profileId, NameConsent consent, CancellationToken ct)
    {
        var member = await CheckProfile(profileId, ct);
        var row = await Reviews.SingleOrDefaultAsync(x => x.HoSoTheoHocId == profileId && x.NguoiGuiId == member && x.DongYCongKhai, ct)
            ?? throw new ApiError(404, "Không tìm thấy đánh giá công khai của bạn.");
        row.TenNguoiDangCongKhai = consent.HienTen
            ? await db.ThanhVien.Where(x => x.Id == member).Select(x => x.HoTen).SingleAsync(ct)
            : null;
        await db.SaveChangesAsync(ct);
        return new { row.TrangThai, row.GhiChuDuyet, CoTheHienTen = true, row.TenNguoiDangCongKhai };
    }

    [AllowAnonymous, HttpGet("cong-khai")]
    public async Task<object> Public(CancellationToken ct) =>
        (await (from r in Reviews
                join h in db.HoSoTheoHoc on r.HoSoTheoHocId equals h.Id
                where r.TrangThai == "DA_DUYET" && r.DongYCongKhai && h.KhoaHoc!.TrangThai == "HOAT_DONG"
                orderby r.UpdatedAt descending
                select new { r.Id, r.SoSao, r.NoiDung, r.TenNguoiDangCongKhai, TenKhoa = h.KhoaHoc!.TenKhoa, CoAnh = r.Anh != null })
            .Take(12).ToListAsync(ct))
        .Select(x => new { x.Id, x.SoSao, x.NoiDung, x.TenKhoa, x.TenNguoiDangCongKhai, Anh = x.CoAnh ? Photo(x.Id) : null });

    [Authorize(Roles = Admin), HttpGet("quan-ly")]
    public async Task<object> Queue(CancellationToken ct) =>
        await (from r in Reviews
               join h in db.HoSoTheoHoc on r.HoSoTheoHocId equals h.Id
               orderby r.TrangThai == "CHO_DUYET" descending, r.CreatedAt descending
               select new { r.Id, TenKhoa = h.KhoaHoc!.TenKhoa, HocVien = h.HocVien!.HoTen,
                   r.SoSao, r.NoiDung, r.TrangThai, r.DongYCongKhai, CoAnh = r.Anh != null, r.GhiChuDuyet, r.RowVersion })
            .Take(500).ToListAsync(ct);

    public sealed record Decision(string TrangThai, string? GhiChu, string RowVersion);

    [Authorize(Roles = Admin), HttpPost("{id:long}/duyet")]
    public async Task<IActionResult> Moderate(long id, Decision decision, CancellationToken ct)
    {
        var row = await Reviews.SingleOrDefaultAsync(x => x.Id == id, ct) ?? throw new ApiError(404, "Không có đánh giá.");
        ApiError.Require(Convert.ToBase64String(row.RowVersion) == decision.RowVersion, "Đánh giá đã thay đổi. Hãy tải lại.", 409);
        ApiError.Require(row.DongYCongKhai && (row.TrangThai == "CHO_DUYET" && decision.TrangThai is "DA_DUYET" or "TU_CHOI"
            || row.TrangThai == "DA_DUYET" && decision.TrangThai == "AN"), "Thao tác không phù hợp với trạng thái đánh giá.");
        ApiError.Require((decision.GhiChu?.Length ?? 0) <= 500, "Ghi chú tối đa 500 ký tự.");
        row.TrangThai = decision.TrangThai;
        row.GhiChuDuyet = decision.GhiChu?.Trim();
        audit.Add("DanhGiaKhoaHoc", id, "DUYET_DANH_GIA_KHOA");
        await db.SaveChangesAsync(ct);
        return NoContent();
    }

    [AllowAnonymous, HttpGet("{id:long}/anh")]
    public async Task<IActionResult> Image(long id, CancellationToken ct)
    {
        var row = await (from r in Reviews
                         join h in db.HoSoTheoHoc on r.HoSoTheoHocId equals h.Id
                         where r.Id == id && r.Anh != null &&
                            ((r.DongYCongKhai && r.TrangThai == "DA_DUYET" && h.KhoaHoc!.TrangThai == "HOAT_DONG")
                                || User.IsInRole(Admin) || (long?)r.NguoiGuiId == User.MemberId())
                         select new { r.Anh, r.LoaiAnh }).SingleOrDefaultAsync(ct);
        if (row is null) return NotFound();
        Response.Headers.CacheControl = "no-store";
        Response.Headers.XContentTypeOptions = "nosniff";
        return File(row.Anh!, row.LoaiAnh == "jpg" ? "image/jpeg" : $"image/{row.LoaiAnh}");
    }

    async Task<long> CheckProfile(long profileId, CancellationToken ct)
    {
        var member = User.MemberId() ?? throw new ApiError(403, "Tài khoản cần liên kết thành viên.");
        var profile = await db.HoSoTheoHoc.FindAsync([profileId], ct) ?? throw new ApiError(404, "Không có hồ sơ học tập.");
        await access.Student(profile.HocVienId, false, ct);
        ApiError.Require(await db.KetQuaKhoaHoc.AnyAsync(x => x.HoSoTheoHocId == profileId && x.KetLuan == "HOAN_THANH", ct),
            "Chỉ gửi đánh giá sau khi trung tâm xác nhận hoàn thành khóa học.");
        return member;
    }
}
