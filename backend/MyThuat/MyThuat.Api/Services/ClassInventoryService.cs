using System.Data;
using Microsoft.EntityFrameworkCore;
using MyThuat.Api.Data;
using MyThuat.Api.Models;

namespace MyThuat.Api.Services;

public sealed class ClassInventoryService(MyThuatDbContext db, InventoryService inventory, AuditService audit)
{
    private IQueryable<PhieuKho> Reservations(long classId) =>
        db.PhieuKho.Where(x => x.LopHocId == classId && x.LoaiPhieu == "DU_TRU" && x.TrangThai == "DA_DUYET");

    private IQueryable<PhieuKho> LegacyMovements(long classId) =>
        db.PhieuKho.Where(x => x.BuoiHoc!.LopHocId == classId &&
            (x.LoaiPhieu == "XUAT_TIEU_HAO" || x.LoaiPhieu == "CAP_DUNG_CHUNG" || x.LoaiPhieu == "DU_TRU") &&
            (x.TrangThai == "DA_GHI_SO" || x.TrangThai == "DA_DUYET"));

    public async Task Reserve(LopHoc cls, long actor, CancellationToken ct)
    {
        // The caller owns the serializable transaction and the class lock.
        await inventory.LockLedger(ct);
        var lastDay = cls.NgayKetThucDuKien ?? cls.NgayKhaiGiangDuKien;
        var existing = await Reservations(cls.Id).SingleOrDefaultAsync(ct);
        if (existing is not null)
        {
            ApiError.Require(!await db.ChiTietPhieuKho.AnyAsync(x => x.PhieuKhoId == existing.Id && x.HanSuDung < lastDay, ct),
                "Lô đã giữ không còn đủ hạn dùng đến buổi cuối của lớp.", 409);
            return;
        }

        var definitions = await db.DinhMucHoaCu.AsNoTracking()
            .Where(x => x.NoiDungBuoi!.KhoaHocId == cls.KhoaHocId).ToListAsync(ct);
        if (definitions.Count == 0) return;

        ApiError.Require(!await LegacyMovements(cls.Id).AnyAsync(ct),
            "Lớp có phiếu kho theo buổi từ trước. Đối chiếu các phiếu cũ trước khi chuyển sang dự trữ tự động.", 409);

        var lines = new List<ChiTietPhieuKho>();
        foreach (var item in definitions.GroupBy(x => x.HoaCuId).OrderBy(x => x.Key))
        {
            var material = await inventory.LockMaterial(item.Key, ct);
            ApiError.Require(material.TrangThai == "HOAT_DONG" && material.NhomHoaCu is "TIEU_HAO" or "DUNG_CHUNG",
                $"Họa cụ {material.TenHoaCu} đã ngừng dùng hoặc thuộc nhóm bán riêng.", 409);

            var perSession = item.GroupBy(x => x.NoiDungBuoiId)
                .Select(g => g.Sum(x => x.SoLuong * (x.CoSoTinh == "HOC_VIEN" ? cls.SiSoToiDa : 1)));
            // Shared tools are reused by the same class across its sessions.
            var required = material.NhomHoaCu == "DUNG_CHUNG" ? perSession.Max() : perSession.Sum();
            var remaining = required;
            var receipts = await db.ChiTietPhieuKho.AsNoTracking()
                .Where(x => x.HoaCuId == material.Id && x.PhieuKho!.TrangThai == "DA_GHI_SO" &&
                    (x.PhieuKho.LoaiPhieu == "NHAP" || x.PhieuKho.LoaiPhieu == "NHAP_DIEU_CHINH") &&
                    (x.HanSuDung == null || x.HanSuDung >= lastDay))
                .ToListAsync(ct);
            foreach (var lot in receipts.GroupBy(x => x.MaLo)
                .Select(g => g.OrderByDescending(x => x.Id).First())
                .OrderBy(x => x.HanSuDung ?? DateOnly.MaxValue).ThenBy(x => x.MaLo, StringComparer.Ordinal))
            {
                var qty = await inventory.Quantities(material.Id, lot.MaLo, ct);
                var take = Math.Min(remaining, qty.stock - qty.reserved);
                if (take <= 0) continue;
                lines.Add(new ChiTietPhieuKho { HoaCuId = material.Id, MaLo = lot.MaLo,
                    HanSuDung = lot.HanSuDung, SoLuong = take, DonGiaChot = lot.DonGiaChot });
                remaining -= take;
                if (remaining == 0) break;
            }
            ApiError.Require(remaining == 0,
                $"Thiếu {remaining:0.###} {material.DonViCoSo} {material.TenHoaCu}. Lớp cần {required:0.###} cho tối đa {cls.SiSoToiDa} học viên.", 409);
        }
        await Record(cls, "DU_TRU", "DA_DUYET", actor, null, lines, ct);
    }

