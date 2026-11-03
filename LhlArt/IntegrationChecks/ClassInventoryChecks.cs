using System.Text.Json;
using LhlArt.Networking;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using MyThuat.Api.Data;
using MyThuat.Api.Models;
using MyThuat.Api.Services;
using MyThuat.Checks;

internal static class ClassInventoryChecks
{
    private static long Id(JsonElement row) => ArtApiClient.Get(row, "id").GetInt64();
    private static string Text(JsonElement row, string field) => ArtApiClient.Text(row, field);

    public static async Task Run(ArtApiClient api, IServiceProvider services, DbContextOptions<MyThuatDbContext> options,
        Action<bool,string> check)
    {
        long paperId, brushId, expiredId, startId, cancelId, sessionId, raceA, raceB, raceC, legacyId;
        var now = BusinessClock.Now;
        await using (var db = new CheckDbContext(options))
        {
            var admin = await db.TaiKhoan.SingleAsync(x => x.TenDangNhap == "root");
            var teacher = await db.GiaoVien.FirstAsync();
            var room = await db.PhongHoc.FirstAsync();
            var rule = await db.QuyDinh.FirstAsync();
            var member = await db.ThanhVien.FirstAsync();
            var student = await db.HocVien.FirstAsync();
            var supplier = new NhaCungCap { MaNCC="NCC-KHO-TEST", TenNCC="Kho kiểm thử", TrangThai="HOAT_DONG" };
            var course = new KhoaHoc { MaKhoa="KHO-TEST", TenKhoa="Khóa vẽ kiểm thử kho", CapDo="FOUNDATION",
                LoaiKhoaHocId=await db.LoaiKhoaHoc.Select(x=>x.Id).FirstAsync(), PhienBan=1, MucTieu="Thử dự trù",
                SoBuoi=2, SoTietMoiBuoi=1, PhutMoiTiet=60, HieuLucTu=BusinessClock.Today, TrangThai="HOAT_DONG" };
            var paper = new HoaCu { MaHoaCu="GIAY-KHO-TEST", TenHoaCu="Giấy kiểm thử", DonViCoSo="tờ",
                QuyCach="A4", NhomHoaCu="TIEU_HAO", TrangThai="HOAT_DONG" };
            var brush = new HoaCu { MaHoaCu="CO-KHO-TEST", TenHoaCu="Cọ kiểm thử", DonViCoSo="cây",
                QuyCach="Cọ tròn", NhomHoaCu="DUNG_CHUNG", TrangThai="HOAT_DONG" };
            db.AddRange(supplier,course,paper,brush);
            await db.SaveChangesAsync();
            paperId=paper.Id; brushId=brush.Id;
            var topics = Enumerable.Range(1,2).Select(i => new NoiDungBuoi { KhoaHocId=course.Id,
                ThuTu=(short)i, ChuDe=$"Buổi thử {i}", NoiDung="Vẽ", SoTiet=1 }).ToArray();
            var receipt = new PhieuKho { MaPhieu="KHO-TEST-NHAP", LoaiPhieu="NHAP", NhaCungCapId=supplier.Id,
                TrangThai="DA_GHI_SO", NguoiLap=admin.Id, NguoiDuyet=admin.Id, LapLuc=now, DuyetLuc=now };
            db.AddRange(topics); db.Add(receipt);
            await db.SaveChangesAsync();
            foreach (var topic in topics)
            {
                db.DinhMucHoaCu.Add(new() { NoiDungBuoiId=topic.Id, HoaCuId=paper.Id, CoSoTinh="HOC_VIEN", SoLuong=2 });
                db.DinhMucHoaCu.Add(new() { NoiDungBuoiId=topic.Id, HoaCuId=brush.Id, CoSoTinh="HOC_VIEN", SoLuong=1 });
            }
            db.DinhMucHoaCu.Add(new() { NoiDungBuoiId=topics[0].Id, HoaCuId=paper.Id, CoSoTinh="LOP", SoLuong=1 });
            db.ChiTietPhieuKho.AddRange(
                new ChiTietPhieuKho { PhieuKhoId=receipt.Id, HoaCuId=paper.Id, MaLo="SOM", HanSuDung=BusinessClock.Today.AddDays(90), SoLuong=20 },
                new ChiTietPhieuKho { PhieuKhoId=receipt.Id, HoaCuId=paper.Id, MaLo="MUON", SoLuong=80 },
                new ChiTietPhieuKho { PhieuKhoId=receipt.Id, HoaCuId=paper.Id, MaLo="HET", HanSuDung=BusinessClock.Today.AddDays(-1), SoLuong=100 },
                new ChiTietPhieuKho { PhieuKhoId=receipt.Id, HoaCuId=brush.Id, SoLuong=30 });
            await db.SaveChangesAsync();

            async Task<(long id,long session)> Create(string name, DateTime first, int confirmed, bool pending=false)
            {
                var cls = new LopHoc { MaLop="KHO-TEST-"+name, TenLop="Lớp kho "+name, KhoaHocId=course.Id,
                    NgayKhaiGiangDuKien=DateOnly.FromDateTime(first), SoBuoiKeHoach=2, LichHocDuKien="Hai buổi",
                    SiSoToiThieu=6, SiSoToiDa=8, MoDangKyLuc=now.AddDays(-15), DongDangKyLuc=first.AddDays(-3), TrangThai="NHAP" };
                db.Add(cls);
                await db.SaveChangesAsync();
                var sessions = topics.Select(t => new BuoiHoc { LopHocId=cls.Id, NoiDungBuoiId=t.Id,
                    ThuTuTrongLop=t.ThuTu, LanXepLich=1, PhongHocId=room.Id, SoTiet=1, PhutMoiTiet=60,
                    BatDau=first.AddDays(t.ThuTu-1), KetThuc=first.AddDays(t.ThuTu-1).AddHours(1), TrangThai="DA_XEP_LICH" }).ToArray();
                db.AddRange(sessions);
                await db.SaveChangesAsync();
                foreach (var s in sessions)
                    db.GiangDayBuoi.Add(new() { BuoiHocId=s.Id, GiaoVienId=teacher.Id, VaiTro="CHINH",
                        NguoiDuyet=admin.Id, SoTietThucDay=1 });
                for (int i=0; i<confirmed+(pending?1:0); i++)
                {
                    var profile = new HoSoTheoHoc { HocVienId=student.Id, KhoaHocId=course.Id, QuyDinhId=rule.Id,
                        NgayBatDau=cls.NgayKhaiGiangDuKien, TrangThai="DANG_THEO_HOC" };
                    db.DangKy.Add(new() { MaDangKy=$"KHO-DK-{name}-{i}", LopHocId=cls.Id, HoSoTheoHoc=profile,
                        NguoiDangKyId=member.Id, LapLuc=now.AddDays(-5), HanGiuCho=now.AddHours(-1),
                        TrangThai=i<confirmed?"DA_XAC_NHAN":"CHO_THANH_TOAN", IdempotencyKey=$"KHO-DK-{name}-{i}",
                        RequestFingerprint=new string('0',64) });
                }
                await db.SaveChangesAsync();
                return (cls.Id,sessions[0].Id);
            }

            (startId,sessionId)=await Create("MO",now.Date.AddHours(8),6);
            (expiredId,_)=await Create("HET",now.Date.AddDays(2).AddHours(8),5,true);
            (cancelId,_)=await Create("HUY",now.Date.AddDays(10).AddHours(8),0);
            (raceA,_)=await Create("A",now.Date.AddDays(12).AddHours(8),0);
            (raceB,_)=await Create("B",now.Date.AddDays(14).AddHours(8),0);
            (raceC,_)=await Create("C",now.Date.AddDays(16).AddHours(8),0);
            var (legacyClass,legacySession)=await Create("CU",now.Date.AddHours(8),6);
            legacyId=legacyClass;
            var legacy=await db.LopHoc.SingleAsync(x=>x.Id==legacyClass);
            legacy.TrangThai="DANG_TUYEN_SINH";
            db.PhieuKho.Add(new() { MaPhieu="KHO-TEST-CU", LoaiPhieu="XUAT_TIEU_HAO", BuoiHocId=legacySession,
                TrangThai="DA_GHI_SO", NguoiLap=admin.Id, LapLuc=now.AddDays(-1) });
            await db.SaveChangesAsync();
        }

        async Task<JsonElement> Update(long id, object body)
        {
            var path = $"api/quan-ly/lop-hoc/{id}";
            var row = await api.Read(path);
            return await api.Put(path,body,Text(row,"rowVersion"));
        }
        async Task<(decimal stock,decimal held,decimal borrowed)> Totals(long material)
        {
            var rows=(await api.Read("api/kho/ton")).EnumerateArray().Where(x=>Id(x)==material&&Text(x,"maLo")!="HET").ToArray();
            decimal Sum(string key)=>rows.Sum(x=>ArtApiClient.Get(x,key).GetDecimal());
            return (Sum("taiKho"),Sum("duTru"),Sum("dangMuon"));
        }
        async Task ReleaseExpired()
        {
            using var scope=services.CreateScope();
            await scope.ServiceProvider.GetRequiredService<ClassInventoryService>().ReleaseExpired(default);
        }
        async Task<bool> Reject(Func<Task> action,string message)
        {
            try { await action(); return false; }
            catch(InvalidOperationException ex)
            {
                if(!ex.Message.Contains(message)) Console.WriteLine($"Expected {message}, received: {ex.Message}");
                return ex.Message.Contains(message);
            }
        }
        async Task<JsonElement> Slip(long id,string type) => (await api.Read("api/kho/phieu")).EnumerateArray()
            .Single(x=>Text(x,"lopHocId")==id.ToString()&&Text(x,"loaiPhieu")==type);
        async Task<JsonElement> CreateSlip(string code, string type, object[] lines, long? session=null, long? source=null) =>
            await api.Post("api/kho/phieu",new{MaPhieu=code,LoaiPhieu=type,BuoiHocId=session,PhieuThamChieuId=source,ChiTiet=lines});
        async Task Approve(JsonElement slip) => await api.Post($"api/kho/phieu/{Id(slip)}/duyet",new{RowVersion=Text(slip,"rowVersion")});

        check(await Totals(paperId)==(100m,0m,0m),"KHO01 Lớp nháp chưa giữ hoặc xuất kho");
        await Update(startId,new{TrangThai="DANG_TUYEN_SINH"});
        check(await Totals(paperId)==(100m,33m,0m)&&await Totals(brushId)==(30m,8m,0m),
            "KHO02 Lớp 6/8 giữ 33 tờ giấy cho hai buổi và 8 cọ dùng lại; tồn thực giữ nguyên");
        var held=await Slip(startId,"DU_TRU");
        var detail=await api.Read($"api/kho/phieu/{Id(held)}");
        var paperLines=ArtApiClient.Get(detail,"chiTiet").EnumerateArray().Where(x=>Text(x,"hoaCuId")==paperId.ToString()).ToList();
        check(paperLines.Count==2&&paperLines.Any(x=>Text(x,"maLo")=="SOM"&&ArtApiClient.Get(x,"soLuong").GetDecimal()==20)
            &&paperLines.All(x=>Text(x,"maLo")!="HET"),"KHO03 Chia dự trữ theo lô hợp lệ, lấy lô hạn gần trước và bỏ lô hết hạn");
        check(await Reject(()=>Update(startId,new{SiSoToiDa=10}),"sĩ số đã chốt"),"KHO04 Sĩ số đã công bố không sửa trực tiếp");
        check(await Reject(()=>api.Post($"api/kho/phieu/{Id(held)}/huy-du-tru",new{RowVersion=Text(held,"rowVersion")}),"Dự trữ tự động"),
            "KHO05 Không giải phóng nhầm phiếu tự động qua chức năng hủy dự trữ tay");
        var duplicate=await CreateSlip("KHO-TEST-TRUNG","XUAT_TIEU_HAO",[new{HoaCuId=paperId,MaLo="MUON",SoLuong=1,DonGia=0}],sessionId);
        check(await Reject(()=>Approve(duplicate),"tránh giữ hoặc xuất trùng"),"KHO06 Chặn phiếu xuất theo buổi trùng với dự trữ của lớp");
        await Update(expiredId,new{TrangThai="DANG_TUYEN_SINH"});
        await ReleaseExpired();
        check(await Totals(paperId)==(100m,33m,0m)&&Text(await Slip(expiredId,"DU_TRU"),"trangThai")=="DA_HUY",
            "KHO07 Hết tuyển sinh với 5 đã xác nhận và 1 chờ tiền thì trả 33 tờ giấy về khả dụng");
        await ReleaseExpired();
        check(await Totals(brushId)==(30m,8m,0m)&&Text(await Slip(startId,"DU_TRU"),"trangThai")=="DA_DUYET",
            "KHO08 Chạy lại tác vụ không cộng tồn lần hai và giữ đủ 8 cọ cho lớp đạt chỉ tiêu");
        await Update(startId,new{TrangThai="DANG_HOC"});
        check(await Totals(paperId)==(67m,0m,0m)&&await Totals(brushId)==(22m,0m,8m),
            "KHO09 Khai giảng 6/8 xuất đủ 33 tờ và cấp 8 cọ, giải phóng dự trữ đúng một lần");
        await Update(startId,new{TenLop="Lớp kho đổi tên",TrangThai="DANG_HOC"});
        check(await Totals(paperId)==(67m,0m,0m)&&await Totals(brushId)==(22m,0m,8m),"KHO10 Sửa lớp đang học không xuất thêm");
        var loan=await Slip(startId,"CAP_DUNG_CHUNG");
        var loanDetail=await api.Read($"api/kho/phieu/{Id(loan)}");
        var returnSlip=await CreateSlip("KHO-TEST-TRA","TRA_DUNG_CHUNG",
            [new{HoaCuId=brushId,SoLuong=8,DonGia=0,ChiTietThamChieuId=Id(ArtApiClient.Get(loanDetail,"chiTiet")[0])}],source:Id(loan));
        await Approve(returnSlip);
        check(await Totals(brushId)==(30m,0m,0m),"KHO11 Nhận trả 8 cọ theo phiếu cấp, tồn và đang mượn khớp");
        await Update(cancelId,new{TrangThai="DANG_TUYEN_SINH"});
        await api.Post($"api/van-hanh-lop/{cancelId}/huy",new{LyDo="Hủy lớp kiểm thử trước khai giảng"});
        check(await Totals(paperId)==(67m,0m,0m)&&Text(await Slip(cancelId,"DU_TRU"),"trangThai")=="DA_HUY",
            "KHO12 Hủy lớp trước khai giảng giải phóng giữ kho và không tạo nhập giả");
        if (dbIsSql())
        {
            // Keep 34 sheets available so two concurrent requests compete for the last allocation.
            await Update(raceA,new{TrangThai="DANG_TUYEN_SINH"});
            var failures = await Task.WhenAll(
                Reject(()=>Update(raceB,new{TrangThai="DANG_TUYEN_SINH"}),"Thiếu"),
                Reject(()=>Update(raceC,new{TrangThai="DANG_TUYEN_SINH"}),"Thiếu"));
            var paperTotal=await Totals(paperId);
            var brushTotal=await Totals(brushId);
            check(failures.Count(x=>x)==1&&paperTotal==(67m,66m,0m)&&brushTotal==(30m,16m,0m),
                "KHO13 Hai lớp tranh cùng lượng tồn: chỉ một lớp giữ được, không âm và không giữ dở dang");
        }
        var beforeLegacy=await Totals(paperId);
        var openedLegacy=await Update(legacyId,new{TrangThai="DANG_HOC"});
        check(Text(openedLegacy,"trangThai")=="DANG_HOC"&&await Totals(paperId)==beforeLegacy&&
            !(await api.Read("api/kho/phieu")).EnumerateArray().Any(x=>Text(x,"lopHocId")==legacyId.ToString()),
            "KHO14 Lớp cũ đã dùng phiếu theo buổi tiếp tục khai giảng mà không bị xuất trùng");
        bool dbIsSql()=>options.Extensions.Any(e=>e.GetType().Name.Contains("SqlServer"));
    }
}
