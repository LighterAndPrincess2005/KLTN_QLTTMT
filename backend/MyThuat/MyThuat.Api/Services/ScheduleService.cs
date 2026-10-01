using System.Data;
using Microsoft.EntityFrameworkCore;
using MyThuat.Api.Contracts;
using MyThuat.Api.Data;
using MyThuat.Api.Models;
using MyThuat.Api.Security;
namespace MyThuat.Api.Services;
public sealed class ScheduleService(MyThuatDbContext db,AuditService audit,AccessService access,PolicyService policies)
{
    public async Task CheckSlot(long classId,long roomId,DateTime start,DateTime end,IEnumerable<long> teacherIds,long? ignore,CancellationToken ct)
    {
        ApiError.Require(start.Kind!=DateTimeKind.Utc&&start.Year>=1900&&end>start,"Giờ học phải là giờ Việt Nam hợp lệ.");
        var rule=PolicySettings.Parse((await policies.Current(ct)).NoiDungJson);
        var padding=rule.NghiGiuaCaPhut;
        var from=start.AddMinutes(-padding);var to=end.AddMinutes(padding);
        var room=await db.PhongHoc.SingleOrDefaultAsync(x=>x.Id==roomId&&x.TrangThai=="HOAT_DONG",ct)??throw new ApiError(400,"Phòng không hoạt động.");
        var cls=await db.LopHoc.SingleAsync(x=>x.Id==classId,ct);
        ApiError.Require(room.SucChua>=cls.SiSoToiDa,"Sức chứa phòng thấp hơn sĩ số lớp.");
        ApiError.Require(!rule.NgayNghi.Contains(DateOnly.FromDateTime(start)),"Ngày này nằm trong lịch nghỉ.");
        ApiError.Require(!await db.BuoiHoc.AnyAsync(x=>x.Id!=ignore&&x.TrangThai!="DA_HUY"&&x.BatDau<to&&x.KetThuc>from
            &&(x.PhongHocId==roomId||x.LopHocId==classId),ct),"Trùng lịch phòng hoặc lớp, hoặc chưa đủ thời gian giữa hai ca.",409);
        foreach(var teacher in teacherIds.Distinct())
        {
            ApiError.Require(await db.GiaoVien.AnyAsync(x=>x.Id==teacher&&x.TrangThai=="HOAT_DONG",ct),"Giáo viên không hoạt động.");
            ApiError.Require(!await db.GiangDayBuoi.AnyAsync(x=>x.GiaoVienId==teacher&&x.BuoiHocId!=ignore
                &&x.BuoiHoc!.TrangThai!="DA_HUY"&&x.BuoiHoc.BatDau<to&&x.BuoiHoc.KetThuc>from,ct),"Giáo viên trùng lịch.",409);
        }
        var makeupStudents=ignore.HasValue?await db.DiemDanh.Where(x=>x.BuoiHocId==ignore&&x.LoaiThamGia=="HOC_BU"&&x.TrangThai=="CHO_THAM_GIA").Select(x=>x.DangKy!.HoSoTheoHoc!.HocVienId).ToListAsync(ct):[];
        var students=await db.DangKy.Where(x=>x.LopHocId==classId&&(x.TrangThai=="DA_XAC_NHAN"
            ||x.TrangThai=="CHO_THANH_TOAN"&&x.HanGiuCho>BusinessClock.Now)).Select(x=>x.HoSoTheoHoc!.HocVienId).ToListAsync(ct);
        students.AddRange(makeupStudents);
        ApiError.Require(!await db.DiemDanh.AnyAsync(x=>x.BuoiHocId!=ignore&&x.TrangThai=="CHO_THAM_GIA"
            &&students.Contains(x.DangKy!.HoSoTheoHoc!.HocVienId)&&x.BuoiHoc!.TrangThai!="DA_HUY"
            &&x.BuoiHoc.BatDau<end&&x.BuoiHoc.KetThuc>start,ct),"Học viên của lớp trùng lịch học khác.",409);
    }
    public async Task<object> Generate(long classId,GenerateScheduleRequest request,CancellationToken ct)
    {
        access.Need(Permission.DaoTao);
        ApiError.Require(request.BuoiDau>BusinessClock.Now&&request.BuoiDau.Kind!=DateTimeKind.Utc,"Buổi đầu phải ở tương lai và dùng giờ Việt Nam.");
        await using var tx=await db.Database.BeginTransactionAsync(IsolationLevel.Serializable,ct);
        var cls=await db.LopHoc.Include(x=>x.KhoaHoc).SingleOrDefaultAsync(x=>x.Id==classId,ct)??throw new ApiError(404,"Không có lớp.");
        ApiError.Require(cls.TrangThai=="NHAP"&&!await db.BuoiHoc.AnyAsync(x=>x.LopHocId==classId,ct),"Chỉ sinh lịch lần đầu cho lớp nháp; thay đổi dùng dời lịch.",409);
        var contents=await db.NoiDungBuoi.Where(x=>x.KhoaHocId==cls.KhoaHocId).OrderBy(x=>x.ThuTu).ToListAsync(ct);
        ApiError.Require(contents.Count==cls.SoBuoiKeHoach,"Chưa đủ đề cương khóa.");
        ApiError.Require(request.XacNhanDuChuyenMon,"Học vụ cần xác nhận giáo viên đủ chuyên môn.");
        ApiError.Require(!cls.KhoaHoc!.CapDo.Equals("Junior",StringComparison.OrdinalIgnoreCase)||request.TroGiangId.HasValue,"Lớp Junior cần trợ giảng.");
        var teachers=new List<long>{request.GiaoVienId};if(request.TroGiangId is long assistant)teachers.Add(assistant);
        ApiError.Require(teachers.Distinct().Count()==teachers.Count,"Giáo viên chính và trợ giảng phải khác nhau.");
        var rule=PolicySettings.Parse((await policies.Current(ct)).NoiDungJson);
        var time=request.BuoiDau;var sessions=new List<BuoiHoc>();
        foreach(var content in contents)
        {
            var attempts=0;
            while(rule.NgayNghi.Contains(DateOnly.FromDateTime(time)))
            {time=time.AddDays(7*request.CachTuan);ApiError.Require(++attempts<104,"Không tìm được ngày học hợp lệ.");}
            var finish=time.AddMinutes(content.SoTiet*cls.KhoaHoc!.PhutMoiTiet+request.PhutNghi);
            await CheckSlot(classId,request.PhongHocId,time,finish,teachers,null,ct);
            var b=new BuoiHoc {LopHocId=classId,NoiDungBuoiId=content.Id,PhongHocId=request.PhongHocId,
                ThuTuTrongLop=content.ThuTu,LanXepLich=1,BatDau=time,KetThuc=finish,SoTiet=content.SoTiet,
                PhutMoiTiet=cls.KhoaHoc!.PhutMoiTiet,PhutNghi=request.PhutNghi,TrangThai="DA_XEP_LICH"};
            db.BuoiHoc.Add(b);await db.SaveChangesAsync(ct);
            db.GiangDayBuoi.Add(new GiangDayBuoi {BuoiHocId=b.Id,GiaoVienId=request.GiaoVienId,VaiTro="CHINH",SoTietThucDay=b.SoTiet,NguoiDuyet=access.User.AccountId()});
            if(request.TroGiangId is long a)db.GiangDayBuoi.Add(new GiangDayBuoi {BuoiHocId=b.Id,GiaoVienId=a,VaiTro="TRO_GIANG",SoTietThucDay=b.SoTiet,NguoiDuyet=access.User.AccountId()});
            await db.SaveChangesAsync(ct);sessions.Add(b);time=time.AddDays(7*request.CachTuan);
        }
        foreach(var teacher in teachers)
        {
            var role=teacher==request.GiaoVienId?"CHINH":"TRO_GIANG";
            db.PhanCong.Add(new(){LopHocId=cls.Id,GiaoVienId=teacher,VaiTroTrongLop=role,
                TuNgay=DateOnly.FromDateTime(sessions[0].BatDau),DenNgay=DateOnly.FromDateTime(sessions[^1].KetThuc),
                XacNhanDuChuyenMon=true,XacNhanNhanLopLuc=BusinessClock.Now,NguoiDuyet=access.User.AccountId()});
        }
        cls.NgayKhaiGiangDuKien=DateOnly.FromDateTime(sessions[0].BatDau);
        cls.NgayKetThucDuKien=DateOnly.FromDateTime(sessions[^1].KetThuc);
        audit.Add("LopHoc",classId,"SINH_LICH",new {SoBuoi=sessions.Count});await db.SaveChangesAsync(ct);await tx.CommitAsync(ct);
        return sessions.Select(ManagementService.Scalars);
    }
    public async Task<object> Assign(long classId,AssignTeacherRequest r,CancellationToken ct)
    {
        access.Need(Permission.DaoTao);
        ApiError.Require(r.XacNhanDuChuyenMon&&r.DenNgay>=r.TuNgay&&r.VaiTro is "CHINH" or "TRO_GIANG","Cần xác nhận chuyên môn, vai trò và khoảng ngày hợp lệ.");
        await using var tx=await db.Database.BeginTransactionAsync(IsolationLevel.Serializable,ct);
        ApiError.Require(await db.GiaoVien.AnyAsync(x=>x.Id==r.GiaoVienId&&x.TrangThai=="HOAT_DONG",ct),"Giáo viên không hoạt động.");
        ApiError.Require(!await db.PhanCong.AnyAsync(x=>x.LopHocId==classId&&x.VaiTroTrongLop==r.VaiTro&&x.TuNgay<=r.DenNgay&&x.DenNgay>=r.TuNgay,ct),"Khoảng phân công chồng lấn.",409);
        var p=new PhanCong {LopHocId=classId,GiaoVienId=r.GiaoVienId,VaiTroTrongLop=r.VaiTro,TuNgay=r.TuNgay,DenNgay=r.DenNgay,
            XacNhanDuChuyenMon=true,XacNhanNhanLopLuc=BusinessClock.Now,NguoiDuyet=access.User.AccountId()};
        db.PhanCong.Add(p);await db.SaveChangesAsync(ct);audit.Add("PhanCong",p.Id,"PHAN_CONG");await db.SaveChangesAsync(ct);await tx.CommitAsync(ct);return ManagementService.Scalars(p);
    }
    public async Task<object> Substitute(long id,SubstituteRequest r,CancellationToken ct)
    {
        access.Need(Permission.DaoTao);ApiError.Require(r.XacNhanDuChuyenMon&&r.VaiTro is "CHINH" or "TRO_GIANG","Cần xác nhận chuyên môn và vai trò.");
        await using var tx=await db.Database.BeginTransactionAsync(IsolationLevel.Serializable,ct);
        var b=await db.BuoiHoc.SingleOrDefaultAsync(x=>x.Id==id,ct)??throw new ApiError(404,"Không có buổi.");
        ApiError.Require(b.TrangThai=="DA_XEP_LICH"&&b.BatDau>BusinessClock.Now,"Chỉ đổi giáo viên trước buổi học.");
        var staff=await db.GiangDayBuoi.Where(x=>x.BuoiHocId==id).ToListAsync(ct);
        ApiError.Require(!staff.Any(x=>x.VaiTro!=r.VaiTro&&x.GiaoVienId==r.GiaoVienId),"Giáo viên đã có vai trò khác trong buổi.");
        await CheckSlot(b.LopHocId,b.PhongHocId,b.BatDau,b.KetThuc,staff.Where(x=>x.VaiTro!=r.VaiTro).Select(x=>x.GiaoVienId).Append(r.GiaoVienId),id,ct);
        var assignment=staff.SingleOrDefault(x=>x.VaiTro==r.VaiTro);
        if(assignment is null){assignment=new GiangDayBuoi {BuoiHocId=id,VaiTro=r.VaiTro};db.GiangDayBuoi.Add(assignment);}
        var old=assignment.GiaoVienId;assignment.GiaoVienId=r.GiaoVienId;assignment.SoTietThucDay=b.SoTiet;
        assignment.LaThayThe=true;assignment.LyDoThayThe=r.LyDo;assignment.NguoiDuyet=access.User.AccountId();
        audit.Add("BuoiHoc",id,"THAY_GIAO_VIEN",new {GiaoVienCu=old,GiaoVienMoi=r.GiaoVienId,r.VaiTro,r.LyDo});
        await db.SaveChangesAsync(ct);await tx.CommitAsync(ct);return ManagementService.Scalars(assignment);
    }
    public async Task<object> Move(long id,RescheduleRequest r,CancellationToken ct)
    {
        access.Need(Permission.DaoTao);
        await using var tx=await db.Database.BeginTransactionAsync(IsolationLevel.Serializable,ct);
        var b=await db.BuoiHoc.SingleOrDefaultAsync(x=>x.Id==id,ct)??throw new ApiError(404,"Không có buổi.");
        ApiError.Require(Convert.ToBase64String(b.RowVersion)==r.RowVersion,"Lịch đã thay đổi, tải lại.",409);
        ApiError.Require(b.TrangThai=="DA_XEP_LICH"&&b.BatDau>BusinessClock.Now&&r.BatDau>BusinessClock.Now,"Chỉ dời lịch chưa bắt đầu.");
        var staff=await db.GiangDayBuoi.Where(x=>x.BuoiHocId==id).ToListAsync(ct);
        var end=r.BatDau.Add(b.KetThuc-b.BatDau);
        ApiError.Require(!await db.BuoiHoc.AnyAsync(x=>x.LopHocId==b.LopHocId&&x.Id!=id&&x.TrangThai!="DA_HUY"
            &&(x.ThuTuTrongLop<b.ThuTuTrongLop&&x.KetThuc>r.BatDau||x.ThuTuTrongLop>b.ThuTuTrongLop&&x.BatDau<end),ct),"Dời lịch không được đảo thứ tự nội dung.");
        await CheckSlot(b.LopHocId,r.PhongHocId,r.BatDau,end,staff.Select(x=>x.GiaoVienId),id,ct);
        b.TrangThai="DA_HUY";await db.SaveChangesAsync(ct);
        var next=new BuoiHoc {LopHocId=b.LopHocId,NoiDungBuoiId=b.NoiDungBuoiId,PhongHocId=r.PhongHocId,
            BuoiThayTheChoId=b.Id,ThuTuTrongLop=b.ThuTuTrongLop,LanXepLich=(short)(b.LanXepLich+1),BatDau=r.BatDau,KetThuc=end,
            SoTiet=b.SoTiet,PhutMoiTiet=b.PhutMoiTiet,PhutNghi=b.PhutNghi,LyDoDoiLich=r.LyDo,TrangThai="DA_XEP_LICH"};
        db.BuoiHoc.Add(next);await db.SaveChangesAsync(ct);
        foreach(var x in staff)db.GiangDayBuoi.Add(new GiangDayBuoi {BuoiHocId=next.Id,GiaoVienId=x.GiaoVienId,
            VaiTro=x.VaiTro,SoTietThucDay=x.SoTietThucDay,NguoiDuyet=access.User.AccountId()});
        var pending=await db.DiemDanh.Where(x=>x.BuoiHocId==id&&x.TrangThai=="CHO_THAM_GIA").ToListAsync(ct);
        foreach(var x in pending)
        {
            x.TrangThai="HUY_BO";
            var replacement=new DiemDanh {DangKyId=x.DangKyId,BuoiHocId=next.Id,NoiDungDuocTinhId=x.NoiDungDuocTinhId,
                LoaiThamGia=x.LoaiThamGia,TrangThai="CHO_THAM_GIA",NhanXet=r.LyDo};
            db.DiemDanh.Add(replacement);await db.SaveChangesAsync(ct);
            var makeup=await db.HocBu.SingleOrDefaultAsync(m=>m.DiemDanhBuId==x.Id,ct);
            if(makeup is not null)makeup.DiemDanhBuId=replacement.Id;
        }
        var cls=await db.LopHoc.SingleAsync(x=>x.Id==b.LopHocId,ct);
        cls.NgayKetThucDuKien=DateOnly.FromDateTime(await db.BuoiHoc.Where(x=>x.LopHocId==b.LopHocId&&x.TrangThai!="DA_HUY").MaxAsync(x=>x.KetThuc,ct));
        audit.Add("BuoiHoc",id,"DOI_LICH",new {BuoiMoiId=next.Id,r.LyDo});await db.SaveChangesAsync(ct);await tx.CommitAsync(ct);
        return ManagementService.Scalars(next);
    }
}