    public async Task Issue(LopHoc cls, long actor, CancellationToken ct)
    {
        await inventory.LockLedger(ct);
        if (await db.PhieuKho.AnyAsync(x => x.LopHocId == cls.Id && x.TrangThai == "DA_GHI_SO", ct)) return;
        if (!await Reservations(cls.Id).AnyAsync(ct) && await LegacyMovements(cls.Id).AnyAsync(ct))
        {
            // Older classes may already use the per-session ledger. Keep that path to avoid
            // issuing the same supplies again at the first start after an upgrade.
            audit.Add("LopHoc", cls.Id, "TIEP_TUC_PHIEU_KHO_CU", actor: actor);
            return;
        }
        await Reserve(cls, actor, ct);
        var reservation = await Reservations(cls.Id).SingleOrDefaultAsync(ct);
        if (reservation is null) return;

        var lines = await db.ChiTietPhieuKho.Include(x => x.HoaCu)
            .Where(x => x.PhieuKhoId == reservation.Id).OrderBy(x => x.HoaCuId).ToListAsync(ct);
        foreach (var line in lines)
        {
            var material = await inventory.LockMaterial(line.HoaCuId, ct);
            var qty = await inventory.Quantities(line.HoaCuId, line.MaLo, ct, reservation.Id);
            ApiError.Require(material.TrangThai == "HOAT_DONG" && qty.stock - qty.reserved >= line.SoLuong &&
                (line.HanSuDung is null || line.HanSuDung >= BusinessClock.Today),
                $"Không thể xuất {material.TenHoaCu} từ lô đã giữ. Kiểm tra tồn và hạn dùng.", 409);
        }

        reservation.TrangThai = "DA_GIAI_PHONG";
        foreach (var group in lines.GroupBy(x => x.HoaCu!.NhomHoaCu))
        {
            var type = group.Key == "DUNG_CHUNG" ? "CAP_DUNG_CHUNG" : "XUAT_TIEU_HAO";
            await Record(cls, type, "DA_GHI_SO", actor, reservation.Id, group.Select(x => new ChiTietPhieuKho
            {
                HoaCuId = x.HoaCuId, MaLo = x.MaLo, HanSuDung = x.HanSuDung, SoLuong = x.SoLuong,
                DonGiaChot = x.DonGiaChot, ChiTietThamChieuId = x.Id
            }).ToList(), ct);
        }
    }

    public async Task Release(long classId, string reason, CancellationToken ct)
    {
        await inventory.LockLedger(ct);
        var slip = await Reservations(classId).SingleOrDefaultAsync(ct);
        if (slip is null) return;
        slip.TrangThai = "DA_HUY";
        slip.LyDo = reason;
        audit.Add("PhieuKho", slip.Id, "GIAI_PHONG_DU_TRU_LOP", new { LopHocId = classId, LyDo = reason },
            actor: slip.NguoiDuyet);
        await db.SaveChangesAsync(ct);
    }

    public async Task ReleaseExpired(CancellationToken ct)
    {
        var now = BusinessClock.Now;
        var candidates = await db.PhieuKho.Where(x => x.LoaiPhieu == "DU_TRU" && x.TrangThai == "DA_DUYET" &&
            x.LopHoc!.TrangThai == "DANG_TUYEN_SINH" && x.LopHoc.DongDangKyLuc <= now)
            .Select(x => x.LopHocId!.Value).Distinct().ToListAsync(ct);
        foreach (var id in candidates.Order())
        {
            await using var tx = await db.Database.BeginTransactionAsync(IsolationLevel.Serializable, ct);
            var cls = db.Database.IsSqlServer()
                ? await db.LopHoc.FromSqlInterpolated($"SELECT * FROM dbo.LopHoc WITH (UPDLOCK,HOLDLOCK) WHERE Id={id}").SingleAsync(ct)
                : await db.LopHoc.SingleAsync(x => x.Id == id, ct);
            if (cls.TrangThai == "DANG_TUYEN_SINH" && cls.DongDangKyLuc <= now &&
                await db.DangKy.CountAsync(x => x.LopHocId == id && x.TrangThai == "DA_XAC_NHAN", ct) < cls.SiSoToiThieu)
                await Release(id, "Hết hạn tuyển sinh và chưa đủ học viên đã thanh toán.", ct);
            await tx.CommitAsync(ct);
        }
    }

    private async Task Record(LopHoc cls, string type, string state, long actor, long? reference,
        List<ChiTietPhieuKho> lines, CancellationToken ct)
    {
        var code = await CodeGenerator.NextAsync(db, db.PhieuKho, x => x.MaPhieu, "PKL-", ct);
        var slip = new PhieuKho
        {
            LopHocId = cls.Id, MaPhieu = code, LoaiPhieu = type, TrangThai = state,
            PhieuThamChieuId = reference, NguoiLap = actor, NguoiDuyet = actor,
            LapLuc = BusinessClock.Now, DuyetLuc = BusinessClock.Now,
            LyDo = $"Chuẩn bị họa cụ cho {cls.TenLop}, tối đa {cls.SiSoToiDa} học viên."
        };
        db.PhieuKho.Add(slip);
        foreach (var line in lines)
        {
            line.PhieuKho = slip;
            db.ChiTietPhieuKho.Add(line);
        }
        await db.SaveChangesAsync(ct);
        audit.Add("PhieuKho", slip.Id, "HOA_CU_LOP", new { cls.Id, LoaiPhieu = type }, actor: actor);
        await db.SaveChangesAsync(ct);
    }
}
