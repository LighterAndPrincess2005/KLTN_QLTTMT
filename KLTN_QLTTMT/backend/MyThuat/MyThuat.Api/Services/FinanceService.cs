using System.Data;
using Microsoft.EntityFrameworkCore;
using MyThuat.Api.Contracts;
using MyThuat.Api.Data;
using MyThuat.Api.Models;
using MyThuat.Api.Security;
namespace MyThuat.Api.Services;
public sealed class FinanceService(MyThuatDbContext db,AccessService access,AuditService audit,EnrollmentService enrollments)
{
    public async Task<decimal> Balance(KhoanThuDangKy charge,CancellationToken ct)
    {
        var received=await db.GiaoDichTien.Where(x=>x.KhoanThuId==charge.Id&&x.TrangThai=="DA_XAC_NHAN").ToListAsync(ct);
        var balance=received.Sum(x=>x.LoaiGiaoDich=="THU"?x.SoTien:-x.SoTien);
        if(charge.LoaiKhoan=="HOC_PHI")
        {
            var transfers=await db.YeuCauThayDoi.Where(x=>x.TrangThai=="DA_THUC_HIEN"&&x.LoaiYeuCau=="CHUYEN"
                &&(x.DangKyNguonId==charge.DangKyId||x.DangKyDichId==charge.DangKyId)).ToListAsync(ct);
            balance+=transfers.Sum(x=>(x.DangKyDichId==charge.DangKyId?1:-1)*(x.GiaTriChuyenThucTe??0));
        }
        return balance;
    }
    public async Task<object> Receive(ReceiptRequest r,CancellationToken ct)
    {
        access.Need(Permission.ThuTien);
        ApiError.Require(r.SoTien>0&&r.SoTien==decimal.Truncate(r.SoTien)&&r.PhuongThuc is "TIEN_MAT" or "CHUYEN_KHOAN","Tiền VND phải nguyên dương, phương thức hợp lệ.");
        ApiError.Require(r.PhuongThuc!="CHUYEN_KHOAN"||!string.IsNullOrWhiteSpace(r.MaThamChieu),"Chuyển khoản cần mã tham chiếu.");
        var key=RequestKeys.Key(access.User.AccountId(),r.IdempotencyKey);var fingerprint=RequestKeys.Fingerprint(r);
        await using var tx=await db.Database.BeginTransactionAsync(IsolationLevel.Serializable,ct);
        var prior=await db.GiaoDichTien.SingleOrDefaultAsync(x=>x.IdempotencyKey==key,ct);
        if(prior is not null){ApiError.Require(prior.RequestFingerprint==fingerprint,"IdempotencyKey đã dùng với nội dung khác.",409);return ManagementService.Scalars(prior);}
        var e=await db.DangKy.SingleOrDefaultAsync(x=>x.Id==r.DangKyId,ct)??throw new ApiError(404,"Không có đăng ký.");
        await enrollments.LockClass(e.LopHocId,ct);await enrollments.Expire(ct);
        var charge=await db.KhoanThuDangKy.SingleOrDefaultAsync(x=>x.Id==r.KhoanThuId&&x.DangKyId==e.Id,ct)??throw new ApiError(400,"Khoản thu không thuộc đăng ký.");
        var remaining=charge.ThanhTienChot-await Balance(charge,ct);
        if(e.TrangThai=="CHO_CHUYEN")
        {
            var pending=await db.YeuCauThayDoi.SingleOrDefaultAsync(x=>x.DangKyDichId==e.Id&&x.TrangThai=="DA_DUYET"&&x.HanThucHien>BusinessClock.Now,ct);
            remaining-=Math.Min(charge.ThanhTienChot,pending?.GiaTriChuyenDuKien??0);
        }
        // Late and excess receipts are saved as unallocated money, never force an expired admission.
        var allocate=e.TrangThai is "DA_XAC_NHAN" or "CHO_THANH_TOAN" or "CHO_CHUYEN"&&remaining>=r.SoTien;
        var payment=new GiaoDichTien {DangKyId=e.Id,KhoanThuId=allocate?charge.Id:null,LoaiGiaoDich="THU",PhuongThuc=r.PhuongThuc,
            SoTien=r.SoTien,MaChungTu=r.MaChungTu,MaThamChieu=r.MaThamChieu,ThoiDiem=BusinessClock.Now,
            NguoiXacNhan=access.User.AccountId(),TrangThai=allocate?"DA_XAC_NHAN":"CHO_DOI_CHIEU",IdempotencyKey=key,RequestFingerprint=fingerprint};
        db.GiaoDichTien.Add(payment);await db.SaveChangesAsync(ct);
        if(allocate)await Confirm(e,ct);
        audit.Add("GiaoDichTien",payment.Id,"NHAN_TIEN",new {r.SoTien,payment.TrangThai});
        await db.SaveChangesAsync(ct);await tx.CommitAsync(ct);return ManagementService.Scalars(payment);
    }
    public async Task Confirm(DangKy e,CancellationToken ct)
    {
        if(e.TrangThai!="CHO_THANH_TOAN"||e.HanGiuCho<=BusinessClock.Now)return;
        var charges=await db.KhoanThuDangKy.Where(x=>x.DangKyId==e.Id).ToListAsync(ct);
        foreach(var charge in charges)if(await Balance(charge,ct)<charge.ThanhTienChot)return;
        e.TrangThai="DA_XAC_NHAN";e.HieuLucTu=BusinessClock.Now;
        foreach(var promo in await db.ApDungUuDai.Where(x=>x.DangKyId==e.Id&&x.TrangThaiLuot=="GIU_LUOT").ToListAsync(ct))promo.TrangThaiLuot="DA_DUNG";
    }
    public async Task<object> Refund(RefundRequest r,CancellationToken ct)
    {
        access.Need(Permission.ThuTien);
        ApiError.Require(r.SoTien>0&&r.SoTien==decimal.Truncate(r.SoTien),"Tiền hoàn phải nguyên dương.");
        var key=RequestKeys.Key(access.User.AccountId(),r.IdempotencyKey);var fingerprint=RequestKeys.Fingerprint(r);
        await using var tx=await db.Database.BeginTransactionAsync(IsolationLevel.Serializable,ct);
        var previous=await db.GiaoDichTien.SingleOrDefaultAsync(x=>x.IdempotencyKey==key,ct);
        if(previous is not null){ApiError.Require(previous.RequestFingerprint==fingerprint,"IdempotencyKey đã dùng với nội dung khác.",409);return ManagementService.Scalars(previous);}
        var source=await db.GiaoDichTien.SingleOrDefaultAsync(x=>x.Id==r.GiaoDichThuGocId&&x.LoaiGiaoDich=="THU",ct)??throw new ApiError(400,"Không có nguồn thu.");
        var already=await db.GiaoDichTien.Where(x=>x.GiaoDichThuGocId==source.Id&&x.LoaiGiaoDich=="HOAN"&&x.TrangThai=="DA_XAC_NHAN").ToListAsync(ct);
        ApiError.Require(already.Sum(x=>x.SoTien)+r.SoTien<=source.SoTien,"Tiền hoàn vượt nguồn thực thu.",409);
        if(source.TrangThai=="DA_XAC_NHAN")
        {
            var decision=await db.YeuCauThayDoi.SingleOrDefaultAsync(x=>x.Id==r.YeuCauThayDoiId&&x.DangKyNguonId==source.DangKyId
                &&(x.TrangThai=="DA_DUYET"||x.TrangThai=="DA_THUC_HIEN"),ct)??throw new ApiError(409,"Hoàn khoản phân bổ cần quyết định duyệt.");
            ApiError.Require(decision.LoaiYeuCau=="HUY"||decision.LoaiYeuCau=="CHUYEN"&&decision.TrangThai=="DA_THUC_HIEN","Chỉ hoàn chênh lệch chuyển khi đã hoàn tất chuyển.");
            var total=await db.GiaoDichTien.Where(x=>x.YeuCauThayDoiId==decision.Id&&x.LoaiGiaoDich=="HOAN"&&x.TrangThai=="DA_XAC_NHAN").ToListAsync(ct);
            ApiError.Require(total.Sum(x=>x.SoTien)+r.SoTien<=decision.SoTienHoanDuKien,"Hoàn vượt phương án đã duyệt.",409);
            var charge=await db.KhoanThuDangKy.SingleAsync(x=>x.Id==source.KhoanThuId,ct);
            var limits=System.Text.Json.JsonSerializer.Deserialize<Dictionary<long,decimal>>(decision.PhuongAnHoanJson)??[];
            var perCharge=total.Where(x=>x.KhoanThuId==charge.Id).Sum(x=>x.SoTien);
            ApiError.Require(limits.TryGetValue(charge.Id,out var allowed)&&perCharge+r.SoTien<=allowed,"Hoàn vượt phương án của khoản thu này.",409);
            ApiError.Require(await Balance(charge,ct)>=r.SoTien,"Không được hoàn tiền đã chuyển hoặc vượt số dư khoản thu.");
        }
        else ApiError.Require(source.TrangThai=="CHO_DOI_CHIEU"&&!r.YeuCauThayDoiId.HasValue,"Tiền chờ đối chiếu hoàn riêng, không gắn phương án hủy/chuyển.");
        var refund=new GiaoDichTien {DangKyId=source.DangKyId,KhoanThuId=source.KhoanThuId,GiaoDichThuGocId=source.Id,
            YeuCauThayDoiId=r.YeuCauThayDoiId,LoaiGiaoDich="HOAN",PhuongThuc=source.PhuongThuc,SoTien=r.SoTien,
            MaChungTu=r.MaChungTu,ThoiDiem=BusinessClock.Now,NguoiXacNhan=access.User.AccountId(),TrangThai="DA_XAC_NHAN",
            IdempotencyKey=key,RequestFingerprint=fingerprint};
        db.GiaoDichTien.Add(refund);await db.SaveChangesAsync(ct);
        if(r.YeuCauThayDoiId is long decisionId)
        {
            var d=await db.YeuCauThayDoi.SingleAsync(x=>x.Id==decisionId,ct);
            var refunds=await db.GiaoDichTien.Where(x=>x.YeuCauThayDoiId==d.Id&&x.LoaiGiaoDich=="HOAN"&&x.TrangThai=="DA_XAC_NHAN").ToListAsync(ct);
            if(d.LoaiYeuCau=="HUY"&&refunds.Sum(x=>x.SoTien)==d.SoTienHoanDuKien){d.TrangThai="DA_THUC_HIEN";d.HoanTatLuc=BusinessClock.Now;}
        }
        audit.Add("GiaoDichTien",refund.Id,"HOAN_TIEN",new {r.SoTien});await db.SaveChangesAsync(ct);await tx.CommitAsync(ct);return ManagementService.Scalars(refund);
    }
    public async Task<decimal> EffectiveDue(KhoanThuDangKy charge,CancellationToken ct)
    {
        if(charge.LoaiKhoan!="HOC_PHI")return charge.ThanhTienChot;
        var moved=await db.YeuCauThayDoi.Where(x=>x.DangKyNguonId==charge.DangKyId&&x.LoaiYeuCau=="CHUYEN"&&x.TrangThai=="DA_THUC_HIEN").ToListAsync(ct);
        return Math.Max(0,charge.ThanhTienChot-moved.Sum(x=>x.GiaTriChuyenThucTe??0));
    }
    public async Task<object> Debt(long id,CancellationToken ct)
    {
        var e=await db.DangKy.AsNoTracking().SingleOrDefaultAsync(x=>x.Id==id,ct)??throw new ApiError(404,"Không có đăng ký.");
        await enrollments.CheckRead(e,ct);
        var charges=await db.KhoanThuDangKy.AsNoTracking().Where(x=>x.DangKyId==id).ToListAsync(ct);
        var result=new List<object>();foreach(var c in charges){var balance=await Balance(c,ct);result.Add(new {c.Id,c.LoaiKhoan,c.ThanhTienChot,DaThanhToan=balance,PhaiThuHieuLuc=await EffectiveDue(c,ct),ConThieu=Math.Max(0,await EffectiveDue(c,ct)-balance)});}
        return new {DangKyId=id,e.TrangThai,KhoanThu=result};
    }
}
