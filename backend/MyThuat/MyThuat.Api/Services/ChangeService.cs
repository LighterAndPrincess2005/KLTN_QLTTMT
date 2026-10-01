using System.Data;
using Microsoft.EntityFrameworkCore;
using MyThuat.Api.Contracts;
using MyThuat.Api.Data;
using MyThuat.Api.Models;
using MyThuat.Api.Security;
namespace MyThuat.Api.Services;
public sealed class ChangeService(MyThuatDbContext db,AccessService access,EnrollmentService enrollments,
    FinanceService finance,PolicyService policies,AuditService audit)
{
    public async Task<object> Request(ChangeRequest r,CancellationToken ct)
    {
        ApiError.Require(r.LoaiYeuCau is "HUY" or "CHUYEN"&&r.NguyenNhan is "NGUOI_HOC" or "TRUNG_TAM","Loại yêu cầu hoặc nguyên nhân không hợp lệ.");
        ApiError.Require(r.NguyenNhan!="TRUNG_TAM"||access.User.Has(Permission.DaoTao),"Chỉ học vụ lập yêu cầu do trung tâm.",403);
        ApiError.Require(r.LoaiYeuCau=="CHUYEN"?r.LopDichId.HasValue:!r.LopDichId.HasValue,"Lớp đích không phù hợp loại yêu cầu.");
        var key=RequestKeys.Key(access.User.AccountId(),r.IdempotencyKey);var fingerprint=RequestKeys.Fingerprint(r);
        await using var tx=await db.Database.BeginTransactionAsync(IsolationLevel.Serializable,ct);
        var prior=await db.YeuCauThayDoi.SingleOrDefaultAsync(x=>x.IdempotencyKey==key,ct);
        if(prior is not null){ApiError.Require(prior.RequestFingerprint==fingerprint,"IdempotencyKey đã dùng với dữ liệu khác.",409);return ManagementService.Scalars(prior);}
        var e=await db.DangKy.Include(x=>x.HoSoTheoHoc).SingleOrDefaultAsync(x=>x.Id==r.DangKyId,ct)??throw new ApiError(404,"Không có đăng ký.");
        await access.Student(e.HoSoTheoHoc!.HocVienId,true,ct);
        ApiError.Require(e.TrangThai is "DA_XAC_NHAN" or "CHO_THANH_TOAN" or "HET_HAN","Đăng ký không còn được thay đổi.");
        ApiError.Require(!await db.YeuCauThayDoi.AnyAsync(x=>x.DangKyNguonId==e.Id&&(x.TrangThai=="CHO_DUYET"||x.TrangThai=="DA_DUYET"),ct),"Đang có yêu cầu chờ xử lý.",409);
        var rqt=new YeuCauThayDoi {DangKyNguonId=e.Id,LopDichId=r.LopDichId,NguoiYeuCauId=access.User.MemberId()??e.NguoiDangKyId,
            LoaiYeuCau=r.LoaiYeuCau,NguyenNhan=r.NguyenNhan,GuiLuc=BusinessClock.Now,LyDo=r.LyDo,TrangThai="CHO_DUYET",
            IdempotencyKey=key,RequestFingerprint=fingerprint};
        await Estimate(e,rqt,ct);
        db.YeuCauThayDoi.Add(rqt);await db.SaveChangesAsync(ct);audit.Add("YeuCauThayDoi",rqt.Id,"GUI_YEU_CAU",new {r.LoaiYeuCau,r.NguyenNhan});
        await db.SaveChangesAsync(ct);await tx.CommitAsync(ct);return ManagementService.Scalars(rqt);
    }
    public async Task Estimate(DangKy e,YeuCauThayDoi r,CancellationToken ct)
    {
        var profile=await db.HoSoTheoHoc.Include(x=>x.KhoaHoc).SingleAsync(x=>x.Id==e.HoSoTheoHocId,ct);
        var settings=await policies.ForProfile(profile.Id,ct);
        var slots=await db.BuoiHoc.Where(x=>x.LopHocId==e.LopHocId&&x.TrangThai!="DA_HUY").OrderBy(x=>x.ThuTuTrongLop).ToListAsync(ct);
        ApiError.Require(slots.Count>0,"Lớp chưa có lịch.");
        var ownSlots=await db.DiemDanh.Include(x=>x.BuoiHoc).Where(x=>x.DangKyId==e.Id&&x.LoaiThamGia=="HOC_CHINH"&&x.TrangThai!="HUY_BO").ToListAsync(ct);
        var used=ownSlots.Count(x=>x.BuoiHoc!.BatDau<=r.GuiLuc);
        r.SoBuoiDaSuDungChot=(short)used;
        var tuition=await db.KhoanThuDangKy.SingleAsync(x=>x.DangKyId==e.Id&&x.LoaiKhoan=="HOC_PHI",ct);
        var paid=await finance.Balance(tuition,ct);
        ApiError.Require(paid>=0,"Số dư học phí không hợp lệ.");
        if(r.LoaiYeuCau=="HUY")
        {
            r.HanThucHien=BusinessClock.AddWorkingDays(BusinessClock.Now,5,(await policies.ForProfile(e.HoSoTheoHocId,ct)).NgayNghi);
            decimal rate=r.NguyenNhan=="TRUNG_TAM"?(ownSlots.Count==0?1:(decimal)(ownSlots.Count-used)/ownSlots.Count)
                :r.GuiLuc<=slots[0].BatDau.AddHours(-48)?1:r.GuiLuc<slots[0].BatDau?0.9m:0;
            r.SoTienHoanDuKien=decimal.Round(paid*rate,0,MidpointRounding.AwayFromZero);
            r.GiaTriChuyenDuKien=0;
            var amounts=new Dictionary<long,decimal>{{tuition.Id,r.SoTienHoanDuKien??0}};
            foreach(var c in await db.KhoanThuDangKy.Where(x=>x.DangKyId==e.Id&&x.LoaiKhoan=="HOA_CU").ToListAsync(ct))
            {
                var refundable=decimal.Round((await finance.Balance(c,ct))*(c.SoLuong-c.SoLuongDaGiao)/c.SoLuong,0,MidpointRounding.AwayFromZero);
                amounts[c.Id]=Math.Max(0,refundable);
            }
            r.SoTienHoanDuKien=amounts.Values.Sum();
            r.PhuongAnHoanJson=System.Text.Json.JsonSerializer.Serialize(amounts);
        }
        else
        {
            ApiError.Require(e.TrangThai=="DA_XAC_NHAN","Chỉ chuyển đăng ký đã xác nhận.");
            ApiError.Require(slots.Count<3||r.GuiLuc<slots[2].BatDau,"Yêu cầu chuyển phải gửi trước buổi thứ ba.");
            ApiError.Require(await db.YeuCauThayDoi.CountAsync(x=>x.DangKyNguon!.HoSoTheoHocId==profile.Id&&x.LoaiYeuCau=="CHUYEN"&&x.TrangThai=="DA_THUC_HIEN",ct)<settings.SoLanChuyen,"Đã hết lượt chuyển của toàn khóa.");
            var target=await db.LopHoc.SingleOrDefaultAsync(x=>x.Id==r.LopDichId,ct)??throw new ApiError(400,"Không có lớp đích.");
            ApiError.Require(target.Id!=e.LopHocId&&target.KhoaHocId==profile.KhoaHocId&&target.TrangThai is "DANG_TUYEN_SINH" or "DANG_HOC","Lớp đích phải cùng phiên bản khóa và đang hoạt động.");
            var remaining=slots.Where(x=>x.BatDau>r.GuiLuc).Select(x=>x.NoiDungBuoiId).ToList();
            var targetRemaining=await db.BuoiHoc.Where(x=>x.LopHocId==target.Id&&x.TrangThai=="DA_XEP_LICH"&&x.BatDau>BusinessClock.Now)
                .OrderBy(x=>x.ThuTuTrongLop).Select(x=>x.NoiDungBuoiId).ToListAsync(ct);
            ApiError.Require(remaining.Count>0&&remaining.SequenceEqual(targetRemaining),"Lớp đích chưa khớp các nội dung còn lại.");
            r.GiaTriChuyenDuKien=decimal.Round(paid*remaining.Count/slots.Count,0,MidpointRounding.AwayFromZero);
            var targetFee=decimal.Round(target.HocPhiApDung*remaining.Count/profile.KhoaHoc!.SoBuoi,0,MidpointRounding.AwayFromZero);
            r.SoTienHoanDuKien=Math.Max(0,r.GiaTriChuyenDuKien.Value-targetFee);
            r.PhuongAnHoanJson=System.Text.Json.JsonSerializer.Serialize(new Dictionary<long,decimal>{{tuition.Id,r.SoTienHoanDuKien.Value}});
        }
    }
    public async Task<object> Approve(long id,ChangeDecision decision,CancellationToken ct)
    {
        ApiError.Require(access.User.IsInRole("QUAN_TRI"),"Quản trị duyệt phương án thay đổi có tiền.",403);
        await using var tx=await db.Database.BeginTransactionAsync(IsolationLevel.Serializable,ct);
        var r=await db.YeuCauThayDoi.SingleOrDefaultAsync(x=>x.Id==id,ct)??throw new ApiError(404,"Không có yêu cầu.");
        ApiError.Require(r.TrangThai=="CHO_DUYET","Yêu cầu đã được xử lý.",409);
        r.NguoiDuyet=access.User.AccountId();r.DuyetLuc=BusinessClock.Now;r.LyDoQuyetDinh=decision.LyDo;
        if(!decision.DongY){r.TrangThai="TU_CHOI";await db.SaveChangesAsync(ct);await tx.CommitAsync(ct);return ManagementService.Scalars(r);}
        var e=await db.DangKy.Include(x=>x.HoSoTheoHoc).SingleAsync(x=>x.Id==r.DangKyNguonId,ct);
        await enrollments.LockClass(e.LopHocId,ct);await Estimate(e,r,ct);
        r.TrangThai="DA_DUYET";
        if(r.LoaiYeuCau=="HUY")
        {
            r.HanThucHien=BusinessClock.AddWorkingDays(BusinessClock.Now,5,(await policies.ForProfile(e.HoSoTheoHocId,ct)).NgayNghi);
            e.TrangThai="DA_HUY";e.HieuLucDen=BusinessClock.Now;e.HoSoTheoHoc!.TrangThai="DA_HUY";
            foreach(var a in await db.DiemDanh.Where(x=>x.DangKyId==e.Id&&x.TrangThai=="CHO_THAM_GIA").ToListAsync(ct))a.TrangThai="HUY_BO";
            foreach(var p in await db.ApDungUuDai.Where(x=>x.DangKyId==e.Id&&x.TrangThaiLuot=="GIU_LUOT").ToListAsync(ct))p.TrangThaiLuot="GIAI_PHONG";
            if(r.SoTienHoanDuKien==0){r.TrangThai="DA_THUC_HIEN";r.HoanTatLuc=BusinessClock.Now;}
        }
        else
        {
            var target=await enrollments.LockClass(r.LopDichId!.Value,ct);
            await enrollments.EnsureCapacity(target,e.HoSoTheoHoc!.HocVienId,ct,e.Id);
            var settings=await policies.ForProfile(e.HoSoTheoHocId,ct);
            r.HanThucHien=BusinessClock.Now.AddHours(settings.HanGiuChoGio);
            var remainingCount=await db.BuoiHoc.CountAsync(x=>x.LopHocId==target.Id&&x.TrangThai=="DA_XEP_LICH"&&x.BatDau>BusinessClock.Now,ct);
            var total=(await db.KhoaHoc.SingleAsync(x=>x.Id==target.KhoaHocId,ct)).SoBuoi;
            var fee=decimal.Round(target.HocPhiApDung*remainingCount/total,0,MidpointRounding.AwayFromZero);
            var next=new DangKy {MaDangKy="DK"+Guid.NewGuid().ToString("N")[..20],HoSoTheoHocId=e.HoSoTheoHocId,LopHocId=target.Id,
                NguoiDangKyId=e.NguoiDangKyId,DangKyNguonId=e.Id,LapLuc=BusinessClock.Now,HanGiuCho=r.HanThucHien.Value,
                HocPhiGocChot=fee,TrangThai="CHO_CHUYEN",IdempotencyKey="CHUYEN-"+r.Id,RequestFingerprint=r.RequestFingerprint};
            db.DangKy.Add(next);await db.SaveChangesAsync(ct);r.DangKyDichId=next.Id;
            db.KhoanThuDangKy.Add(new(){DangKyId=next.Id,LoaiKhoan="HOC_PHI",MoTaChot="Học phí phần còn lại khi chuyển lớp",SoLuong=1,DonGiaChot=fee,ThanhTienChot=fee});
        }
        audit.Add("YeuCauThayDoi",id,"DUYET_THAY_DOI",new {r.LoaiYeuCau,r.SoTienHoanDuKien,r.GiaTriChuyenDuKien});
        await db.SaveChangesAsync(ct);await tx.CommitAsync(ct);return ManagementService.Scalars(r);
    }
    public async Task<object> FinalizeTransfer(long id,CancellationToken ct)
    {
        access.Need(Permission.DaoTao);
        await using var tx=await db.Database.BeginTransactionAsync(IsolationLevel.Serializable,ct);
        var r=await db.YeuCauThayDoi.SingleOrDefaultAsync(x=>x.Id==id,ct)??throw new ApiError(404,"Không có yêu cầu.");
        ApiError.Require(r.LoaiYeuCau=="CHUYEN"&&r.TrangThai=="DA_DUYET"&&r.HanThucHien>BusinessClock.Now,"Không còn phương án chuyển hiệu lực.");
        var e=await db.DangKy.Include(x=>x.HoSoTheoHoc).SingleAsync(x=>x.Id==r.DangKyNguonId,ct);
        var next=await db.DangKy.SingleAsync(x=>x.Id==r.DangKyDichId,ct);
        var target=await enrollments.LockClass(next.LopHocId,ct);
        ApiError.Require(next.TrangThai=="CHO_CHUYEN"&&e.TrangThai=="DA_XAC_NHAN","Trạng thái đăng ký không phù hợp.");
        ApiError.Require(!await db.BuoiHoc.AnyAsync(x=>x.LopHocId==e.LopHocId&&x.TrangThai!="DA_HUY"&&x.BatDau>r.GuiLuc&&x.BatDau<=BusinessClock.Now,ct),
            "Lớp nguồn đã học thêm sau phương án duyệt; hết hiệu lực chuyển, cần xử lý lại.");
        var targetCharge=await db.KhoanThuDangKy.SingleAsync(x=>x.DangKyId==next.Id&&x.LoaiKhoan=="HOC_PHI",ct);
        var credit=Math.Min(r.GiaTriChuyenDuKien??0,targetCharge.ThanhTienChot);
        ApiError.Require(await finance.Balance(targetCharge,ct)+credit>=targetCharge.ThanhTienChot,"Chưa đủ tiền chênh lệch.");
        var settings=await policies.ForProfile(e.HoSoTheoHocId,ct);
        ApiError.Require(await db.YeuCauThayDoi.CountAsync(x=>x.DangKyNguon!.HoSoTheoHocId==e.HoSoTheoHocId&&x.TrangThai=="DA_THUC_HIEN"&&x.LoaiYeuCau=="CHUYEN",ct)<settings.SoLanChuyen,"Lượt chuyển đã được dùng.");
        e.TrangThai="DA_CHUYEN";e.HieuLucDen=BusinessClock.Now;
        foreach(var a in await db.DiemDanh.Where(x=>x.DangKyId==e.Id&&x.TrangThai=="CHO_THAM_GIA"&&x.LoaiThamGia=="HOC_CHINH").ToListAsync(ct))a.TrangThai="HUY_BO";
        next.TrangThai="DA_XAC_NHAN";next.HieuLucTu=BusinessClock.Now;
        r.GiaTriChuyenThucTe=credit;r.TrangThai="DA_THUC_HIEN";r.HoanTatLuc=BusinessClock.Now;
        foreach(var b in await db.BuoiHoc.Where(x=>x.LopHocId==target.Id&&x.TrangThai=="DA_XEP_LICH"&&x.BatDau>BusinessClock.Now).ToListAsync(ct))
            db.DiemDanh.Add(new(){DangKyId=next.Id,BuoiHocId=b.Id,NoiDungDuocTinhId=b.NoiDungBuoiId,LoaiThamGia="HOC_CHINH",TrangThai="CHO_THAM_GIA"});
        audit.Add("YeuCauThayDoi",id,"HOAN_TAT_CHUYEN",new {credit});
        await db.SaveChangesAsync(ct);await tx.CommitAsync(ct);return ManagementService.Scalars(r);
    }
}

