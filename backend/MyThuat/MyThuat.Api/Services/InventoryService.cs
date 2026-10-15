using System.Data;
using Microsoft.EntityFrameworkCore;
using MyThuat.Api.Contracts;
using MyThuat.Api.Data;
using MyThuat.Api.Models;
using MyThuat.Api.Security;
namespace MyThuat.Api.Services;
public sealed class InventoryService(MyThuatDbContext db,AccessService access,AuditService audit)
{
    public static readonly string[] Types=["NHAP","XUAT_TIEU_HAO","CAP_DUNG_CHUNG","TRA_DUNG_CHUNG","BAN_RIENG",
        "TRA_HANG","NHAP_DIEU_CHINH","XUAT_DIEU_CHINH","DE_NGHI_MUA","KIEM_KE","DU_TRU","HONG_MAT"];
    public static int Sign(string type)=>type switch
    {
        "NHAP" or "TRA_DUNG_CHUNG" or "TRA_HANG" or "NHAP_DIEU_CHINH"=>1,
        "XUAT_TIEU_HAO" or "CAP_DUNG_CHUNG" or "BAN_RIENG" or "XUAT_DIEU_CHINH"=>-1,
        _=>0
    };
    public async Task<(decimal stock,decimal reserved,decimal borrowed)> Quantities(long id,string? lot,CancellationToken ct,long? ignore=null)
    {
        var rows=await db.ChiTietPhieuKho.Include(x=>x.PhieuKho).Where(x=>x.HoaCuId==id&&x.MaLo==lot&&x.PhieuKhoId!=ignore
            &&(x.PhieuKho!.TrangThai=="DA_GHI_SO"||x.PhieuKho.TrangThai=="DA_DUYET")).ToListAsync(ct);
        var stock=rows.Where(x=>x.PhieuKho!.TrangThai=="DA_GHI_SO").Sum(x=>Sign(x.PhieuKho!.LoaiPhieu)*x.SoLuong);
        var reserved=rows.Where(x=>x.PhieuKho!.LoaiPhieu=="DU_TRU"&&x.PhieuKho.TrangThai=="DA_DUYET").Sum(x=>x.SoLuong);
        var borrowed=rows.Where(x=>x.PhieuKho!.TrangThai=="DA_GHI_SO").Sum(x=>x.PhieuKho!.LoaiPhieu=="CAP_DUNG_CHUNG"?x.SoLuong
            :x.PhieuKho.LoaiPhieu is "TRA_DUNG_CHUNG" or "HONG_MAT"?-x.SoLuong:0);
        return (stock,reserved,borrowed);
    }
    public async Task<object> Create(InventoryRequest r,CancellationToken ct)
    {
        access.Need(Permission.Kho);
        ApiError.Require(Types.Contains(r.LoaiPhieu)&&r.ChiTiet.Count is >0 and <=100,"Loại phiếu hoặc chi tiết không hợp lệ.");
        ApiError.Require(r.ChiTiet.GroupBy(x=>new{x.HoaCuId,x.MaLo}).All(x=>x.Count()==1),"Không lặp cùng SKU/lô trong phiếu.");
        await using var tx=await db.Database.BeginTransactionAsync(IsolationLevel.Serializable,ct);
        var slip=new PhieuKho {MaPhieu=r.MaPhieu,LoaiPhieu=r.LoaiPhieu,NhaCungCapId=r.NhaCungCapId,BuoiHocId=r.BuoiHocId,
            PhieuThamChieuId=r.PhieuThamChieuId,NguoiNhan=r.NguoiNhan,LapLuc=BusinessClock.Now,NguoiLap=access.User.AccountId(),
            LyDo=r.LyDo,TepBienBanUrl=r.TepBienBanUrl,TrangThai="NHAP"};
        db.PhieuKho.Add(slip);await db.SaveChangesAsync(ct);
        foreach(var line in r.ChiTiet)
        {
            ApiError.Require(line.SoLuong>=0&&decimal.Round(line.SoLuong,3)==line.SoLuong&&(r.LoaiPhieu=="KIEM_KE"||line.SoLuong>0)&&line.DonGia>=0&&decimal.Truncate(line.DonGia)==line.DonGia,"Lượng và giá không hợp lệ.");
            var material=await db.HoaCu.SingleOrDefaultAsync(x=>x.Id==line.HoaCuId&&x.TrangThai=="HOAT_DONG",ct)??throw new ApiError(400,"Họa cụ không hoạt động.");
            ApiError.Require(r.LoaiPhieu is not ("CAP_DUNG_CHUNG" or "TRA_DUNG_CHUNG" or "HONG_MAT")||material.NhomHoaCu=="DUNG_CHUNG","Loại phiếu này chỉ dùng cho dụng cụ chung.");
            ApiError.Require(r.LoaiPhieu!="BAN_RIENG"||material.NhomHoaCu=="BAN_RIENG","Chỉ xuất bán họa cụ bán riêng.");
            ApiError.Require(r.LoaiPhieu!="XUAT_TIEU_HAO"||material.NhomHoaCu=="TIEU_HAO","Xuất tiêu hao cần đúng nhóm vật tư.");
            var qty=await Quantities(material.Id,line.MaLo,ct);
            db.ChiTietPhieuKho.Add(new(){PhieuKhoId=slip.Id,HoaCuId=material.Id,MaLo=line.MaLo,HanSuDung=line.HanSuDung,
                SoLuong=line.SoLuong,DonGiaChot=line.DonGia,ChiTietThamChieuId=line.ChiTietThamChieuId,KhoanThuDangKyId=line.KhoanThuDangKyId,
                SoLuongSoSachTaiKho=r.LoaiPhieu=="KIEM_KE"?qty.stock:null,SoLuongThucTeTaiKho=line.SoLuongThucTe});
        }
        audit.Add("PhieuKho",slip.Id,"LAP_PHIEU_KHO",new {r.LoaiPhieu});await db.SaveChangesAsync(ct);await tx.CommitAsync(ct);
        return ManagementService.Scalars(slip);
    }
    public async Task<object> Approve(long id,InventoryApproval r,CancellationToken ct)
    {
        access.Need(Permission.Kho);
        await using var tx=await db.Database.BeginTransactionAsync(IsolationLevel.Serializable,ct);
        var slip=await db.PhieuKho.SingleOrDefaultAsync(x=>x.Id==id,ct)??throw new ApiError(404,"Không có phiếu kho.");
        ApiError.Require(slip.TrangThai=="NHAP"&&Convert.ToBase64String(slip.RowVersion)==r.RowVersion,"Phiếu đã thay đổi hoặc đã ghi sổ.",409);
        var adjustment=slip.LoaiPhieu is "NHAP_DIEU_CHINH" or "XUAT_DIEU_CHINH";
        if(adjustment)ApiError.Require(access.User.IsInRole("QUAN_TRI")&&slip.NguoiLap!=access.User.AccountId()
            &&!string.IsNullOrWhiteSpace(slip.TepBienBanUrl)&&slip.PhieuThamChieuId.HasValue,"Điều chỉnh cần biên bản, phiếu kiểm kê và quản trị khác người lập duyệt.",403);
        var lines=await db.ChiTietPhieuKho.Where(x=>x.PhieuKhoId==id).OrderBy(x=>x.HoaCuId).ThenBy(x=>x.MaLo).ToListAsync(ct);
        ApiError.Require(lines.Count>0,"Phiếu chưa có chi tiết.");
        foreach(var line in lines)
        {
            if(db.Database.IsSqlServer())
                _=await db.HoaCu.FromSqlInterpolated($"SELECT * FROM dbo.HoaCu WITH (UPDLOCK,HOLDLOCK) WHERE Id={line.HoaCuId}").SingleAsync(ct);
            var material=await db.HoaCu.SingleAsync(x=>x.Id==line.HoaCuId,ct);
            ApiError.Require(material.TrangThai=="HOAT_DONG","Họa cụ không hoạt động.");
            var qty=await Quantities(line.HoaCuId,line.MaLo,ct,id);
            var other=await db.ChiTietPhieuKho.Where(x=>x.HoaCuId==line.HoaCuId&&x.MaLo==line.MaLo&&x.PhieuKhoId!=id
                &&x.PhieuKho!.TrangThai=="DA_GHI_SO"&&x.PhieuKho.LoaiPhieu=="NHAP").ToListAsync(ct);
            if(other.Count>0)ApiError.Require(other.All(x=>x.HanSuDung==line.HanSuDung),"Hạn dùng không thống nhất trong cùng SKU/lô.");
            if(Sign(slip.LoaiPhieu)<0||slip.LoaiPhieu=="DU_TRU")
            {
                ApiError.Require(line.HanSuDung is null||line.HanSuDung>=BusinessClock.Today,"Không cấp lô hết hạn.");
                ApiError.Require(qty.stock-qty.reserved>=line.SoLuong,"Tồn khả dụng không đủ.",409);
                if(slip.LoaiPhieu!="DU_TRU"&&line.MaLo is not null)ApiError.Require(other.Count>0,"Lô chưa được nhập.");
            }
            if(slip.LoaiPhieu=="NHAP")ApiError.Require(slip.NhaCungCapId.HasValue,"Phiếu nhập cần nhà cung cấp.");
            if(slip.LoaiPhieu is "TRA_DUNG_CHUNG" or "HONG_MAT" or "TRA_HANG"||line.ChiTietThamChieuId.HasValue)
            {
                var source=await db.ChiTietPhieuKho.Include(x=>x.PhieuKho).SingleOrDefaultAsync(x=>x.Id==line.ChiTietThamChieuId,ct)??throw new ApiError(400,"Thiếu dòng tham chiếu gốc.");
                ApiError.Require(source.PhieuKho!.TrangThai is "DA_GHI_SO" or "DA_DUYET"&&source.HoaCuId==line.HoaCuId&&source.MaLo==line.MaLo,"Dòng tham chiếu không cùng SKU/lô hoặc chưa duyệt.");
                var returns=await db.ChiTietPhieuKho.Where(x=>x.ChiTietThamChieuId==source.Id&&x.PhieuKhoId!=id&&x.PhieuKho!.TrangThai=="DA_GHI_SO").ToListAsync(ct);
                ApiError.Require(returns.Sum(x=>x.SoLuong)+line.SoLuong<=source.SoLuong,"Lượng nhận/trả vượt lượng tham chiếu còn lại.",409);
                if(slip.LoaiPhieu is "TRA_DUNG_CHUNG" or "HONG_MAT")ApiError.Require(source.PhieuKho.LoaiPhieu=="CAP_DUNG_CHUNG","Phải dẫn chiếu dòng cấp dụng cụ.");
                if(slip.LoaiPhieu=="TRA_HANG")ApiError.Require(source.PhieuKho.LoaiPhieu=="BAN_RIENG","Phải dẫn chiếu dòng xuất bán.");
                if(slip.LoaiPhieu=="NHAP")ApiError.Require(source.PhieuKho.LoaiPhieu=="DE_NGHI_MUA","Nhận hàng dẫn chiếu đề nghị mua.");
            }
            if(slip.LoaiPhieu=="BAN_RIENG")
            {
                var charge=await db.KhoanThuDangKy.SingleOrDefaultAsync(x=>x.Id==line.KhoanThuDangKyId&&x.HoaCuId==line.HoaCuId&&x.LoaiKhoan=="HOA_CU",ct)??throw new ApiError(400,"Xuất bán cần khoản thu họa cụ tương ứng.");
                var receipts=await db.GiaoDichTien.Where(x=>x.KhoanThuId==charge.Id&&x.TrangThai=="DA_XAC_NHAN").ToListAsync(ct);
                ApiError.Require(receipts.Sum(x=>x.LoaiGiaoDich=="THU"?x.SoTien:-x.SoTien)>=charge.ThanhTienChot,"Chưa thanh toán đủ họa cụ.");
                ApiError.Require(charge.SoLuongDaGiao+line.SoLuong<=charge.SoLuong,"Giao vượt lượng đã mua.");
                charge.SoLuongDaGiao+=line.SoLuong;
            }
            if(adjustment)
            {
                var count=await db.ChiTietPhieuKho.Include(x=>x.PhieuKho).SingleOrDefaultAsync(x=>x.PhieuKhoId==slip.PhieuThamChieuId
                    &&x.HoaCuId==line.HoaCuId&&x.MaLo==line.MaLo,ct)??throw new ApiError(400,"Không có dòng kiểm kê tương ứng.");
                ApiError.Require(count.PhieuKho!.LoaiPhieu=="KIEM_KE"&&count.PhieuKho.TrangThai=="DA_DUYET"&&count.SoLuongThucTeTaiKho.HasValue,"Phiếu kiểm kê chưa duyệt hoặc thiếu thực tế.");
                ApiError.Require(qty.stock==count.SoLuongSoSachTaiKho,"Sổ kho thay đổi sau kiểm kê; kiểm kê lại.",409);
                var difference=count.SoLuongThucTeTaiKho!.Value-qty.stock;
                ApiError.Require(line.SoLuong==Math.Abs(difference)&&(difference>0)==(Sign(slip.LoaiPhieu)>0),"Lượng điều chỉnh phải bằng chênh lệch kiểm kê.");
                ApiError.Require(!await db.PhieuKho.AnyAsync(x=>x.Id!=id&&x.PhieuThamChieuId==slip.PhieuThamChieuId&&x.TrangThai=="DA_GHI_SO"&&
                    (x.LoaiPhieu=="NHAP_DIEU_CHINH"||x.LoaiPhieu=="XUAT_DIEU_CHINH"),ct),"Kiểm kê đã được điều chỉnh.",409);
            }
            if(slip.LoaiPhieu=="KIEM_KE")ApiError.Require(line.SoLuongThucTeTaiKho>=0,"Kiểm kê cần số lượng thực tế không âm.");
        }
        slip.NguoiDuyet=access.User.AccountId();slip.DuyetLuc=BusinessClock.Now;
        slip.TrangThai=Sign(slip.LoaiPhieu)!=0||slip.LoaiPhieu=="HONG_MAT"?"DA_GHI_SO":"DA_DUYET";
        audit.Add("PhieuKho",id,"DUYET_PHIEU_KHO",new {slip.LoaiPhieu,slip.TrangThai});await db.SaveChangesAsync(ct);await tx.CommitAsync(ct);return ManagementService.Scalars(slip);
    }
    public async Task<object> Stock(CancellationToken ct)
    {
        var materials=await db.HoaCu.AsNoTracking().OrderBy(x=>x.Id).ToListAsync(ct);
        var result=new List<object>();
        foreach(var m in materials)
        {
            var lots=await db.ChiTietPhieuKho.Where(x=>x.HoaCuId==m.Id).Select(x=>x.MaLo).Distinct().ToListAsync(ct);
            if(lots.Count==0)lots.Add(null);
            foreach(var lot in lots){var q=await Quantities(m.Id,lot,ct);result.Add(new {m.Id,m.TenHoaCu,m.DonViCoSo,MaLo=lot,TaiKho=q.stock,DuTru=q.reserved,DangMuon=q.borrowed,KhaDung=q.stock-q.reserved});}
        }
        return result;
    }
}
