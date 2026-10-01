using System.Data;
using Microsoft.EntityFrameworkCore;
using MyThuat.Api.Contracts;
using MyThuat.Api.Data;
using MyThuat.Api.Models;
using MyThuat.Api.Security;
namespace MyThuat.Api.Services;
public sealed class EnrollmentService(MyThuatDbContext db,AccessService access,PolicyService policies,AuditService audit)
{
    public async Task<LopHoc> LockClass(long id,CancellationToken ct)=>
        (db.Database.IsSqlServer()?await db.LopHoc.FromSqlInterpolated($"SELECT * FROM dbo.LopHoc WITH (UPDLOCK,HOLDLOCK) WHERE Id={id}").SingleOrDefaultAsync(ct)
            :await db.LopHoc.SingleOrDefaultAsync(x=>x.Id==id,ct))??throw new ApiError(404,"Không tìm thấy lớp.");
    public async Task Expire(CancellationToken ct)
    {
        var now=BusinessClock.Now;
        var expired=await db.DangKy.Where(x=>(x.TrangThai=="CHO_THANH_TOAN"||x.TrangThai=="CHO_CHUYEN")&&x.HanGiuCho<=now).ToListAsync(ct);
        var ids=expired.Select(x=>x.Id).ToList();
        foreach(var x in expired){x.TrangThai="HET_HAN";x.HieuLucDen=now;}
        foreach(var x in await db.ApDungUuDai.Where(x=>ids.Contains(x.DangKyId)&&x.TrangThaiLuot=="GIU_LUOT").ToListAsync(ct))x.TrangThaiLuot="GIAI_PHONG";
        foreach(var x in await db.DiemDanh.Where(x=>ids.Contains(x.DangKyId)&&x.TrangThai=="CHO_THAM_GIA").ToListAsync(ct))x.TrangThai="HUY_BO";
        foreach(var request in await db.YeuCauThayDoi.Where(x=>x.TrangThai=="DA_DUYET"&&x.LoaiYeuCau=="CHUYEN"&&x.HanThucHien<=now).ToListAsync(ct))
            request.TrangThai="HET_HAN";
        await db.SaveChangesAsync(ct);
    }
    public async Task EnsureCapacity(LopHoc cls,long studentId,CancellationToken ct,long? ignoreRegistration=null)
    {
        var now=BusinessClock.Now;
        ApiError.Require(await db.DangKy.CountAsync(x=>x.LopHocId==cls.Id&&(x.TrangThai=="DA_XAC_NHAN"||(x.TrangThai=="CHO_THANH_TOAN"||x.TrangThai=="CHO_CHUYEN")&&x.HanGiuCho>now),ct)<cls.SiSoToiDa,"Lớp đã hết chỗ.",409);
        ApiError.Require(!await db.DangKy.AnyAsync(x=>x.LopHocId==cls.Id&&x.HoSoTheoHoc!.HocVienId==studentId
            &&(x.TrangThai=="DA_XAC_NHAN"||(x.TrangThai=="CHO_THANH_TOAN"||x.TrangThai=="CHO_CHUYEN")&&x.HanGiuCho>now),ct),"Học viên đã có chỗ trong lớp.",409);
        var slots=await db.BuoiHoc.Where(x=>x.LopHocId==cls.Id&&x.TrangThai=="DA_XEP_LICH").ToListAsync(ct);
        foreach(var slot in slots)
            ApiError.Require(!await db.DiemDanh.AnyAsync(x=>x.DangKyId!=ignoreRegistration&&x.DangKy!.HoSoTheoHoc!.HocVienId==studentId&&x.TrangThai=="CHO_THAM_GIA"
                &&x.BuoiHoc!.TrangThai!="DA_HUY"&&x.BuoiHoc.BatDau<slot.KetThuc&&x.BuoiHoc.KetThuc>slot.BatDau,ct),"Học viên trùng lịch học.",409);
    }
    public async Task<object> Create(EnrollRequest r,CancellationToken ct)
    {
        await access.Student(r.HocVienId,true,ct);
        ApiError.Require(access.User.Has(Permission.HoSo)||access.User.MemberId()==r.NguoiDangKyId,"Người đăng ký không đúng tài khoản.",403);
        ApiError.Require(r.HoaCu is null||r.HoaCu.Count<=50,"Tối đa 50 dòng họa cụ.");
        var actor=access.User.AccountId();var key=RequestKeys.Key(actor,r.IdempotencyKey);var fingerprint=RequestKeys.Fingerprint(r);
        await using var tx=await db.Database.BeginTransactionAsync(IsolationLevel.Serializable,ct);
        var previous=await db.DangKy.SingleOrDefaultAsync(x=>x.IdempotencyKey==key,ct);
        if(previous is not null){ApiError.Require(previous.RequestFingerprint==fingerprint,"IdempotencyKey đã dùng cho dữ liệu khác.",409);return ManagementService.Scalars(previous);}
        var cls=await LockClass(r.LopHocId,ct);await Expire(ct);
        var now=BusinessClock.Now;
        ApiError.Require(cls.TrangThai=="DANG_TUYEN_SINH"&&cls.MoDangKyLuc<=now&&cls.DongDangKyLuc>now,"Lớp không trong thời gian tuyển sinh.",409);
        var course=await db.KhoaHoc.SingleAsync(x=>x.Id==cls.KhoaHocId,ct);
        ApiError.Require(course.TrangThai=="HOAT_DONG","Khóa không hoạt động.");
        var student=await db.HocVien.SingleOrDefaultAsync(x=>x.Id==r.HocVienId&&x.TrangThai=="HOAT_DONG",ct)??throw new ApiError(400,"Học viên chưa được xác minh.");
        var member=await db.ThanhVien.SingleOrDefaultAsync(x=>x.Id==r.NguoiDangKyId&&x.TrangThai=="HOAT_DONG",ct)??throw new ApiError(400,"Thành viên chưa được xác minh.");
        var link=await db.DaiDienHocVien.SingleOrDefaultAsync(x=>x.HocVienId==student.Id&&x.ThanhVienId==member.Id
            &&x.XacNhanLuc!=null&&x.HieuLucTu<=now&&(x.HieuLucDen==null||x.HieuLucDen>now),ct);
        ApiError.Require(link is not null&&link.QuyenDaiDien is "DAY_DU" or "DANG_KY","Thiếu quyền đại diện đã xác minh.");
        var birthday=student.NgaySinh;var age=cls.NgayKhaiGiangDuKien.Year-birthday.Year;
        if(birthday.AddYears(age)>cls.NgayKhaiGiangDuKien)age--;
        ApiError.Require(course.TuoiToiThieu is null||age>=course.TuoiToiThieu,"Học viên chưa đủ tuổi tối thiểu. Không áp dụng tuổi tối đa.");
        ApiError.Require(age>=18||link!.QuanHe!="TU_BAN_THAN","Người dưới 18 tuổi cần người đại diện.");
        ApiError.Require(student.NgayDanhGiaDauVao.HasValue&&student.GiaoVienDanhGia.HasValue
            &&student.CapDoDeXuat==course.CapDo,"Cần đánh giá đầu vào và cấp độ đề xuất phù hợp trước khi đăng ký.");
        await EnsureCapacity(cls,student.Id,ct);
        var rule=await policies.Current(ct);var settings=PolicySettings.Parse(rule.NoiDungJson);
        var first=await db.BuoiHoc.Where(x=>x.LopHocId==cls.Id&&x.TrangThai!="DA_HUY").MinAsync(x=>(DateTime?)x.BatDau,ct);
        ApiError.Require(first.HasValue,"Lớp chưa có lịch.");
        var limit=new[]{now.AddHours(settings.HanGiuChoGio),cls.DongDangKyLuc,first!.Value.AddHours(-settings.DongDangKyTruocGio)}.Min();
        ApiError.Require(limit>now,"Không còn thời gian giữ chỗ.",409);
        var profile=new HoSoTheoHoc {HocVienId=student.Id,KhoaHocId=course.Id,QuyDinhId=rule.Id,NgayBatDau=cls.NgayKhaiGiangDuKien,TrangThai="DANG_THEO_HOC"};
        db.HoSoTheoHoc.Add(profile);await db.SaveChangesAsync(ct);
        var enrollment=new DangKy {MaDangKy="DK"+Guid.NewGuid().ToString("N")[..20],HoSoTheoHocId=profile.Id,LopHocId=cls.Id,
            NguoiDangKyId=member.Id,LapLuc=now,HanGiuCho=limit,HocPhiGocChot=cls.HocPhiApDung,TrangThai="CHO_THANH_TOAN",
            IdempotencyKey=key,RequestFingerprint=fingerprint};
        db.DangKy.Add(enrollment);await db.SaveChangesAsync(ct);
        var charges=new List<KhoanThuDangKy>{new(){DangKyId=enrollment.Id,LoaiKhoan="HOC_PHI",MoTaChot=course.TenKhoa,
            SoLuong=1,DonGiaChot=cls.HocPhiApDung,ThanhTienChot=cls.HocPhiApDung}};
        foreach(var purchase in r.HoaCu??[])
        {
            ApiError.Require(purchase.SoLuong>0&&purchase.SoLuong<=1000000&&decimal.Round(purchase.SoLuong,3)==purchase.SoLuong,"Số lượng họa cụ không hợp lệ.");
            var material=await db.HoaCu.SingleOrDefaultAsync(x=>x.Id==purchase.HoaCuId&&x.NhomHoaCu=="BAN_RIENG"&&x.TrangThai=="HOAT_DONG",ct)
                ??throw new ApiError(400,"Họa cụ không bán riêng hoặc không hoạt động.");
            ApiError.Require(!charges.Any(x=>x.HoaCuId==material.Id),"Không lặp dòng họa cụ.");
            charges.Add(new(){DangKyId=enrollment.Id,LoaiKhoan="HOA_CU",HoaCuId=material.Id,MoTaChot=material.TenHoaCu,
                SoLuong=purchase.SoLuong,DonGiaChot=material.GiaBanThamKhao,ThanhTienChot=decimal.Round(purchase.SoLuong*material.GiaBanThamKhao,0,MidpointRounding.AwayFromZero)});
        }
        await Discount(enrollment,member,charges,"HOC_PHI",r.MaGiamHocPhi,ct);
        await Discount(enrollment,member,charges,"HOA_CU",r.MaGiamHoaCu,ct);
        if(charges.Sum(x=>x.ThanhTienChot)==0)
        {
            enrollment.TrangThai="DA_XAC_NHAN";enrollment.HieuLucTu=now;
            foreach(var promo in db.ChangeTracker.Entries<ApDungUuDai>().Where(x=>x.Entity.DangKyId==enrollment.Id))
                promo.Entity.TrangThaiLuot="DA_DUNG";
        }
        db.KhoanThuDangKy.AddRange(charges);
        foreach(var b in await db.BuoiHoc.Where(x=>x.LopHocId==cls.Id&&x.TrangThai=="DA_XEP_LICH").ToListAsync(ct))
            db.DiemDanh.Add(new(){DangKyId=enrollment.Id,BuoiHocId=b.Id,NoiDungDuocTinhId=b.NoiDungBuoiId,LoaiThamGia="HOC_CHINH",TrangThai="CHO_THAM_GIA"});
        audit.Add("DangKy",enrollment.Id,"GIU_CHO",new {cls.Id,HanGiuCho=limit});
        await db.SaveChangesAsync(ct);await tx.CommitAsync(ct);return ManagementService.Scalars(enrollment);
    }
    private async Task Discount(DangKy enrollment,ThanhVien member,List<KhoanThuDangKy> charges,string group,string? code,CancellationToken ct)
    {
        if(string.IsNullOrWhiteSpace(code))return;
        var now=BusinessClock.Now;
        var promo=await db.KhuyenMai.SingleOrDefaultAsync(x=>x.MaKhuyenMai==code&&x.TrangThai=="HOAT_DONG"&&x.BatDau<=now&&x.KetThuc>now,ct)
            ??throw new ApiError(400,"Mã ưu đãi không hiệu lực.");
        var profile=await db.HoSoTheoHoc.SingleAsync(x=>x.Id==enrollment.HoSoTheoHocId,ct);
        ApiError.Require(promo.NhomKhoanThu==group&&(promo.KhoaApDungId==null||promo.KhoaApDungId==profile.KhoaHocId),"Mã không áp dụng cho khoản thu/khóa này.");
        ApiError.Require(promo.SoNgayTuXacNhanTV is null||member.XacNhanThanhVienLuc.HasValue
            &&now<=member.XacNhanThanhVienLuc.Value.AddDays(promo.SoNgayTuXacNhanTV.Value),"Thành viên ngoài thời hạn ưu đãi.");
        ApiError.Require(await db.ApDungUuDai.CountAsync(x=>x.KhuyenMaiId==promo.Id&&(x.TrangThaiLuot=="DA_DUNG"||x.TrangThaiLuot=="GIU_LUOT"&&x.HanGiuLuot>now),ct)<promo.TongLuot,"Ưu đãi đã hết lượt.",409);
        var lines=charges.Where(x=>x.LoaiKhoan==group).ToList();var subtotal=lines.Sum(x=>x.ThanhTienChot);
        ApiError.Require(subtotal>0,"Không có khoản thu phù hợp để áp dụng ưu đãi.");
        var discount=promo.LoaiGiam=="PHAN_TRAM"?decimal.Round(subtotal*promo.GiaTriGiam/100,0,MidpointRounding.AwayFromZero):promo.GiaTriGiam;
        discount=Math.Min(subtotal,Math.Min(discount,promo.TranGiam??decimal.MaxValue));
        var remaining=discount;
        foreach(var line in lines){line.TienGiamChot=Math.Min(line.ThanhTienChot,remaining);line.ThanhTienChot-=line.TienGiamChot;remaining-=line.TienGiamChot;}
        db.ApDungUuDai.Add(new(){DangKyId=enrollment.Id,KhuyenMaiId=promo.Id,NhomKhoanThu=group,GiaTriGiamChot=discount,HanGiuLuot=enrollment.HanGiuCho,TrangThaiLuot="GIU_LUOT"});
    }
    public async Task CheckRead(DangKy enrollment,CancellationToken ct)
    {
        if(access.User.Has(Permission.HoSo)||access.User.Has(Permission.ThuTien))return;
        var profile=await db.HoSoTheoHoc.AsNoTracking().SingleAsync(x=>x.Id==enrollment.HoSoTheoHocId,ct);
        await access.Student(profile.HocVienId,false,ct);
    }
    public async Task<object> Detail(long id,CancellationToken ct)
    {
        var e=await db.DangKy.AsNoTracking().SingleOrDefaultAsync(x=>x.Id==id,ct)??throw new ApiError(404,"Không có đăng ký.");
        await CheckRead(e,ct);
        var charges=await db.KhoanThuDangKy.AsNoTracking().Where(x=>x.DangKyId==id).OrderBy(x=>x.Id).ToListAsync(ct);
        return new {DangKy=ManagementService.Scalars(e),KhoanThu=charges.Select(ManagementService.Scalars)};
    }
}
