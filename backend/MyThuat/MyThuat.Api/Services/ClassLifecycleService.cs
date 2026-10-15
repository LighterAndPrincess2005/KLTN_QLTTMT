using System.Data;
using Microsoft.EntityFrameworkCore;
using MyThuat.Api.Contracts;
using MyThuat.Api.Data;
using MyThuat.Api.Models;
using MyThuat.Api.Security;
namespace MyThuat.Api.Services;
public sealed class ClassLifecycleService(MyThuatDbContext db,AccessService access,EnrollmentService enrollments,
    ScheduleService schedule,ChangeService changes,AuditService audit)
{
    public async Task<object> Postpone(long id,PostponeClassRequest r,CancellationToken ct)
    {
        ApiError.Require(access.User.IsInRole("QUAN_TRI"),"Quản trị duyệt hoãn lớp.",403);
        await using var tx=await db.Database.BeginTransactionAsync(IsolationLevel.Serializable,ct);
        var cls=await enrollments.LockClass(id,ct);
        ApiError.Require(cls.TrangThai is "NHAP" or "DANG_TUYEN_SINH","Lớp đã bắt đầu hoặc kết thúc.");
        ApiError.Require(!await db.NhatKyThayDoi.AnyAsync(x=>x.LoaiDoiTuong=="LopHoc"&&x.IdDoiTuong==id&&x.HanhDong=="HOAN_LOP",ct),"Lớp chỉ được hoãn một lần.",409);
        var slots=await db.BuoiHoc.Where(x=>x.LopHocId==id&&x.TrangThai=="DA_XEP_LICH").OrderBy(x=>x.ThuTuTrongLop).ToListAsync(ct);
        ApiError.Require(slots.Count==cls.SoBuoiKeHoach&&slots[0].BatDau>BusinessClock.Now,"Chỉ hoãn lớp chưa khai giảng, có đủ lịch.");
        var delta=r.BuoiDauMoi-slots[0].BatDau;
        ApiError.Require(delta>TimeSpan.Zero&&delta<=TimeSpan.FromDays(7)&&r.BuoiDauMoi.Kind!=DateTimeKind.Utc,"Chỉ hoãn tối đa 7 ngày, dùng giờ Việt Nam.");
        foreach(var s in slots)s.TrangThai="DA_HUY";await db.SaveChangesAsync(ct);
        foreach(var s in slots)
        {
            var staff=await db.GiangDayBuoi.Where(x=>x.BuoiHocId==s.Id).ToListAsync(ct);
            var begin=s.BatDau+delta;var end=s.KetThuc+delta;
            await schedule.CheckSlot(id,s.PhongHocId,begin,end,staff.Select(x=>x.GiaoVienId),s.Id,ct);
            var replacement=new BuoiHoc {LopHocId=id,NoiDungBuoiId=s.NoiDungBuoiId,PhongHocId=s.PhongHocId,BuoiThayTheChoId=s.Id,
                ThuTuTrongLop=s.ThuTuTrongLop,LanXepLich=(short)(s.LanXepLich+1),BatDau=begin,KetThuc=end,SoTiet=s.SoTiet,
                PhutMoiTiet=s.PhutMoiTiet,PhutNghi=s.PhutNghi,LyDoDoiLich=r.LyDo,TrangThai="DA_XEP_LICH"};
            db.BuoiHoc.Add(replacement);await db.SaveChangesAsync(ct);
            foreach(var teacher in staff)db.GiangDayBuoi.Add(new(){BuoiHocId=replacement.Id,GiaoVienId=teacher.GiaoVienId,
                VaiTro=teacher.VaiTro,SoTietThucDay=teacher.SoTietThucDay,NguoiDuyet=access.User.AccountId()});
            var records=await db.DiemDanh.Where(x=>x.BuoiHocId==s.Id&&x.TrangThai=="CHO_THAM_GIA").ToListAsync(ct);
            foreach(var old in records)
            {
                old.TrangThai="HUY_BO";
                var row=new DiemDanh {DangKyId=old.DangKyId,BuoiHocId=replacement.Id,NoiDungDuocTinhId=old.NoiDungDuocTinhId,LoaiThamGia=old.LoaiThamGia,TrangThai="CHO_THAM_GIA"};
                db.DiemDanh.Add(row);await db.SaveChangesAsync(ct);
                var makeup=await db.HocBu.SingleOrDefaultAsync(x=>x.DiemDanhBuId==old.Id,ct);if(makeup is not null)makeup.DiemDanhBuId=row.Id;
            }
        }
        cls.NgayKhaiGiangDuKien=DateOnly.FromDateTime(r.BuoiDauMoi);cls.NgayKetThucDuKien=DateOnly.FromDateTime(slots[^1].KetThuc+delta);
        cls.DongDangKyLuc=r.BuoiDauMoi.AddHours(-48);
        var assignments=await db.PhanCong.Where(x=>x.LopHocId==id).ToListAsync(ct);
        foreach(var a in assignments){a.TuNgay=cls.NgayKhaiGiangDuKien;a.DenNgay=cls.NgayKetThucDuKien.Value;}
        audit.Add("LopHoc",id,"HOAN_LOP",new {r.BuoiDauMoi,r.LyDo});await db.SaveChangesAsync(ct);await tx.CommitAsync(ct);return ManagementService.Scalars(cls);
    }
    public async Task<object> Cancel(long id,CancelClassRequest r,CancellationToken ct)
    {
        ApiError.Require(access.User.IsInRole("QUAN_TRI"),"Quản trị duyệt hủy lớp.",403);
        await using var tx=await db.Database.BeginTransactionAsync(IsolationLevel.Serializable,ct);
        var cls=await enrollments.LockClass(id,ct);ApiError.Require(cls.TrangThai is "NHAP" or "DANG_TUYEN_SINH" or "DANG_HOC","Lớp không còn được hủy.");
        var registrations=await db.DangKy.Include(x=>x.HoSoTheoHoc).Where(x=>x.LopHocId==id&&(x.TrangThai=="DA_XAC_NHAN"||x.TrangThai=="CHO_THANH_TOAN")).ToListAsync(ct);
        foreach(var e in registrations)
        {
            ApiError.Require(!await db.YeuCauThayDoi.AnyAsync(x=>x.DangKyNguonId==e.Id&&(x.TrangThai=="CHO_DUYET"||x.TrangThai=="DA_DUYET"),ct),"Đăng ký có yêu cầu đang xử lý; giải quyết trước khi hủy lớp.",409);
            var decision=new YeuCauThayDoi {DangKyNguonId=e.Id,NguoiYeuCauId=e.NguoiDangKyId,LoaiYeuCau="HUY",NguyenNhan="TRUNG_TAM",
                GuiLuc=BusinessClock.Now,LyDo=r.LyDo,NguoiDuyet=access.User.AccountId(),DuyetLuc=BusinessClock.Now,LyDoQuyetDinh=r.LyDo,
                IdempotencyKey="HUY_LOP-"+id+"-"+e.Id,RequestFingerprint=RequestKeys.Fingerprint(r),TrangThai="DA_DUYET"};
            await changes.Estimate(e,decision,ct);
            if(decision.SoTienHoanDuKien==0){decision.TrangThai="DA_THUC_HIEN";decision.HoanTatLuc=BusinessClock.Now;}
            db.YeuCauThayDoi.Add(decision);e.TrangThai="DA_HUY";e.HieuLucDen=BusinessClock.Now;e.HoSoTheoHoc!.TrangThai="DA_HUY";
            foreach(var p in await db.ApDungUuDai.Where(x=>x.DangKyId==e.Id&&x.TrangThaiLuot=="GIU_LUOT").ToListAsync(ct))p.TrangThaiLuot="GIAI_PHONG";
        }
        var sessions=await db.BuoiHoc.Where(x=>x.LopHocId==id&&x.TrangThai=="DA_XEP_LICH").ToListAsync(ct);
        var sessionIds=sessions.Select(x=>x.Id).ToList();
        foreach(var s in sessions)s.TrangThai="DA_HUY";
        foreach(var attendance in await db.DiemDanh.Where(x=>sessionIds.Contains(x.BuoiHocId)&&x.TrangThai=="CHO_THAM_GIA").ToListAsync(ct))
        {
            attendance.TrangThai="HUY_BO";
            var makeup=await db.HocBu.SingleOrDefaultAsync(x=>x.DiemDanhBuId==attendance.Id,ct);
            if(makeup is not null){makeup.TrangThai="DA_HUY";makeup.DiemDanhBuId=null;}
        }
        cls.TrangThai="DA_HUY";audit.Add("LopHoc",id,"HUY_LOP",new {r.LyDo,SoDangKy=registrations.Count});
        await db.SaveChangesAsync(ct);await tx.CommitAsync(ct);return new {cls.Id,cls.TrangThai,SoPhuongAnHoan=registrations.Count};
    }
}
