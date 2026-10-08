using System.Data;
using Microsoft.EntityFrameworkCore;
using MyThuat.Api.Contracts;
using MyThuat.Api.Data;
using MyThuat.Api.Models;
using MyThuat.Api.Security;
namespace MyThuat.Api.Services;
public sealed class LearningService(MyThuatDbContext db,AccessService access,PolicyService policies,AuditService audit,FinanceService finance)
{
    public async Task<object> Roster(long session,CancellationToken ct)
    {
        await access.TeacherSession(session,ct);
        return await db.DiemDanh.AsNoTracking().Where(x=>x.BuoiHocId==session&&x.TrangThai!="HUY_BO"&&x.DangKy!.TrangThai=="DA_XAC_NHAN")
            .OrderBy(x=>x.DangKy!.HoSoTheoHoc!.HocVien!.HoTen)
            .Select(x=>new {x.Id,x.DangKyId,HocVienId=x.DangKy!.HoSoTheoHoc!.HocVienId,HoTen=x.DangKy.HoSoTheoHoc.HocVien!.HoTen,NgaySinh=x.DangKy.HoSoTheoHoc.HocVien.NgaySinh,
                LuuY=x.DangKy.HoSoTheoHoc.HocVien.LuuYHoTroHocTap,x.LoaiThamGia,x.TrangThai,x.BaoNghiLuc,x.PhutThamDu,x.NhanXet,x.DiemSanPham,x.SanPhamUrl,x.RowVersion}).ToListAsync(ct);
    }
    public async Task<object> Attend(long id,AttendRequest r,CancellationToken ct)
    {
        await using var tx=await db.Database.BeginTransactionAsync(IsolationLevel.Serializable,ct);
        var record=await db.DiemDanh.Include(x=>x.BuoiHoc).Include(x=>x.DangKy).SingleOrDefaultAsync(x=>x.Id==id,ct)??throw new ApiError(404,"Không có điểm danh.");
        await access.TeacherSession(record.BuoiHocId,ct);
        ApiError.Require(Convert.ToBase64String(record.RowVersion)==r.RowVersion,"Điểm danh đã thay đổi, hãy tải lại.",409);
        var now=BusinessClock.Now;var b=record.BuoiHoc!;
        ApiError.Require(b.TrangThai is "DA_XEP_LICH" or "DA_TO_CHUC"&&b.BatDau<=now&&record.DangKy!.TrangThai=="DA_XAC_NHAN","Buổi chưa bắt đầu, đã hủy hoặc học viên chưa xác nhận.");
        ApiError.Require(access.User.Has(Permission.DaoTao)||DateOnly.FromDateTime(b.BatDau)==BusinessClock.Today,"Giáo viên ghi trong ngày; chỉnh muộn cần học vụ.",403);
        ApiError.Require(r.TrangThai is "CO_MAT" or "VANG"&&r.DiemSanPham is null or >=0 and <=10,"Trạng thái hoặc điểm không hợp lệ.");
        ApiError.Require(r.PhutThamDu is null||r.PhutThamDu>=0&&r.PhutThamDu<=(b.KetThuc-b.BatDau).TotalMinutes,"Số phút tham dự không hợp lệ.");
        ApiError.Require(r.TrangThai!="CO_MAT"||r.PhutThamDu>0,"Có mặt phải ghi số phút tham dự.");
        ApiError.Require(record.TrangThai!="HUY_BO","Điểm danh này đã hủy.");
        if(!string.IsNullOrWhiteSpace(r.SanPhamUrl))ApiError.Require(Uri.TryCreate(r.SanPhamUrl,UriKind.Absolute,out var uri)&&uri.Scheme is "http" or "https","Liên kết sản phẩm cần dùng HTTP/HTTPS.");
        record.TrangThai=r.TrangThai;record.PhutThamDu=r.PhutThamDu;record.NhanXet=r.NhanXet;
        record.DiemSanPham=r.DiemSanPham;record.SanPhamUrl=r.SanPhamUrl;record.NguoiGhi=access.User.AccountId();record.GhiLuc=now;
        var makeup=await db.HocBu.SingleOrDefaultAsync(x=>x.DiemDanhBuId==id,ct);
        if(makeup is not null)makeup.TrangThai=r.TrangThai=="CO_MAT"?"DA_HOAN_TAT":"VANG";
        audit.Add("DiemDanh",id,"DIEM_DANH",new {r.TrangThai});await db.SaveChangesAsync(ct);await tx.CommitAsync(ct);return ManagementService.Scalars(record);
    }
    public async Task<object> Absence(long id,AbsenceRequest r,CancellationToken ct)
    {
        var record=await db.DiemDanh.Include(x=>x.DangKy).ThenInclude(x=>x!.HoSoTheoHoc).Include(x=>x.BuoiHoc).SingleOrDefaultAsync(x=>x.Id==id,ct)??throw new ApiError(404,"Không có buổi học của học viên.");
        await access.Student(record.DangKy!.HoSoTheoHoc!.HocVienId,true,ct);
        ApiError.Require(record.TrangThai=="CHO_THAM_GIA"&&record.BuoiHoc!.BatDau>BusinessClock.Now&&record.BuoiHoc.TrangThai=="DA_XEP_LICH","Không thể báo nghỉ buổi đã bắt đầu hoặc hủy.");
        record.BaoNghiLuc=BusinessClock.Now;record.NhanXet=r.LyDo;
        audit.Add("DiemDanh",id,"BAO_NGHI");await db.SaveChangesAsync(ct);return ManagementService.Scalars(record);
    }
    public async Task<object> Makeup(MakeupRequest r,CancellationToken ct)
    {
        access.Need(Permission.DaoTao);
        await using var tx=await db.Database.BeginTransactionAsync(IsolationLevel.Serializable,ct);
        var absent=await db.DiemDanh.Include(x=>x.DangKy).ThenInclude(x=>x!.HoSoTheoHoc).Include(x=>x.BuoiHoc)
            .SingleOrDefaultAsync(x=>x.Id==r.DiemDanhVangId,ct)??throw new ApiError(404,"Không có lượt vắng.");
        var profile=absent.DangKy!.HoSoTheoHoc!;var settings=await policies.ForProfile(profile.Id,ct);
        ApiError.Require(absent.TrangThai=="VANG"&&absent.LoaiThamGia=="HOC_CHINH"&&absent.BaoNghiLuc<=absent.BuoiHoc!.BatDau.AddHours(-settings.BaoNghiTruocGio),"Cần vắng buổi chính và báo nghỉ đủ thời hạn.");
        var existing=await db.HocBu.SingleOrDefaultAsync(x=>x.DiemDanhVangId==absent.Id,ct);
        ApiError.Require(existing is null||existing.TrangThai=="DA_HUY","Buổi này đã có quyền bù được sử dụng/đặt.",409);
        ApiError.Require(await db.HocBu.CountAsync(x=>x.DiemDanhVang!.DangKy!.HoSoTheoHocId==profile.Id&&x.TrangThai!="DA_HUY",ct)<settings.SoLuotHocBu,"Đã hết lượt học bù của cả khóa.",409);
        var target=await db.BuoiHoc.Include(x=>x.LopHoc).Include(x=>x.PhongHoc).SingleOrDefaultAsync(x=>x.Id==r.BuoiHocId,ct)??throw new ApiError(404,"Không có lịch bù.");
        var originalClass=await db.LopHoc.SingleAsync(x=>x.Id==absent.BuoiHoc!.LopHocId,ct);
        var deadline=(originalClass.NgayKetThucDuKien??DateOnly.FromDateTime(absent.BuoiHoc!.KetThuc)).AddDays(settings.HanHocBuNgay).ToDateTime(TimeOnly.MaxValue);
        ApiError.Require(target.TrangThai=="DA_XEP_LICH"&&target.BatDau>BusinessClock.Now&&target.BatDau<=deadline
            &&target.LopHoc!.KhoaHocId==profile.KhoaHocId&&target.NoiDungBuoiId==absent.NoiDungDuocTinhId,"Lịch bù không cùng nội dung/phiên bản, quá hạn hoặc đã bắt đầu.");
        ApiError.Require(!await db.DiemDanh.AnyAsync(x=>x.DangKy!.HoSoTheoHocId==profile.Id&&x.NoiDungDuocTinhId==target.NoiDungBuoiId
            &&x.TrangThai=="CO_MAT",ct),"Nội dung này đã hoàn thành.");
        ApiError.Require(await db.DiemDanh.CountAsync(x=>x.BuoiHocId==target.Id&&x.TrangThai!="HUY_BO"&&x.DangKy!.TrangThai=="DA_XAC_NHAN",ct)
            <Math.Min(target.LopHoc!.SiSoToiDa,target.PhongHoc!.SucChua),"Buổi bù đã hết chỗ.",409);
        ApiError.Require(!await db.DiemDanh.AnyAsync(x=>x.DangKy!.HoSoTheoHoc!.HocVienId==profile.HocVienId&&x.TrangThai=="CHO_THAM_GIA"
            &&x.BuoiHoc!.BatDau<target.KetThuc&&x.BuoiHoc.KetThuc>target.BatDau,ct),"Học viên trùng lịch.",409);
        var active=await db.DangKy.SingleOrDefaultAsync(x=>x.HoSoTheoHocId==profile.Id&&x.TrangThai=="DA_XAC_NHAN",ct)??throw new ApiError(409,"Không có đăng ký hiệu lực để học bù.");
        var attendance=new DiemDanh {DangKyId=active.Id,BuoiHocId=target.Id,NoiDungDuocTinhId=target.NoiDungBuoiId,LoaiThamGia="HOC_BU",TrangThai="CHO_THAM_GIA"};
        db.DiemDanh.Add(attendance);await db.SaveChangesAsync(ct);
        var m=existing??new HocBu {DiemDanhVangId=absent.Id};
        m.DiemDanhBuId=attendance.Id;m.HanHoanTat=deadline;m.NguoiDuyet=access.User.AccountId();m.TrangThai="DA_DAT";
        if(existing is null)db.HocBu.Add(m);
        var marker=$"[HOC_BU:{absent.Id}]";
        foreach(var request in await db.PhanHoi.Where(x=>x.DangKy!.HoSoTheoHocId==profile.Id&&x.NoiDung.StartsWith(marker)&&x.TrangThai=="CHO_XU_LY").ToListAsync(ct))
        {
            request.TrangThai="DA_TRA_LOI";request.NguoiXuLy=access.User.AccountId();request.TraLoiLuc=BusinessClock.Now;
            request.TraLoi=$"Trung tâm đã xếp học bù lúc {target.BatDau:HH:mm dd/MM/yyyy}, lớp {target.LopHoc!.TenLop}.";
        }
        await db.SaveChangesAsync(ct);audit.Add("HocBu",m.Id,"DAT_HOC_BU");await db.SaveChangesAsync(ct);await tx.CommitAsync(ct);return ManagementService.Scalars(m);
    }
    public async Task<object> Suggest(StudentLearningRequest r,CancellationToken ct)
    {
        var member=access.User.MemberId();ApiError.Require(member.HasValue,"Tài khoản cần liên kết thành viên.",403);
        ApiError.Require(r.LoaiYeuCau is "HOC_BU" or "DOI_LICH","Loại đề nghị không hợp lệ.");
        await using var tx=await db.Database.BeginTransactionAsync(IsolationLevel.Serializable,ct);
        var attendance=await db.DiemDanh.Include(x=>x.DangKy).ThenInclude(x=>x!.HoSoTheoHoc).Include(x=>x.BuoiHoc)
            .SingleOrDefaultAsync(x=>x.Id==r.DiemDanhId,ct)??throw new ApiError(404,"Không có buổi học của học viên.");
        var profile=attendance.DangKy!.HoSoTheoHoc!;await access.Student(profile.HocVienId,true,ct);
        var active=await db.DangKy.SingleOrDefaultAsync(x=>x.HoSoTheoHocId==profile.Id&&x.TrangThai=="DA_XAC_NHAN",ct)
            ??throw new ApiError(409,"Cần đăng ký đã xác nhận còn hiệu lực để gửi đề nghị học tập.");
        var settings=await policies.ForProfile(profile.Id,ct);
        if(r.LoaiYeuCau=="HOC_BU")
        {
            ApiError.Require(attendance.TrangThai=="VANG"&&attendance.LoaiThamGia=="HOC_CHINH"
                &&attendance.BaoNghiLuc<=attendance.BuoiHoc!.BatDau.AddHours(-settings.BaoNghiTruocGio),"Cần vắng buổi chính và báo nghỉ đủ thời hạn để đề nghị học bù.");
            ApiError.Require(!await db.HocBu.AnyAsync(x=>x.DiemDanhVangId==attendance.Id&&x.TrangThai!="DA_HUY",ct),"Buổi này đã có lịch/quyền học bù.",409);
            ApiError.Require(await db.HocBu.CountAsync(x=>x.DiemDanhVang!.DangKy!.HoSoTheoHocId==profile.Id&&x.TrangThai!="DA_HUY",ct)<settings.SoLuotHocBu,"Đã hết lượt học bù của khóa.",409);
            var originalClass=await db.LopHoc.SingleAsync(x=>x.Id==attendance.BuoiHoc!.LopHocId,ct);
            var deadline=(originalClass.NgayKetThucDuKien??DateOnly.FromDateTime(attendance.BuoiHoc!.KetThuc)).AddDays(settings.HanHocBuNgay).ToDateTime(TimeOnly.MaxValue);
            ApiError.Require(BusinessClock.Now<=deadline,"Đã quá hạn đề nghị học bù.",409);
        }
        else ApiError.Require(attendance.DangKyId==active.Id&&attendance.TrangThai=="CHO_THAM_GIA"
            &&attendance.BuoiHoc!.TrangThai=="DA_XEP_LICH"&&attendance.BuoiHoc.BatDau>BusinessClock.Now,"Chỉ đề nghị đổi lịch cho buổi sắp tới của lớp đang học.");
        var marker=$"[{r.LoaiYeuCau}:{attendance.Id}]";
        ApiError.Require(!await db.PhanHoi.AnyAsync(x=>x.DangKyId==active.Id&&x.NoiDung.StartsWith(marker)&&x.TrangThai=="CHO_XU_LY",ct),"Buổi này đã có đề nghị cùng loại đang chờ xử lý.",409);
        var label=r.LoaiYeuCau=="HOC_BU"?"Đề nghị học bù":"Đề nghị đổi lịch";
        var feedback=new PhanHoi {DangKyId=active.Id,NguoiGuiId=member!.Value,GuiLuc=BusinessClock.Now,
            HanPhanHoi=BusinessClock.AddWorkingDays(BusinessClock.Now,2,settings.NgayNghi),TrangThai="CHO_XU_LY",
            NoiDung=$"{marker}\n{label} buổi {attendance.BuoiHoc!.ThuTuTrongLop} ({attendance.BuoiHoc.BatDau:dd/MM/yyyy HH:mm}).\nLý do: {r.LyDo.Trim()}\nKhung giờ mong muốn: {r.KhungGioMongMuon?.Trim()??"Trung tâm tư vấn"}"};
        db.PhanHoi.Add(feedback);await db.SaveChangesAsync(ct);audit.Add("PhanHoi",feedback.Id,"GUI_DE_NGHI_HOC_TAP",new {r.LoaiYeuCau,DiemDanhId=attendance.Id});
        await db.SaveChangesAsync(ct);await tx.CommitAsync(ct);
        return new {feedback.Id,feedback.TrangThai,ThongBao="Đã gửi đề nghị. Trung tâm sẽ xem xét và phản hồi; lịch học chỉ đổi sau khi được xác nhận."};
    }
    public async Task CloseSession(long id,CancellationToken ct)
    {
        await access.TeacherSession(id,ct);
        var session=await db.BuoiHoc.SingleOrDefaultAsync(x=>x.Id==id,ct)??throw new ApiError(404,"Không có buổi học.");
        ApiError.Require(session.TrangThai=="DA_XEP_LICH"&&session.KetThuc<=BusinessClock.Now,"Buổi chưa kết thúc hoặc đã hủy.");
        ApiError.Require(!await db.DiemDanh.AnyAsync(x=>x.BuoiHocId==id&&x.TrangThai=="CHO_THAM_GIA"&&x.DangKy!.TrangThai=="DA_XAC_NHAN",ct),"Cần ghi điểm danh tất cả học viên đã xác nhận.");
        session.TrangThai="DA_TO_CHUC";audit.Add("BuoiHoc",id,"HOAN_THANH_BUOI");await db.SaveChangesAsync(ct);
    }
    public async Task<object> Result(ResultRequest r,CancellationToken ct)
    {
        ApiError.Require(access.User.IsInRole("QUAN_TRI"),"Quản trị duyệt kết quả hoàn thành.",403);
        await using var tx=await db.Database.BeginTransactionAsync(IsolationLevel.Serializable,ct);
        var profile=await db.HoSoTheoHoc.Include(x=>x.KhoaHoc).SingleOrDefaultAsync(x=>x.Id==r.HoSoTheoHocId,ct)??throw new ApiError(404,"Không có hồ sơ theo học.");
        ApiError.Require(profile.TrangThai=="DANG_THEO_HOC","Hồ sơ đã kết thúc hoặc hủy.");
        var settings=await policies.ForProfile(profile.Id,ct);
        var count=await db.DiemDanh.Where(x=>x.DangKy!.HoSoTheoHocId==profile.Id&&x.TrangThai=="CO_MAT"&&x.BuoiHoc!.TrangThai=="DA_TO_CHUC")
            .Select(x=>x.NoiDungDuocTinhId).Distinct().CountAsync(ct);
        var required=(int)Math.Ceiling(profile.KhoaHoc!.SoBuoi*settings.TyLeBuoiDat);
        var enrollmentIds=await db.DangKy.Where(x=>x.HoSoTheoHocId==profile.Id).Select(x=>x.Id).ToListAsync(ct);
        foreach(var charge in await db.KhoanThuDangKy.Where(x=>enrollmentIds.Contains(x.DangKyId)&&x.LoaiKhoan=="HOC_PHI").ToListAsync(ct))
            ApiError.Require(await finance.Balance(charge,ct)>=await finance.EffectiveDue(charge,ct),"Hồ sơ còn thiếu học phí.");
        ApiError.Require(count>=required&&r.DiemCuoiKhoa>=settings.DiemDat&&!string.IsNullOrWhiteSpace(r.BaiCuoiKhoaUrl),"Chưa đạt số buổi, điểm hoặc thiếu bài cuối khóa.");
        var result=await db.KetQuaKhoaHoc.SingleOrDefaultAsync(x=>x.HoSoTheoHocId==profile.Id,ct);
        if(result is null){result=new KetQuaKhoaHoc {HoSoTheoHocId=profile.Id};db.KetQuaKhoaHoc.Add(result);}
        result.SoBuoiHopLeChot=(short)count;result.DiemCuoiKhoaChot=r.DiemCuoiKhoa;result.KetLuan="HOAN_THANH";
        result.NhanXetTongKet=r.NhanXet;result.BaiCuoiKhoaUrl=r.BaiCuoiKhoaUrl;result.NguoiDuyet=access.User.AccountId();
        result.ChotLuc=BusinessClock.Now;result.SoXacNhan??="XN"+Guid.NewGuid().ToString("N")[..20];result.NgayCap=BusinessClock.Today;
        profile.TrangThai="DA_HOAN_THANH";await db.SaveChangesAsync(ct);audit.Add("KetQuaKhoaHoc",result.Id,"DUYET_HOAN_THANH");
        await db.SaveChangesAsync(ct);await tx.CommitAsync(ct);return ManagementService.Scalars(result);
    }
    public async Task<object> ProposeResult(ResultRequest r,CancellationToken ct)
    {
        access.Need(Permission.DiemDanh);
        var profile=await db.HoSoTheoHoc.SingleOrDefaultAsync(x=>x.Id==r.HoSoTheoHocId,ct)??throw new ApiError(404,"Không có hồ sơ.");
        if(!access.User.Has(Permission.DaoTao))
            ApiError.Require(await db.DangKy.AnyAsync(x=>x.HoSoTheoHocId==profile.Id&&x.TrangThai=="DA_XAC_NHAN"
                &&db.BuoiHoc.Any(b=>b.LopHocId==x.LopHocId&&db.GiangDayBuoi.Any(g=>g.BuoiHocId==b.Id&&g.GiaoVienId==access.User.TeacherId())),ct),
                "Bạn không được phân công lớp hiện tại của học viên.",403);
        var result=await db.KetQuaKhoaHoc.SingleOrDefaultAsync(x=>x.HoSoTheoHocId==profile.Id,ct);
        ApiError.Require(result is null||result.KetLuan=="DE_XUAT","Kết quả đã được duyệt.");
        if(result is null){result=new KetQuaKhoaHoc {HoSoTheoHocId=profile.Id};db.KetQuaKhoaHoc.Add(result);}
        result.SoBuoiHopLeChot=(short)await db.DiemDanh.Where(x=>x.DangKy!.HoSoTheoHocId==profile.Id&&x.TrangThai=="CO_MAT"&&x.BuoiHoc!.TrangThai=="DA_TO_CHUC")
            .Select(x=>x.NoiDungDuocTinhId).Distinct().CountAsync(ct);
        result.DiemCuoiKhoaChot=r.DiemCuoiKhoa;result.NhanXetTongKet=r.NhanXet;result.BaiCuoiKhoaUrl=r.BaiCuoiKhoaUrl;
        result.KetLuan="DE_XUAT";result.NguoiDuyet=access.User.AccountId();result.ChotLuc=BusinessClock.Now;
        await db.SaveChangesAsync(ct);audit.Add("KetQuaKhoaHoc",result.Id,"GV_DE_XUAT_KET_QUA");await db.SaveChangesAsync(ct);
        return ManagementService.Scalars(result);
    }

}
