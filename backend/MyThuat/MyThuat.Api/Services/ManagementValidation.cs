using Microsoft.EntityFrameworkCore;
using MyThuat.Api.Data;
using MyThuat.Api.Models;
using MyThuat.Api.Security;
namespace MyThuat.Api.Services;
public sealed class ManagementValidation(MyThuatDbContext db,AccessService access)
{
    public async Task Check(EntityBase item,bool update,Dictionary<string,object?> before,CancellationToken ct)
    {
        switch(item)
        {
            case HocVien v:
                ApiError.Require(v.NgaySinh<=BusinessClock.Today,"Ngày sinh không được ở tương lai.");
                break;
            case GiaoVien g:
                ApiError.Require(g.NgaySinh is null||g.NgaySinh<=BusinessClock.Today,"Ngày sinh không được ở tương lai.");
                break;
            case PhongHoc p:
                ApiError.Require(p.SucChua>0,"Sức chứa phải lớn hơn 0.");
                if(update)ApiError.Require(!await db.BuoiHoc.AnyAsync(x=>x.PhongHocId==p.Id&&x.TrangThai=="DA_XEP_LICH"&&x.LopHoc!.SiSoToiDa>p.SucChua,ct),"Sức chứa thấp hơn lớp đã xếp lịch.");
                break;
            case KhoaHoc k:
                ApiError.Require(k.SoBuoi>0&&k.SoTietMoiBuoi>0&&k.PhutMoiTiet>0&&k.HocPhi>=0&&k.PhienBan>0&&k.TuoiToiThieu is null or >=0,"Thông số khóa học không hợp lệ.");
                if(update&&await db.LopHoc.AnyAsync(x=>x.KhoaHocId==k.Id,ct))
                    ApiError.Require(ManagementService.Scalars(k).All(p=>p.Key is "TrangThai" or "RowVersion" or "UpdatedAt"||Equals(p.Value,before[p.Key])),"Phiên bản khóa đã dùng: tạo phiên bản mới thay vì sửa.");
                if(k.TrangThai=="HOAT_DONG")
                {
                    ApiError.Require(access.User.IsInRole("QUAN_TRI"),"Quản trị duyệt công bố khóa.",403);
                    ApiError.Require(await db.NoiDungBuoi.CountAsync(x=>x.KhoaHocId==k.Id,ct)==k.SoBuoi,"Cần đủ nội dung từng buổi trước khi công bố khóa.");
                }
                if(k.KhoaGocId.HasValue)ApiError.Require(k.KhoaGocId!=k.Id&&await db.KhoaHoc.AnyAsync(x=>x.Id==k.KhoaGocId&&x.MaKhoa==k.MaKhoa,ct),"Khóa gốc không hợp lệ.");
                break;
            case NoiDungBuoi n:
                ApiError.Require(n.ThuTu>0&&n.SoTiet>0,"Thứ tự và số tiết phải lớn hơn 0.");
                ApiError.Require(!await db.LopHoc.AnyAsync(x=>x.KhoaHocId==n.KhoaHocId,ct),"Đề cương phiên bản đã mở lớp không được sửa.");
                var course=await db.KhoaHoc.SingleOrDefaultAsync(x=>x.Id==n.KhoaHocId,ct)??throw new ApiError(400,"Khóa không tồn tại.");
                ApiError.Require(n.ThuTu<=course.SoBuoi,"Thứ tự vượt số buổi khóa.");
                break;
            case LopHoc l:
                var c=await db.KhoaHoc.SingleOrDefaultAsync(x=>x.Id==l.KhoaHocId,ct)??throw new ApiError(400,"Khóa không tồn tại.");
                ApiError.Require(l.SoBuoiKeHoach==c.SoBuoi&&l.SiSoToiThieu>0&&l.SiSoToiDa>=l.SiSoToiThieu&&l.HocPhiApDung>=0
                    &&l.MoDangKyLuc<l.DongDangKyLuc,"Thông số lớp không hợp lệ.");
                ApiError.Require(l.TrangThai is "NHAP" or "DANG_TUYEN_SINH" or "DANG_HOC" or "DA_KET_THUC","Trạng thái lớp không hợp lệ; hủy lớp cần xử lý đăng ký liên quan.");
                if(update&&await db.DangKy.AnyAsync(x=>x.LopHocId==l.Id,ct))
                    ApiError.Require(Equals(before["KhoaHocId"],l.KhoaHocId)&&Equals(before["HocPhiApDung"],l.HocPhiApDung)
                        &&Equals(before["SoBuoiKeHoach"],l.SoBuoiKeHoach),"Lớp đã có đăng ký: không sửa khóa, số buổi hay học phí.");
                if(l.TrangThai!="NHAP")
                {
                    ApiError.Require(access.User.IsInRole("QUAN_TRI"),"Quản trị duyệt công bố và trạng thái vận hành lớp.",403);
                    l.NguoiDuyet=access.User.AccountId();
                    ApiError.Require(c.TrangThai=="HOAT_DONG","Khóa chưa công bố.");
                    var sessions=await db.BuoiHoc.Where(x=>x.LopHocId==l.Id&&x.TrangThai!="DA_HUY").ToListAsync(ct);
                    ApiError.Require(sessions.Select(x=>x.NoiDungBuoiId).Distinct().Count()==l.SoBuoiKeHoach,"Lớp cần đủ lịch từng buổi.");
                    foreach(var b in sessions)
                    {
                        ApiError.Require(await db.GiangDayBuoi.AnyAsync(x=>x.BuoiHocId==b.Id&&x.VaiTro=="CHINH",ct),"Mỗi buổi phải có giáo viên chính.");
                        if(c.CapDo.Equals("Junior",StringComparison.OrdinalIgnoreCase))
                            ApiError.Require(await db.GiangDayBuoi.AnyAsync(x=>x.BuoiHocId==b.Id&&x.VaiTro=="TRO_GIANG",ct),"Junior cần trợ giảng từng buổi.");
                    }
                    l.NgayKetThucDuKien=DateOnly.FromDateTime(sessions.Max(x=>x.KetThuc));
                    ApiError.Require(l.DongDangKyLuc<=sessions.Min(x=>x.BatDau).AddHours(-48),"Cần đóng tuyển sinh ít nhất 48 giờ trước buổi đầu.");
                    if(l.TrangThai=="DA_KET_THUC")
                        ApiError.Require(sessions.All(x=>x.TrangThai=="DA_TO_CHUC"),"Cần tổ chức đủ các buổi trước khi kết thúc lớp.");
                    if(l.TrangThai=="DANG_HOC")
                    {
                        ApiError.Require(access.User.IsInRole("QUAN_TRI"),"Quản trị duyệt khai giảng.",403);
                        ApiError.Require(await db.DangKy.CountAsync(x=>x.LopHocId==l.Id&&x.TrangThai=="DA_XAC_NHAN",ct)>=l.SiSoToiThieu,"Chưa đủ học viên đã thanh toán để khai giảng.");
                        l.NgayKhaiGiangThucTe=BusinessClock.Today;
                    }
                }
                break;
            case DaiDienHocVien d:
                ApiError.Require(d.QuanHe is "CHA" or "ME" or "GIAM_HO" or "TU_BAN_THAN"&&d.QuyenDaiDien is "DAY_DU" or "DANG_KY" or "XEM_KET_QUA","Quan hệ hoặc quyền đại diện không hợp lệ.");
                ApiError.Require(d.HieuLucDen is null||d.HieuLucDen>d.HieuLucTu,"Khoảng hiệu lực không hợp lệ.");
                if(d.LaLienHeChinh)ApiError.Require(!await db.DaiDienHocVien.AnyAsync(x=>x.Id!=d.Id&&x.HocVienId==d.HocVienId&&x.LaLienHeChinh&&(x.HieuLucDen==null||x.HieuLucDen>d.HieuLucTu),ct),"Học viên đã có liên hệ chính còn hiệu lực.");
                d.XacNhanLuc=BusinessClock.Now;d.NguoiXacNhan=access.User.AccountId();
                if(d.DongYHinhAnh||d.DongYTacPham)d.ThoiDiemDongY=BusinessClock.Now;
                break;
            case ThanhVien m:
                if(m.TrangThai=="HOAT_DONG"&&m.XacNhanThanhVienLuc is null)m.XacNhanThanhVienLuc=BusinessClock.Now;
                break;
            case KhuyenMai p:
                ApiError.Require(access.User.IsInRole("QUAN_TRI"),"Quản trị duyệt ưu đãi.",403);
                ApiError.Require(p.NhomKhoanThu is "HOC_PHI" or "HOA_CU"&&p.LoaiGiam is "PHAN_TRAM" or "SO_TIEN"
                    &&p.GiaTriGiam>=0&&(p.LoaiGiam!="PHAN_TRAM"||p.GiaTriGiam<=100)&&p.TongLuot>0&&p.BatDau<p.KetThuc,"Ưu đãi không hợp lệ.");
                if(update)ApiError.Require(!await db.ApDungUuDai.AnyAsync(x=>x.KhuyenMaiId==p.Id,ct),"Ưu đãi đã sử dụng: tạo mã mới.");
                break;
            case QuyDinh q:
                ApiError.Require(access.User.IsInRole("QUAN_TRI"),"Chỉ quản trị duyệt quy định.",403);
                _=PolicySettings.Parse(q.NoiDungJson);
                if(update)ApiError.Require(!await db.HoSoTheoHoc.AnyAsync(x=>x.QuyDinhId==q.Id,ct),"Quy định đã dùng: tạo phiên bản mới.");
                ApiError.Require(q.SoPhienBan>0&&(q.HieuLucDen is null||q.HieuLucDen>q.HieuLucTu),"Hiệu lực quy định không hợp lệ.");
                q.NguoiDuyet=access.User.AccountId();
                break;
            case HoaCu h:
                ApiError.Require(h.NhomHoaCu is "TIEU_HAO" or "DUNG_CHUNG" or "BAN_RIENG"&&h.GiaBanThamKhao>=0&&h.MucDatHang>=0,"Thông tin họa cụ không hợp lệ.");
                if(update&&await db.ChiTietPhieuKho.AnyAsync(x=>x.HoaCuId==h.Id,ct))
                    ApiError.Require(Equals(before["DonViCoSo"],h.DonViCoSo)&&Equals(before["NhomHoaCu"],h.NhomHoaCu),"Họa cụ đã có sổ kho: giữ đơn vị và nhóm.");
                break;
            case DinhMucHoaCu d:
                ApiError.Require(d.SoLuong>0&&d.CoSoTinh is "HOC_VIEN" or "LOP","Định mức không hợp lệ.");break;
        }
        var status=item.GetType().GetProperty("TrangThai");
        if(item is HocVien or GiaoVien or ThanhVien or HoaCu or NhaCungCap or LoaiKhoaHoc or PhongHoc)
            ApiError.Require(status?.GetValue(item) is "NHAP" or "HOAT_DONG" or "NGUNG" or "CHO_XAC_NHAN","Trạng thái hồ sơ không hợp lệ.");
    }
}
