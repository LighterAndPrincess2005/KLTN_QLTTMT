using System.Data;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using Microsoft.EntityFrameworkCore;
using MyThuat.Api.Data;
using MyThuat.Api.Models;
namespace MyThuat.Api.Services;
public sealed record SePayEvent(long Id,string AccountNumber,string TransferType,decimal TransferAmount,string? Content,string? Code,string? ReferenceCode,string? TransactionDate);
public sealed class OnlinePaymentService(MyThuatDbContext db,AccessService access,EnrollmentService enrollments,FinanceService finance,IConfiguration config,AuditService audit)
{
    bool Enabled=>config.GetValue<bool>("SePay:Enabled")&&!string.IsNullOrWhiteSpace(config["SePay:AccountNumber"])&&!string.IsNullOrWhiteSpace(config["SePay:BankCode"])&&!string.IsNullOrWhiteSpace(config["SePay:WebhookKey"]);
    public void Authenticate(string header){
        ApiError.Require(Enabled,"Thanh toán tự động chưa được cấu hình.",503);
        var token=header.StartsWith("Apikey ",StringComparison.OrdinalIgnoreCase)?header[7..].Trim():"";
        ApiError.Require(token.Length>0&&CryptographicOperations.FixedTimeEquals(SHA256.HashData(Encoding.UTF8.GetBytes(token)),SHA256.HashData(Encoding.UTF8.GetBytes(config["SePay:WebhookKey"]!))),"Webhook không hợp lệ.",401);
    }
    async Task<DangKy> Own(long id,CancellationToken ct){var e=await db.DangKy.Include(x=>x.HoSoTheoHoc).Include(x=>x.LopHoc).SingleOrDefaultAsync(x=>x.Id==id,ct)??throw new ApiError(404,"Không có phiếu đăng ký.");await access.Student(e.HoSoTheoHoc!.HocVienId,true,ct);return e;}
    async Task<decimal> Due(long id,CancellationToken ct){var rows=await db.KhoanThuDangKy.Where(x=>x.DangKyId==id).ToListAsync(ct);decimal due=0;foreach(var row in rows)due+=Math.Max(0,row.ThanhTienChot-await finance.Balance(row,ct));return due;}
    public async Task<object> Checkout(long id,CancellationToken ct){
        var e=await Own(id,ct);await using var tx=await db.Database.BeginTransactionAsync(IsolationLevel.Serializable,ct);await enrollments.LockClass(e.LopHocId,ct);await enrollments.Expire(ct);
        var due=await Due(id,ct);var free=await db.KhoanThuDangKy.Where(x=>x.DangKyId==id).SumAsync(x=>x.ThanhTienChot,ct)==0;
        ApiError.Require(e.TrangThai is "CHO_THANH_TOAN" or "DA_XAC_NHAN","Phiếu đã hết hiệu lực; liên hệ trung tâm nếu đã chuyển tiền.",409);
        if(due==0&&e.TrangThai=="CHO_THANH_TOAN"){await finance.Confirm(e,ct);await db.SaveChangesAsync(ct);}
        var intent=await db.Set<PhienThanhToan>().SingleOrDefaultAsync(x=>x.DangKyId==id,ct);
        if(due>0&&Enabled&&intent==null){intent=new(){DangKyId=id,MaThanhToan="BUI"+Convert.ToHexString(RandomNumberGenerator.GetBytes(6))};db.Add(intent);audit.Add("DangKy",id,"TAO_PHIEN_THANH_TOAN",new{SoTien=due});await db.SaveChangesAsync(ct);}
        await tx.CommitAsync(ct);return await View(e,due,free,intent,ct);
    }
    public async Task<object> Status(long id,CancellationToken ct){var e=await Own(id,ct);return await View(e,await Due(id,ct),await db.KhoanThuDangKy.Where(x=>x.DangKyId==id).SumAsync(x=>x.ThanhTienChot,ct)==0,await db.Set<PhienThanhToan>().AsNoTracking().SingleOrDefaultAsync(x=>x.DangKyId==id,ct),ct);}
    async Task<object> View(DangKy e,decimal due,bool free,PhienThanhToan? intent,CancellationToken ct){
        var expired=e.TrangThai=="HET_HAN"||e.TrangThai=="CHO_THANH_TOAN"&&e.HanGiuCho<=BusinessClock.Now;
        var payable=!expired&&e.TrangThai=="CHO_THANH_TOAN"&&due>0&&Enabled&&intent!=null;
        var code=payable?intent!.MaThanhToan:null;
        var qr=payable?$"https://vietqr.app/img?acc={Uri.EscapeDataString(config["SePay:AccountNumber"]!)}&bank={Uri.EscapeDataString(config["SePay:BankCode"]!)}&amount={due:0}&des={Uri.EscapeDataString(code!)}":null;
        return new{DangKyId=e.Id,TenLop=e.LopHoc!.TenLop,TenHocVien=await db.HocVien.Where(x=>x.Id==e.HoSoTheoHoc!.HocVienId).Select(x=>x.HoTen).SingleAsync(ct),ConPhaiTra=due,MienPhi=free,DaXacNhan=e.TrangThai=="DA_XAC_NHAN",HetHan=expired,HanGiuCho=e.HanGiuCho,SanSang=payable,MaThanhToan=code,QrUrl=qr,NganHang=payable?config["SePay:BankCode"]:null,SoTaiKhoan=payable?config["SePay:AccountNumber"]:null,ChuTaiKhoan=payable?config["SePay:AccountName"]:null,ThongBao=free?"Lớp miễn phí không phát sinh giao dịch ngân hàng.":!Enabled?"Trung tâm chưa cấu hình thanh toán tự động. Bạn có thể liên hệ trung tâm để thanh toán tại quầy.":"Quét QR và giữ nguyên số tiền, nội dung. Chỉ xác nhận khi hệ thống nhận được thông báo giao dịch hợp lệ."};
    }
    public async Task<object> Receive(SePayEvent r,CancellationToken ct){
        ApiError.Require(r.Id>0&&r.TransferAmount>0&&r.TransferAmount==decimal.Truncate(r.TransferAmount)&&r.TransferAmount<=1000000000000m,"Giao dịch không hợp lệ.");
        ApiError.Require(r.TransferType is "in" or "out"&&(r.Code?.Length??0)<=100,"Loại giao dịch hoặc mã thanh toán không hợp lệ.");
        ApiError.Require(r.AccountNumber==config["SePay:AccountNumber"],"Tài khoản nhận không hợp lệ.",400);
        var content=(r.Content??"").Trim();var reference=(r.ReferenceCode??"").Trim();ApiError.Require(content.Length<=2000&&reference.Length<=150,"Nội dung giao dịch vượt giới hạn.");
        var fingerprint=Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes($"{r.Id}|{r.AccountNumber}|{r.TransferType}|{r.TransferAmount:0}|{content}|{r.Code}|{reference}")));
        await using var tx=await db.Database.BeginTransactionAsync(IsolationLevel.Serializable,ct);
        var prior=await db.Set<SuKienSePay>().SingleOrDefaultAsync(x=>x.MaGiaoDichSePay==r.Id,ct);
        if(prior!=null){ApiError.Require(prior.DauVanTay==fingerprint,"Mã giao dịch đã có dữ liệu khác.",409);return new{success=true};}
        var record=new SuKienSePay{MaGiaoDichSePay=r.Id,SoTien=r.TransferAmount,NoiDung=content,MaThamChieu=reference,DauVanTay=fingerprint};db.Add(record);
        if(r.TransferType!="in"){record.TrangThai="BO_QUA";await db.SaveChangesAsync(ct);await tx.CommitAsync(ct);return new{success=true};}
        var match=Regex.Match((r.Code??"")+" "+content,@"\bBUI[A-F0-9]{12}\b",RegexOptions.IgnoreCase);
        var intent=match.Success?await db.Set<PhienThanhToan>().SingleOrDefaultAsync(x=>x.MaThanhToan==match.Value.ToUpper(),ct):null;
        if(intent!=null){
            var e=await db.DangKy.SingleAsync(x=>x.Id==intent.DangKyId,ct);await enrollments.LockClass(e.LopHocId,ct);await enrollments.Expire(ct);record.DangKyId=e.Id;
            var charges=await db.KhoanThuDangKy.Where(x=>x.DangKyId==e.Id).OrderBy(x=>x.Id).ToListAsync(ct);
            var due=await Due(e.Id,ct);var allocate=e.TrangThai=="CHO_THANH_TOAN"&&e.HanGiuCho>BusinessClock.Now&&r.TransferAmount<=due;
            var left=r.TransferAmount;
            foreach(var charge in charges){if(!allocate||left<=0)break;var amount=Math.Min(left,Math.Max(0,charge.ThanhTienChot-await finance.Balance(charge,ct)));if(amount<=0)continue;
                db.GiaoDichTien.Add(new(){DangKyId=e.Id,KhoanThuId=charge.Id,LoaiGiaoDich="THU",PhuongThuc="CHUYEN_KHOAN",SoTien=amount,MaChungTu="SP"+r.Id,MaThamChieu=reference,ThoiDiem=BusinessClock.Now,TrangThai="DA_XAC_NHAN",IdempotencyKey=$"SEPAY:{r.Id}:{charge.Id}",RequestFingerprint=fingerprint});left-=amount;
            }
            if(!allocate)db.GiaoDichTien.Add(new(){DangKyId=e.Id,LoaiGiaoDich="THU",PhuongThuc="CHUYEN_KHOAN",SoTien=r.TransferAmount,MaChungTu="SP"+r.Id,MaThamChieu=reference,ThoiDiem=BusinessClock.Now,TrangThai="CHO_DOI_CHIEU",IdempotencyKey=$"SEPAY:{r.Id}",RequestFingerprint=fingerprint});
            record.TrangThai=allocate?"DA_GHI_NHAN":"CHO_DOI_CHIEU";await db.SaveChangesAsync(ct);if(allocate){await finance.Confirm(e,ct);await db.SaveChangesAsync(ct);}
        }
        await db.SaveChangesAsync(ct);await tx.CommitAsync(ct);return new{success=true};
    }
}
