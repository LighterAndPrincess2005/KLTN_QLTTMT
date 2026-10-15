using System.Data;
using System.Security.Cryptography;
using System.Text.Json;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using MyThuat.Api.Data;
using MyThuat.Api.Models;

namespace MyThuat.Api.Services;

// Explicit CLI command only. These are proposed/example data from tables 1.2 and 1.3,
// not a production seed and never run on normal API startup.
public static class DemoData
{
    public static async Task Seed(MyThuatDbContext db, string contentRoot)
    {
        await using var tx = await db.Database.BeginTransactionAsync(IsolationLevel.Serializable);
        var admin = await db.TaiKhoan.Where(x => x.VaiTroChinh == "QUAN_TRI" && x.TrangThai == "HOAT_DONG")
            .OrderBy(x => x.Id).FirstOrDefaultAsync()
            ?? throw new InvalidOperationException("Tạo quản trị lần đầu trước khi thêm dữ liệu mẫu.");
        if (await db.KhoaHoc.AnyAsync(x => x.MaKhoa == "DEMO_KH01"))
        {
            Console.WriteLine("Bộ mẫu đã có. Giữ nguyên dữ liệu và không thêm trùng.");
            return;
        }

        var core = new LoaiKhoaHoc { MaLoai = "DEMO_CORE", TenLoai = "Core", MoTa = "Nhóm chương trình nền tảng theo tài liệu Word", TrangThai = "HOAT_DONG" };
        var extra = new LoaiKhoaHoc { MaLoai = "DEMO_BO_TRO", TenLoai = "Bổ trợ", MoTa = "Nhóm chương trình bổ trợ minh họa", TrangThai = "HOAT_DONG" };
        var p1 = new PhongHoc { MaPhong = "DEMO_P01", TenPhong = "Studio Sắc Màu", SucChua = 10, TrangThai = "HOAT_DONG" };
        var p2 = new PhongHoc { MaPhong = "DEMO_P02", TenPhong = "Studio Sáng Tạo", SucChua = 10, TrangThai = "HOAT_DONG" };
        var teachers = new[] { "Nguyễn An", "Trần Mai", "Lê Minh", "Khánh Linh" }.Select((name, i) => new GiaoVien {
            MaGiaoVien = $"DEMO_GV{i + 1:00}", HoTen = name + " (mẫu)", DienThoai = "0000000000",
            TrinhDo = "Mỹ thuật — hồ sơ minh họa", ChuyenMon = "Hội họa, sáng tạo thiếu nhi",
            CapDoCoTheDay = "JUNIOR,FOUNDATION,PRE_BASIC,BASIC,INTERMEDIATE,ACRYLIC", TrangThai = "HOAT_DONG"
        }).ToArray();
        db.AddRange(core, extra, p1, p2); db.GiaoVien.AddRange(teachers);
        await db.SaveChangesAsync();

        var names = new[] { "Junior", "Foundation 1", "Pre-Basic 1", "Basic 1", "Intermediate 1", "Acrylic nhập môn" };
        var levels = new[] { "JUNIOR", "FOUNDATION", "PRE_BASIC", "BASIC", "INTERMEDIATE", "ACRYLIC" };
        short[] ages = [3, 6, 7, 9, 12, 9];
        decimal[] fees = [3200000, 3600000, 3800000, 4000000, 4400000, 2400000];
        var goals = new[] {
            "Nhận biết màu, nét và hình; khám phá sáp nến, giấy vẽ và đất nặn an toàn.",
            "Quan sát, thể hiện chủ thể và nền tranh bằng chì phác, sáp dầu và gôm.",
            "Luyện nét, sắc độ và hình khối bằng chì màu, sáp dầu và giấy.",
            "Luyện bố cục, trái cây và con vật bằng chì màu, sáp dầu và bút viền.",
            "Vẽ người và kể chuyện qua tranh bằng màu nước, poster, cọ và giấy dày.",
            "Pha màu, xử lý lớp nền và vẽ phong cảnh trên canvas bằng acrylic."
        };
        var themes = new[] {
            new[] { "Làm quen màu sắc", "Nét thẳng và nét cong", "Hình tròn và hình vuông", "Khu vườn sắc màu", "Thế giới con vật", "Đất nặn và hình khối", "Câu chuyện của em", "Trưng bày tác phẩm" },
            new[] { "Quan sát đồ vật", "Phác hình chủ thể", "Hình khối cơ bản", "Bố cục nền tranh", "Tĩnh vật với sáp dầu", "Phong cảnh gần gũi", "Sắc màu của em", "Hoàn thiện chủ đề" },
            new[] { "Luyện nét cơ bản", "Sắc độ sáng tối", "Dựng khối", "Phối màu chì", "Tĩnh vật đơn giản", "Mảng màu và không gian", "Thực hành quan sát", "Bài tổng hợp" },
            new[] { "Bố cục tranh", "Dựng hình trái cây", "Vẽ con vật", "Nét viền và chi tiết", "Tĩnh vật màu", "Phong cảnh tự chọn", "Nhân vật và không gian", "Hoàn thiện tranh" },
            new[] { "Tỷ lệ cơ thể", "Chân dung", "Dáng người", "Câu chuyện qua tranh", "Màu nước", "Màu poster", "Bố cục nhân vật", "Bài sáng tạo cuối khóa" },
            new[] { "Làm quen acrylic", "Pha màu", "Lớp nền", "Kỹ thuật cọ", "Bố cục phong cảnh", "Chi tiết và sắc độ", "Hoàn thiện canvas", "Trưng bày tác phẩm" }
        };
        var firstSaturday = new DateOnly(2026, 10, 17);
        if (firstSaturday < BusinessClock.Today.AddDays(3)) {
            firstSaturday = BusinessClock.Today.AddDays(14);
            while (firstSaturday.DayOfWeek != DayOfWeek.Saturday) firstSaturday = firstSaturday.AddDays(1);
        }
        var holidays = new HashSet<DateOnly>();
        var rule = await db.QuyDinh.Where(x => x.TrangThai == "HOAT_DONG" && x.HieuLucTu <= BusinessClock.Now
            && (x.HieuLucDen == null || x.HieuLucDen > BusinessClock.Now)).OrderByDescending(x => x.HieuLucTu).FirstOrDefaultAsync();
        if (rule is not null) holidays.UnionWith(PolicySettings.Parse(rule.NoiDungJson).NgayNghi);
        var madeCourses = new List<KhoaHoc>();
        int totalSessions = 0;
        for (var i = 0; i < 6; i++) {
            short count = (short)(i == 5 ? 8 : 16), periods = (short)(i == 0 ? 3 : 4);
            var course = new KhoaHoc { LoaiKhoaHocId = i == 5 ? extra.Id : core.Id, MaKhoa = $"DEMO_KH{i + 1:00}", TenKhoa = names[i],
                PhienBan = 1, CapDo = levels[i], TuoiToiThieu = ages[i], YeuCauDauVao = "Giáo viên đánh giá đầu vào và đề xuất cấp độ phù hợp.",
                MucTieu = goals[i], SoBuoi = count, SoTietMoiBuoi = periods, PhutMoiTiet = 30, HocPhi = fees[i], HieuLucTu = BusinessClock.Today, TrangThai = "HOAT_DONG" };
            db.KhoaHoc.Add(course); await db.SaveChangesAsync(); madeCourses.Add(course);
            var day = firstSaturday.AddDays(i >= 3 ? 1 : 0);
            var hour = i == 1 ? new TimeOnly(9, 45) : i == 5 ? new TimeOnly(10, 15) : new TimeOnly(8, 0);
            var teacher = teachers[i == 0 ? 0 : i is 1 or 3 ? 1 : 2];
            var room = i is 0 or 1 or 3 ? p1 : p2;
            var slot = day.ToDateTime(hour);
            var cls = new LopHoc { KhoaHocId = course.Id, MaLop = $"DEMO_L{i + 1:00}", TenLop = names[i] + " — " + (i < 3 ? "Thứ Bảy" : "Chủ nhật"),
                NgayKhaiGiangDuKien = day, SoBuoiKeHoach = count, LichHocDuKien = (i < 3 ? "Thứ Bảy" : "Chủ nhật") + " · " + hour.ToString("HH:mm") + "–" + hour.AddMinutes(periods * 30).ToString("HH:mm"),
                SiSoToiThieu = 6, SiSoToiDa = (short)(i == 0 ? 8 : 10), HocPhiApDung = fees[i], MoDangKyLuc = BusinessClock.Now.AddDays(-1),
                DongDangKyLuc = slot.AddHours(-48), NguoiDuyet = admin.Id, TrangThai = "DANG_TUYEN_SINH" };
            db.LopHoc.Add(cls); await db.SaveChangesAsync();
            for (short n = 1; n <= count; n++) {
                var topic = new NoiDungBuoi { KhoaHocId = course.Id, ThuTu = n, ChuDe = themes[i][(n - 1) % themes[i].Length] + (n > 8 ? " — thực hành" : ""),
                    NoiDung = goals[i] + " Đề cương buổi mẫu để minh họa chương trình trong báo cáo.", SoTiet = periods, YeuCauSanPham = "Hoàn thiện bài thực hành và chia sẻ ý tưởng." };
                db.NoiDungBuoi.Add(topic); await db.SaveChangesAsync();
                while (holidays.Contains(DateOnly.FromDateTime(slot))) slot = slot.AddDays(7);
                var finish = slot.AddMinutes(periods * 30);
                if (await db.BuoiHoc.AnyAsync(b => b.TrangThai != "DA_HUY" && b.PhongHocId == room.Id && b.BatDau < finish && b.KetThuc > slot))
                    throw new InvalidOperationException("Lịch mẫu trùng phòng. Không ghi bộ mẫu; kiểm tra lịch đang có.");
                var session = new BuoiHoc { LopHocId = cls.Id, NoiDungBuoiId = topic.Id, PhongHocId = room.Id, ThuTuTrongLop = n, LanXepLich = 1,
                    BatDau = slot, KetThuc = finish, SoTiet = periods, PhutMoiTiet = 30, PhutNghi = 0, TrangThai = "DA_XEP_LICH" };
                db.BuoiHoc.Add(session); await db.SaveChangesAsync();
                db.GiangDayBuoi.Add(new() { BuoiHocId = session.Id, GiaoVienId = teacher.Id, VaiTro = "CHINH", SoTietThucDay = periods, NguoiDuyet = admin.Id });
                if (i == 0) db.GiangDayBuoi.Add(new() { BuoiHocId = session.Id, GiaoVienId = teachers[3].Id, VaiTro = "TRO_GIANG", SoTietThucDay = periods, NguoiDuyet = admin.Id });
                cls.NgayKetThucDuKien = DateOnly.FromDateTime(slot); slot = slot.AddDays(7); totalSessions++;
            }
            db.PhanCong.Add(new() { LopHocId = cls.Id, GiaoVienId = teacher.Id, VaiTroTrongLop = "CHINH", TuNgay = day, DenNgay = cls.NgayKetThucDuKien,
                XacNhanDuChuyenMon = true, XacNhanNhanLopLuc = BusinessClock.Now, NguoiDuyet = admin.Id });
            if (i == 0) db.PhanCong.Add(new() { LopHocId = cls.Id, GiaoVienId = teachers[3].Id, VaiTroTrongLop = "TRO_GIANG", TuNgay = day, DenNgay = cls.NgayKetThucDuKien,
                XacNhanDuChuyenMon = true, XacNhanNhanLopLuc = BusinessClock.Now, NguoiDuyet = admin.Id });
            await db.SaveChangesAsync();
        }
        var member = new ThanhVien { MaThanhVien = "DEMO_TV01", HoTen = "Nguyễn Thu Minh (mẫu)", DienThoai = "0000000000", TrangThai = "HOAT_DONG", XacNhanThanhVienLuc = BusinessClock.Now };
        db.ThanhVien.Add(member); await db.SaveChangesAsync();
        var pw = "ArtoraDemo" + Convert.ToHexString(RandomNumberGenerator.GetBytes(8)) + "!";
        var account = new TaiKhoan { TenDangNhap = "demo_artora_" + Convert.ToHexString(RandomNumberGenerator.GetBytes(3)).ToLowerInvariant(), ThanhVienId = member.Id,
            VaiTroChinh = "THANH_VIEN", TrangThai = "HOAT_DONG", PhienBanBaoMat = 1 };
        account.PasswordHash = new PasswordHasher<TaiKhoan>().HashPassword(account, pw);
        db.TaiKhoan.Add(account);
        var children = new[] { ("Nguyễn Bảo An", 8, "FOUNDATION"), ("Nguyễn Minh Khang", 10, "BASIC"), ("Nguyễn Khánh Linh", 13, "INTERMEDIATE") };
        for (var i = 0; i < children.Length; i++) {
            var child = children[i];
            var hv = new HocVien { MaHocVien = $"DEMO_HV{i + 1:00}", HoTen = child.Item1 + " (mẫu)", NgaySinh = BusinessClock.Today.AddYears(-child.Item2),
                LienHeKhanCapTen = member.HoTen, LienHeKhanCapSDT = "0000000000", TrangThai = "HOAT_DONG", GiaoVienDanhGia = teachers[1].Id,
                NgayDanhGiaDauVao = BusinessClock.Today, CapDoDeXuat = child.Item3, NhanXetDauVao = "Đánh giá mẫu để thử luồng đăng ký; không phải hồ sơ học viên thật." };
            db.HocVien.Add(hv); await db.SaveChangesAsync();
            db.DaiDienHocVien.Add(new() { ThanhVienId = member.Id, HocVienId = hv.Id, QuanHe = "ME", QuyenDaiDien = "DAY_DU", LaLienHeChinh = true,
                HieuLucTu = BusinessClock.Now, XacNhanLuc = BusinessClock.Now, NguoiXacNhan = admin.Id });
        }
        db.NhatKyThayDoi.Add(new() { LoaiDoiTuong = "DemoData", IdDoiTuong = 0, HanhDong = "THEM_MAU", CorrelationId = Guid.NewGuid().ToString("N"), Kenh = "CLI", ThoiDiem = BusinessClock.Now,
            TaiKhoanId = admin.Id, SauJson = JsonSerializer.Serialize(new { Source = "CNTT-KLCN082_Tuan4.docx", Courses = 6, Classes = 6, Sessions = totalSessions }) });
        await db.SaveChangesAsync(); await tx.CommitAsync();
        var local = Path.Combine(contentRoot, ".local"); Directory.CreateDirectory(local);
        await File.WriteAllTextAsync(Path.Combine(local, "Tai_khoan_mau.txt"), "Tài khoản thành viên minh họa để thử web\nTên đăng nhập: " + account.TenDangNhap + "\nMật khẩu: " + pw + "\nCó 3 học viên đã xác minh, phù hợp Foundation 1 / Basic 1 / Intermediate 1.\nKhông có phiếu thanh toán giả. Quy định của trung tâm cần được kích hoạt trước khi đăng ký.\n");
        Console.WriteLine($"Đã thêm 6 khóa, 6 lớp, {totalSessions} buổi, 4 giáo viên, 2 phòng và 3 học viên mẫu. Thông tin đăng nhập nằm trong .local/Tai_khoan_mau.txt.");
    }
}
