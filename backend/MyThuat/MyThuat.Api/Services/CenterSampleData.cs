using System.Data;
using System.Text.Json;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using MyThuat.Api.Data;
using MyThuat.Api.Models;

namespace MyThuat.Api.Services;

// Bộ dữ liệu hư cấu, chỉ nạp bằng lệnh riêng vào database trống.
public static class CenterSampleData
{
    public const string Password = "LhlArt2026!HocTap";
    const string Marker = "LHL_CENTER_V1";
    public static async Task Seed(MyThuatDbContext db)
    {
        await using var tx = await db.Database.BeginTransactionAsync(IsolationLevel.Serializable);
        if (await db.NhatKyThayDoi.AnyAsync(x => x.LoaiDoiTuong == Marker))
        {
            Console.WriteLine("Bộ dữ liệu trung tâm đã có. Không thêm trùng hoặc đặt lại mật khẩu.");
            return;
        }
        foreach (var type in db.Model.GetEntityTypes())
        {
            var table = type.GetTableName()!;
            // Tên bảng lấy từ EF model, không nhận từ người dùng.
            await using var command = db.Database.GetDbConnection().CreateCommand();
            command.Transaction = Microsoft.EntityFrameworkCore.Storage.DbContextTransactionExtensions.GetDbTransaction(tx);
            command.CommandText = $"SELECT COUNT(*) FROM [{table}]";
            if (Convert.ToInt64(await command.ExecuteScalarAsync()) > 0)
                throw new InvalidOperationException("Chỉ nạp bộ mẫu vào database trống. Dùng database MyThuatCenter riêng để giữ dữ liệu hiện có.");
        }
        var now = BusinessClock.Now;
        var saturday = BusinessClock.Today;
        while (saturday.DayOfWeek != DayOfWeek.Saturday) saturday = saturday.AddDays(-1);
        var hasher = new PasswordHasher<TaiKhoan>();
        TaiKhoan Account(string username, string role, string name, long? teacher = null, long? member = null)
        {
            var a = new TaiKhoan { TenDangNhap = username, VaiTroChinh = role, HoTenNhanVien = name,
                GiaoVienId = teacher, ThanhVienId = member, TrangThai = "HOAT_DONG", PhienBanBaoMat = 1 };
            a.PasswordHash = hasher.HashPassword(a, Password); db.TaiKhoan.Add(a); return a;
        }
        var admin = Account("quantri", "QUAN_TRI", "Trần Hoàng Anh");
        var office = Account("hocvu", "HOC_VU", "Nguyễn Thảo Vy");
        var cashier = Account("thungan", "THU_NGAN", "Lê Ngọc Hân");
        var warehouse = Account("quanlykho", "QUAN_LY_KHO", "Phạm Quốc Bảo");
        await db.SaveChangesAsync();
        var rule = new QuyDinh { MaBoQuyDinh = "QD_LHL", SoPhienBan = 1, HieuLucTu = now.AddYears(-1),
            NguoiDuyet = admin.Id, TrangThai = "HOAT_DONG", NoiDungJson = JsonSerializer.Serialize(new PolicySettings()) };
        db.QuyDinh.Add(rule);
        var core = new LoaiKhoaHoc { MaLoai = "NEN_TANG", TenLoai = "Mỹ thuật nền tảng", MoTa = "Lộ trình từ làm quen đến phát triển tư duy tạo hình.", TrangThai = "HOAT_DONG" };
        var creative = new LoaiKhoaHoc { MaLoai = "CHAT_LIEU", TenLoai = "Khám phá chất liệu", MoTa = "Thực hành hội họa với chất liệu chuyên biệt.", TrangThai = "HOAT_DONG" };
        db.AddRange(core, creative);
        string[] teacherNames = ["Nguyễn Hoàng Minh", "Trần Ngọc Mai", "Lê Thanh Trúc", "Phạm Quang Huy", "Võ Khánh Linh", "Đặng Bảo Châu", "Nguyễn Thùy Dương"];
        string[] levels = ["JUNIOR", "FOUNDATION", "PRE_BASIC", "BASIC", "INTERMEDIATE", "ACRYLIC"];
        var teachers = teacherNames.Select((name, i) => new GiaoVien { MaGiaoVien = $"GV{i+1:000}", HoTen = name,
            DienThoai = $"000100{i+1:0000}", Email = $"giaovien{i+1:00}@example.invalid", TrinhDo = "Cử nhân Mỹ thuật",
            ChuyenMon = i == 5 ? "Hội họa acrylic và bố cục" : "Tạo hình, màu sắc và hướng dẫn sáng tạo", CapDoCoTheDay = levels[i % 6], TrangThai = "HOAT_DONG" }).ToArray();
        string[] roomNames = ["Mầm màu", "Nét cọ", "Hình khối", "Bảng màu", "Ánh sáng", "Xưởng acrylic"];
        var rooms = roomNames.Select((name, i) => new PhongHoc { MaPhong = $"P{i+1:00}", TenPhong = "Studio " + name, SucChua = 12, TrangThai = "HOAT_DONG" }).ToArray();
        db.GiaoVien.AddRange(teachers); db.PhongHoc.AddRange(rooms); await db.SaveChangesAsync();
        var teacherAccounts = teachers.Select((t, i) => Account($"giaovien{i+1:00}", "GIAO_VIEN", t.HoTen, t.Id)).ToArray();
        var suppliers = new[] { "Họa phẩm Sắc Việt", "Giấy và mỹ thuật An Nhiên", "Xưởng khung Mộc" }.Select((name, i) => new NhaCungCap {
            MaNCC = $"NCC{i+1:00}", TenNCC = name, DienThoai = $"000200{i+1:0000}", TrangThai = "HOAT_DONG" }).ToArray();
        db.NhaCungCap.AddRange(suppliers); await db.SaveChangesAsync();
        string[] materialNames = ["Giấy vẽ A3", "Giấy màu thủ công", "Màu nước 12 màu", "Màu acrylic trắng", "Màu acrylic xanh", "Cọ tròn số 6", "Cọ bản số 12", "Bảng pha màu", "Giá vẽ gỗ", "Canvas 30 × 40 cm", "Bộ chì phác thảo", "Sổ vẽ A4"];
        decimal[] prices = [5000, 2000, 95000, 65000, 65000, 22000, 35000, 18000, 280000, 60000, 75000, 45000];
        var materials = materialNames.Select((name, i) => new HoaCu { MaHoaCu = $"HC{i+1:000}", TenHoaCu = name,
            QuyCach = i < 2 ? "Định lượng 200 gsm" : "Quy cách tiêu chuẩn của xưởng", DonViCoSo = i < 2 ? "Tờ" : i is 2 or 10 ? "Bộ" : "Cái",
            NhomHoaCu = i >= 9 ? "BAN_RIENG" : i >= 5 ? "DUNG_CHUNG" : "TIEU_HAO", GiaBanThamKhao = prices[i],
            MucDatHang = i < 2 ? 200 : 15, TrangThai = "HOAT_DONG" }).ToArray();
        db.HoaCu.AddRange(materials); await db.SaveChangesAsync();
        var receipt = new PhieuKho { MaPhieu = "NK-0001", LoaiPhieu = "NHAP", NhaCungCapId = suppliers[0].Id,
            LapLuc = saturday.AddDays(-180).ToDateTime(new TimeOnly(9,0)), DuyetLuc = saturday.AddDays(-180).ToDateTime(new TimeOnly(10,0)),
            NguoiLap = warehouse.Id, NguoiDuyet = admin.Id, TrangThai = "DA_GHI_SO", LyDo = "Nhập họa cụ đầu kỳ" };
        db.PhieuKho.Add(receipt); await db.SaveChangesAsync();
        foreach (var (m, i) in materials.Select((m,i)=>(m,i))) db.ChiTietPhieuKho.Add(new() { PhieuKhoId = receipt.Id, HoaCuId = m.Id,
            MaLo = $"LO-{i+1:000}", SoLuong = i < 2 ? 10000 : i >= 5 && i < 9 ? 100 : 500, DonGiaChot = prices[i] * .7m });
        string[] courses = ["Junior — Mầm sáng tạo", "Foundation 1 — Nền tảng tạo hình", "Pre-Basic 1 — Nét và khối", "Basic 1 — Bố cục và sắc màu", "Intermediate 1 — Kể chuyện bằng tranh", "Acrylic — Phong cảnh trên canvas"];
        short[] ages = [4, 6, 7, 9, 12, 16]; decimal[] fees = [3200000, 3600000, 3800000, 4000000, 4400000, 2400000];
        string[] topics = ["Làm quen chất liệu", "Quan sát và phác hình", "Đường nét và nhịp điệu", "Hình khối và ánh sáng", "Pha màu cơ bản", "Màu nóng và màu lạnh", "Bố cục chủ thể", "Bài thực hành tổng hợp", "Tĩnh vật quanh em", "Cây và hoa", "Phong cảnh phố", "Thế giới động vật", "Nhân vật trong tranh", "Ý tưởng cá nhân", "Hoàn thiện tác phẩm", "Trưng bày và chia sẻ"];
        string[] surnames = ["Nguyễn", "Trần", "Lê", "Phạm", "Võ", "Đặng"];
        string[][] pupilNames = [["Bảo An", "Minh Khang", "Ngọc Hà", "Khánh Vy", "Gia Huy", "Thảo Nhi"], ["Tuấn Kiệt", "Hà My", "Đức Anh", "Phương Linh", "Hải Đăng", "Quỳnh Anh"], ["Minh Châu", "Bảo Ngọc", "Anh Khoa", "Hoàng Nam", "Thanh Vy", "Phúc An"]];
        string[][] parentNames = [["Thu Minh", "Hoàng Phúc", "Thanh Tâm"], ["Hồng Hạnh", "Quốc Dũng", "Ngọc Diệp"], ["Thanh Hoa", "Văn Khánh", "Phương Thảo"]];
        int memberNumber = 0, pupilNumber = 0, registrationNumber = 0, slipNumber = 1;
        for (int c = 0; c < courses.Length; c++)
        {
            short length = (short)(c == 5 ? 8 : 16), periods = (short)(c == 0 ? 3 : 4);
            var course = new KhoaHoc { MaKhoa = $"KH{c+1:00}", TenKhoa = courses[c], LoaiKhoaHocId = c == 5 ? creative.Id : core.Id,
                PhienBan = 1, CapDo = levels[c], TuoiToiThieu = ages[c], SoBuoi = length, SoTietMoiBuoi = periods, PhutMoiTiet = 30,
                HocPhi = fees[c], HieuLucTu = saturday.AddDays(-180), TrangThai = "HOAT_DONG", YeuCauDauVao = "Đánh giá đầu vào theo cấp độ trước khi xếp lớp.",
                MucTieu = "Phát triển khả năng quan sát, bố cục và cảm nhận màu sắc; hoàn thiện tác phẩm thể hiện ý tưởng cá nhân." };
            db.KhoaHoc.Add(course); await db.SaveChangesAsync();
            var contents = Enumerable.Range(1,length).Select(n => new NoiDungBuoi { KhoaHocId = course.Id, ThuTu = (short)n,
                ChuDe = topics[n-1], NoiDung = "Quan sát mẫu, trao đổi ý tưởng, thực hành và nhận xét bài vẽ.", SoTiet = periods,
                YeuCauSanPham = "Một bài thực hành hoàn chỉnh theo chủ đề của buổi học." }).ToArray();
            db.NoiDungBuoi.AddRange(contents); await db.SaveChangesAsync();
            foreach (var topic in contents) db.DinhMucHoaCu.Add(new() { NoiDungBuoiId = topic.Id, HoaCuId = materials[0].Id, CoSoTinh = "HOC_VIEN", SoLuong = 2 });
            for (int cohort = 0; cohort < 3; cohort++)
            {
                var first = saturday.AddDays(cohort == 0 ? -154 : cohort == 1 ? -21 : 21).ToDateTime(new TimeOnly(cohort == 2 ? 14 : 9,0));
                var last = first.AddDays((length-1)*7);
                var cls = new LopHoc { MaLop = $"L{c+1:00}-{cohort+1:00}", TenLop = courses[c].Split('—')[0].Trim()+$" · {first:MM/yyyy}",
                    KhoaHocId = course.Id, NgayKhaiGiangDuKien = DateOnly.FromDateTime(first), NgayKhaiGiangThucTe = cohort < 2 ? DateOnly.FromDateTime(first) : null,
                    NgayKetThucDuKien = DateOnly.FromDateTime(last), SoBuoiKeHoach = length, SiSoToiThieu = 6, SiSoToiDa = 12,
                    HocPhiApDung = fees[c], MoDangKyLuc = first.AddDays(-35), DongDangKyLuc = first.AddHours(-48), NguoiDuyet = admin.Id,
                    LichHocDuKien = $"Thứ Bảy · {first:HH:mm}–{first.AddMinutes(periods*30):HH:mm}", TrangThai = cohort == 0 ? "DA_KET_THUC" : cohort == 1 ? "DANG_HOC" : "DANG_TUYEN_SINH" };
                db.LopHoc.Add(cls); await db.SaveChangesAsync();
                var sessions = contents.Select((topic,n) => new BuoiHoc { LopHocId = cls.Id, NoiDungBuoiId = topic.Id, PhongHocId = rooms[c].Id,
                    ThuTuTrongLop = (short)(n+1), LanXepLich = 1, BatDau = first.AddDays(n*7), KetThuc = first.AddDays(n*7).AddMinutes(periods*30),
                    SoTiet = periods, PhutMoiTiet = 30, TrangThai = first.AddDays(n*7).AddMinutes(periods*30) < now ? "DA_TO_CHUC" : "DA_XEP_LICH" }).ToArray();
                db.BuoiHoc.AddRange(sessions); await db.SaveChangesAsync();
                db.PhanCong.Add(new() { LopHocId = cls.Id, GiaoVienId = teachers[c].Id, VaiTroTrongLop = "CHINH", TuNgay = DateOnly.FromDateTime(first),
                    DenNgay = DateOnly.FromDateTime(last), XacNhanDuChuyenMon = true, XacNhanNhanLopLuc = first.AddDays(-30), NguoiDuyet = admin.Id });
                if (c == 0) db.PhanCong.Add(new() { LopHocId = cls.Id, GiaoVienId = teachers[6].Id, VaiTroTrongLop = "TRO_GIANG", TuNgay = DateOnly.FromDateTime(first), DenNgay = DateOnly.FromDateTime(last), XacNhanDuChuyenMon = true, XacNhanNhanLopLuc = first.AddDays(-30), NguoiDuyet = admin.Id });
                foreach (var session in sessions)
                {
                    if (c == 0) db.GiangDayBuoi.Add(new() { BuoiHocId = session.Id, GiaoVienId = teachers[6].Id, VaiTro = "TRO_GIANG", SoTietThucDay = periods, NguoiDuyet = admin.Id });
                    db.GiangDayBuoi.Add(new() { BuoiHocId = session.Id, GiaoVienId = teachers[c].Id, VaiTro = "CHINH", SoTietThucDay = periods, NguoiDuyet = admin.Id });
                    if (session.TrangThai == "DA_TO_CHUC")
                    {
                        var slip = new PhieuKho { MaPhieu = $"XK-{++slipNumber:0000}", LoaiPhieu = "XUAT_TIEU_HAO", BuoiHocId = session.Id,
                            LapLuc = session.BatDau.AddHours(-1), DuyetLuc = session.BatDau.AddMinutes(-30), NguoiLap = warehouse.Id, NguoiDuyet = admin.Id, NguoiNhan = teacherAccounts[c].Id, TrangThai = "DA_GHI_SO", LyDo = "Giấy thực hành theo sĩ số lớp" };
                        db.PhieuKho.Add(slip); await db.SaveChangesAsync();
                        db.ChiTietPhieuKho.Add(new() { PhieuKhoId = slip.Id, HoaCuId = materials[0].Id, MaLo = "LO-001", SoLuong = 12, DonGiaChot = 3500 });
                    }
                }
                ThanhVien? parent = null;
                for (int s = 0; s < 6; s++)
                {
                    if (s%2 == 0)
                    {
                        memberNumber++;
                        parent = new ThanhVien { MaThanhVien = $"TV{memberNumber:000}", HoTen = surnames[c]+" "+parentNames[cohort][s/2],
                            DienThoai = $"000300{memberNumber:0000}", Email = $"phuhuynh{memberNumber:00}@example.invalid", DiaChi = "Tân Phú, TP. Hồ Chí Minh",
                            XacNhanThanhVienLuc = first.AddDays(-40), TrangThai = "HOAT_DONG" };
                        db.ThanhVien.Add(parent); await db.SaveChangesAsync();
                        Account($"phuhuynh{memberNumber:00}","THANH_VIEN",parent.HoTen,member:parent.Id);
                    }
                    pupilNumber++;
                    var pupil = new HocVien { MaHocVien = $"HV{pupilNumber:000}", HoTen = surnames[c]+" "+pupilNames[cohort][s], NgaySinh = DateOnly.FromDateTime(first).AddYears(-ages[c]-2).AddDays(-s*19),
                        LienHeKhanCapTen = parent!.HoTen, LienHeKhanCapSDT = parent.DienThoai, TrangThai = "HOAT_DONG", GiaoVienDanhGia = teachers[c].Id,
                        NgayDanhGiaDauVao = DateOnly.FromDateTime(first.AddDays(-30)), CapDoDeXuat = levels[c], NhanXetDauVao = "Phù hợp cấp độ; cần luyện nét và tự tin trình bày ý tưởng.", MucTieuHoc = "Nâng cao kỹ năng vẽ và khả năng sáng tạo." };
                    // Đánh giá cho lớp tuyển sinh phải được thực hiện trước thời điểm hiện tại.
                    if (pupil.NgayDanhGiaDauVao > BusinessClock.Today) pupil.NgayDanhGiaDauVao = BusinessClock.Today.AddDays(-1);
                    db.HocVien.Add(pupil); await db.SaveChangesAsync();
                    db.DaiDienHocVien.Add(new() { ThanhVienId = parent.Id, HocVienId = pupil.Id, QuanHe = s/2 == 1 ? "CHA" : "ME", QuyenDaiDien = "DAY_DU", LaLienHeChinh = true,
                        HieuLucTu = first.AddDays(-40), XacNhanLuc = first.AddDays(-40), NguoiXacNhan = office.Id });
                    if (cohort == 1 && s == 0)
                    {
                        var studentMember = new ThanhVien { MaThanhVien = $"SV{c+1:00}", HoTen = pupil.HoTen, DienThoai = parent.DienThoai, TrangThai = "HOAT_DONG", XacNhanThanhVienLuc = now.AddDays(-30) };
                        db.ThanhVien.Add(studentMember); await db.SaveChangesAsync();
                        Account($"hocvien{c+1:00}","THANH_VIEN",pupil.HoTen,member:studentMember.Id);
                        db.DaiDienHocVien.Add(new() { ThanhVienId = studentMember.Id, HocVienId = pupil.Id, QuanHe = "TU_BAN_THAN", QuyenDaiDien = "XEM_KET_QUA",
                            HieuLucTu = first.AddDays(-30), XacNhanLuc = first.AddDays(-30), NguoiXacNhan = office.Id });
                    }
                    var profile = new HoSoTheoHoc { HocVienId = pupil.Id, KhoaHocId = course.Id, QuyDinhId = rule.Id, NgayBatDau = DateOnly.FromDateTime(first), TrangThai = cohort == 0 ? "HOAN_THANH" : "DANG_THEO_HOC" };
                    db.HoSoTheoHoc.Add(profile); await db.SaveChangesAsync();
                    registrationNumber++;
                    bool pending = cohort == 2 && s == 4, expired = cohort == 2 && s == 5;
                    var registered = cohort < 2 ? first.AddDays(-14) : pending ? now.AddHours(-2) : expired ? now.AddDays(-2) : now.AddDays(-3);
                    var enrollment = new DangKy { MaDangKy = $"DK{registrationNumber:0000}", HoSoTheoHocId = profile.Id, LopHocId = cls.Id, NguoiDangKyId = parent.Id,
                        LapLuc = registered, HanGiuCho = registered.AddHours(24), HieuLucTu = pending || expired ? null : registered.AddHours(1),
                        HieuLucDen = expired ? registered.AddHours(24) : null, HocPhiGocChot = fees[c], TrangThai = pending ? "CHO_THANH_TOAN" : expired ? "HET_HAN" : "DA_XAC_NHAN",
                        IdempotencyKey = $"center-enroll-{registrationNumber}", RequestFingerprint = $"center-{registrationNumber}" };
                    db.DangKy.Add(enrollment); await db.SaveChangesAsync();
                    var charge = new KhoanThuDangKy { DangKyId = enrollment.Id, LoaiKhoan = "HOC_PHI", MoTaChot = course.TenKhoa, SoLuong = 1, DonGiaChot = fees[c], ThanhTienChot = fees[c] };
                    db.KhoanThuDangKy.Add(charge); await db.SaveChangesAsync();
                    if (!expired) db.GiaoDichTien.Add(new() { DangKyId = enrollment.Id, KhoanThuId = charge.Id, MaChungTu = $"PT{registrationNumber:0000}", LoaiGiaoDich = "THU",
                        PhuongThuc = s%2==0 ? "TIEN_MAT" : "CHUYEN_KHOAN", SoTien = pending ? fees[c]/2 : fees[c], ThoiDiem = registered.AddHours(1),
                        NguoiXacNhan = cashier.Id, TrangThai = "DA_XAC_NHAN", IdempotencyKey = $"center-receipt-{registrationNumber}", RequestFingerprint = $"center-receipt-{registrationNumber}" });
                    if (s % 3 == 0 && !pending && !expired)
                    {
                        var materialCharge = new KhoanThuDangKy { DangKyId = enrollment.Id, LoaiKhoan = "HOA_CU", HoaCuId = materials[9].Id,
                            MoTaChot = materials[9].TenHoaCu, SoLuong = 1, DonGiaChot = 60000, ThanhTienChot = 60000, SoLuongDaGiao = 1 };
                        db.KhoanThuDangKy.Add(materialCharge); await db.SaveChangesAsync();
                        db.GiaoDichTien.Add(new() { DangKyId = enrollment.Id, KhoanThuId = materialCharge.Id, MaChungTu = $"PT-HC{registrationNumber:0000}",
                            LoaiGiaoDich = "THU", PhuongThuc = "TIEN_MAT", SoTien = 60000, ThoiDiem = registered.AddHours(1), NguoiXacNhan = cashier.Id,
                            TrangThai = "DA_XAC_NHAN", IdempotencyKey = $"center-sale-{registrationNumber}", RequestFingerprint = $"center-sale-{registrationNumber}" });
                        var sale = new PhieuKho { MaPhieu = $"BH-{registrationNumber:0000}", LoaiPhieu = "BAN_RIENG", LapLuc = registered.AddHours(2),
                            DuyetLuc = registered.AddHours(2), NguoiLap = warehouse.Id, NguoiDuyet = admin.Id, TrangThai = "DA_GHI_SO", LyDo = "Giao canvas đã thanh toán" };
                        db.PhieuKho.Add(sale); await db.SaveChangesAsync();
                        db.ChiTietPhieuKho.Add(new() { PhieuKhoId = sale.Id, HoaCuId = materials[9].Id, MaLo = "LO-010", KhoanThuDangKyId = materialCharge.Id, SoLuong = 1, DonGiaChot = 60000 });
                    }
                    var attendance = sessions.Select((session,n) => new DiemDanh { DangKyId = enrollment.Id, BuoiHocId = session.Id, NoiDungDuocTinhId = session.NoiDungBuoiId, LoaiThamGia = "HOC_CHINH",
                        TrangThai = expired ? "HUY_BO" : session.TrangThai == "DA_TO_CHUC" ? s==1&&n==1 ? "VANG" : "CO_MAT" : "CHO_THAM_GIA",
                        PhutThamDu = session.TrangThai == "DA_TO_CHUC" ? (short?)(s==1&&n==1 ? 0 : periods*30) : null,
                        DiemSanPham = session.TrangThai == "DA_TO_CHUC" && !(s==1&&n==1) ? 7m+(s%3)*.5m : null,
                        NhanXet = session.TrangThai == "DA_TO_CHUC" ? s==1&&n==1 ? "Vắng có báo trước." : "Bố cục rõ, phối màu hài hòa; luyện thêm nét ở phần chi tiết." : null,
                        BaoNghiLuc = s==1&&n==1 ? session.BatDau < now ? session.BatDau.AddHours(-24) : null : null,
                        GhiLuc = session.TrangThai == "DA_TO_CHUC" ? session.KetThuc : null, NguoiGhi = session.TrangThai == "DA_TO_CHUC" ? teacherAccounts[c].Id : null }).ToArray();
                    db.DiemDanh.AddRange(attendance);
                    if (cohort == 0) db.KetQuaKhoaHoc.Add(new() { HoSoTheoHocId = profile.Id, SoBuoiHopLeChot = (short)(length-(s==1?1:0)), DiemCuoiKhoaChot = 7m+(s%3)*.5m,
                        KetLuan = "DAT", NhanXetTongKet = "Hoàn thành yêu cầu khóa học; tiếp tục phát triển ý tưởng cá nhân.", NguoiDuyet = office.Id, ChotLuc = last.AddDays(2), SoXacNhan = $"XN{registrationNumber:0000}", NgayCap = DateOnly.FromDateTime(last.AddDays(2)) });
                    if (cohort == 1 && s < 2) db.PhanHoi.Add(new() { DangKyId = enrollment.Id, NguoiGuiId = parent.Id, GuiLuc = now.AddDays(-1), HanPhanHoi = now.AddDays(1),
                        NoiDung = s==0 ? "Nhờ giáo viên gợi ý bài luyện nét để con thực hành thêm ở nhà." : "Gia đình muốn được tư vấn lịch học bù cho buổi con đã báo nghỉ.",
                        TrangThai = s==0 ? "DA_TRA_LOI" : "CHO_XU_LY", NguoiXuLy = s==0 ? office.Id : null,
                        TraLoi = s==0 ? "Con có thể luyện nét cong và phác đồ vật 15 phút mỗi ngày. Giáo viên sẽ nhận xét bài trong buổi tới." : null, TraLoiLuc = s==0 ? now.AddHours(-12) : null });
                    await db.SaveChangesAsync();
                }
            }
            // Hồ sơ sẵn sàng để kiểm tra đăng ký mới từ website.
            var prospectParent = await db.ThanhVien.SingleAsync(x => x.MaThanhVien == $"TV{c*9+1:000}");
            var prospect = new HocVien { MaHocVien = $"HVNEW{c+1:00}", HoTen = surnames[c]+" Tuệ An", NgaySinh = BusinessClock.Today.AddYears(-ages[c]-3),
                LienHeKhanCapTen = prospectParent.HoTen, LienHeKhanCapSDT = prospectParent.DienThoai, TrangThai = "HOAT_DONG", GiaoVienDanhGia = teachers[c].Id,
                NgayDanhGiaDauVao = BusinessClock.Today.AddDays(-1), CapDoDeXuat = levels[c], NhanXetDauVao = "Sẵn sàng tham gia cấp độ đề xuất." };
            db.HocVien.Add(prospect); await db.SaveChangesAsync();
            var parentId = prospectParent.Id;
            db.DaiDienHocVien.Add(new() { ThanhVienId = parentId, HocVienId = prospect.Id, QuanHe = "ME", QuyenDaiDien = "DAY_DU", LaLienHeChinh = true,
                HieuLucTu = now.AddDays(-1), XacNhanLuc = now.AddDays(-1), NguoiXacNhan = office.Id });
        }
        for (int c = 0; c < 6; c++)
        {
            var absent = await db.DiemDanh.Include(x=>x.BuoiHoc).Include(x=>x.DangKy).ThenInclude(x=>x!.LopHoc)
                .SingleAsync(x=>x.DangKy!.LopHoc!.MaLop==$"L{c+1:00}-02" && x.TrangThai=="VANG");
            var target = await db.BuoiHoc.SingleAsync(x=>x.LopHoc!.MaLop==$"L{c+1:00}-03" && x.ThuTuTrongLop==2);
            var makeup = new DiemDanh { DangKyId=absent.DangKyId, BuoiHocId=target.Id, NoiDungDuocTinhId=absent.NoiDungDuocTinhId,
                LoaiThamGia="HOC_BU", TrangThai="CHO_THAM_GIA" };
            db.DiemDanh.Add(makeup); await db.SaveChangesAsync();
            db.HocBu.Add(new() { DiemDanhVangId=absent.Id, DiemDanhBuId=makeup.Id, HanHoanTat=absent.DangKy!.LopHoc!.NgayKetThucDuKien!.Value.AddDays(30).ToDateTime(TimeOnly.MaxValue),
                NguoiDuyet=office.Id, TrangThai="DA_DAT", LyDo="Bù nội dung đã báo nghỉ đúng hạn theo lịch tư vấn." });
        }
        db.KhuyenMai.Add(new() { MaKhuyenMai = "NHAPHOC10", TenKhuyenMai = "Ưu đãi nhập học", NhomKhoanThu = "HOC_PHI", LoaiGiam = "PHAN_TRAM", GiaTriGiam = 10,
            TranGiam = 400000, TongLuot = 50, BatDau = now.AddDays(-7), KetThuc = now.AddDays(60), TrangThai = "HOAT_DONG" });
        db.KhuyenMai.Add(new() { MaKhuyenMai = "HOACU15", TenKhuyenMai = "Ưu đãi bộ họa cụ", NhomKhoanThu = "HOA_CU", LoaiGiam = "PHAN_TRAM", GiaTriGiam = 15,
            TranGiam = 50000, TongLuot = 100, BatDau = now.AddDays(-7), KetThuc = now.AddDays(60), TrangThai = "HOAT_DONG" });
        db.NhatKyThayDoi.Add(new() { LoaiDoiTuong = Marker, IdDoiTuong = 0, TaiKhoanId = admin.Id, HanhDong = "NAP_DU_LIEU_MAU", Kenh = "CLI", ThoiDiem = now,
            CorrelationId = Guid.NewGuid().ToString("N"), SauJson = JsonSerializer.Serialize(new { Fictional = true, Courses = 6, Classes = 18, Pupils = 114, Enrollments = 108 }) });
        await db.SaveChangesAsync(); await tx.CommitAsync();
        Console.WriteLine("Đã nạp bộ dữ liệu trung tâm: 6 khóa, 18 lớp, 114 học viên, 108 đăng ký. Tài khoản mẫu xem README.md.");
    }
}
