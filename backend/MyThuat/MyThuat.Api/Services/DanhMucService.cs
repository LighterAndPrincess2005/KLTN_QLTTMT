using Microsoft.EntityFrameworkCore;
using MyThuat.Api.Contracts;
using MyThuat.Api.Data;
using MyThuat.Api.Models;
namespace MyThuat.Api.Services;

public sealed class DanhMucService(MyThuatDbContext db)
{
    // Quy ước của API bản đầu; chỉ công bố khóa đã duyệt và lớp đang tuyển sinh.
    private IQueryable<KhoaHoc> KhoaCongBo => db.KhoaHoc.AsNoTracking()
        .Where(x => x.TrangThai == "HOAT_DONG" && x.LoaiKhoaHoc!.TrangThai == "HOAT_DONG");
    private IQueryable<LopHoc> LopCongBo => db.LopHoc.AsNoTracking()
        .Where(x => x.TrangThai == "DANG_TUYEN_SINH" && x.KhoaHoc!.TrangThai == "HOAT_DONG"
            && x.KhoaHoc.LoaiKhoaHoc!.TrangThai == "HOAT_DONG");

    public async Task<List<LoaiKhoaHocDto>> LayLoai(CancellationToken ct) =>
        await db.LoaiKhoaHoc.AsNoTracking().Where(x => x.TrangThai == "HOAT_DONG")
            .OrderBy(x => x.TenLoai).Select(x => new LoaiKhoaHocDto(x.Id, x.MaLoai, x.TenLoai, x.MoTa)).ToListAsync(ct);

    public async Task<TrangKetQua<KhoaHocDto>> LayKhoa(int trang, int kichThuoc, long? loaiId, string? tuKhoa, CancellationToken ct)
    {
        var query = KhoaCongBo;
        if (loaiId.HasValue) query = query.Where(x => x.LoaiKhoaHocId == loaiId);
        if (!string.IsNullOrWhiteSpace(tuKhoa)) query = query.Where(x => x.TenKhoa.Contains(tuKhoa.Trim()));
        var count = await query.CountAsync(ct);
        var rows = await ChieuKhoa(query.OrderBy(x => x.Id).Skip((trang - 1) * kichThuoc).Take(kichThuoc)).ToListAsync(ct);
        return new(trang, kichThuoc, count, rows);
    }

    private static IQueryable<KhoaHocDto> ChieuKhoa(IQueryable<KhoaHoc> query) => query.Select(x =>
        new KhoaHocDto(x.Id, x.LoaiKhoaHocId, x.MaKhoa, x.TenKhoa, x.PhienBan, x.CapDo,
            x.TuoiToiThieu, x.YeuCauDauVao, x.MucTieu, x.SoBuoi, x.SoTietMoiBuoi, x.PhutMoiTiet, x.HocPhi));
    public Task<KhoaHocDto?> LayKhoa(long id, CancellationToken ct) => ChieuKhoa(KhoaCongBo.Where(x => x.Id == id)).SingleOrDefaultAsync(ct);

    public async Task<List<NoiDungBuoiDto>?> LayNoiDung(long id, CancellationToken ct)
    {
        if (!await KhoaCongBo.AnyAsync(x => x.Id == id, ct)) return null;
        return await db.NoiDungBuoi.AsNoTracking().Where(x => x.KhoaHocId == id).OrderBy(x => x.ThuTu)
            .Select(x => new NoiDungBuoiDto(x.Id, x.ThuTu, x.ChuDe, x.NoiDung, x.SoTiet, x.YeuCauSanPham)).ToListAsync(ct);
    }

    public async Task<TrangKetQua<LopHocDto>> LayLop(int trang, int kichThuoc, long? khoaId, CancellationToken ct)
    {
        var query = LopCongBo;
        if (khoaId.HasValue) query = query.Where(x => x.KhoaHocId == khoaId);
        var count = await query.CountAsync(ct);
        var rows = await ChieuLop(query.OrderBy(x => x.NgayKhaiGiangDuKien).ThenBy(x => x.Id)
            .Skip((trang - 1) * kichThuoc).Take(kichThuoc)).ToListAsync(ct);
        return new(trang, kichThuoc, count, rows);
    }
    private static IQueryable<LopHocDto> ChieuLop(IQueryable<LopHoc> query) => query.Select(x =>
        new LopHocDto(x.Id, x.KhoaHocId, x.MaLop, x.TenLop, x.NgayKhaiGiangDuKien,
            x.NgayKetThucDuKien, x.SoBuoiKeHoach, x.LichHocDuKien, x.SiSoToiDa,
            x.HocPhiApDung, x.MoDangKyLuc, x.DongDangKyLuc));
    public Task<LopHocDto?> LayLop(long id, CancellationToken ct) => ChieuLop(LopCongBo.Where(x => x.Id == id)).SingleOrDefaultAsync(ct);
}
