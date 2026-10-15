using LhlArt.Models;
using LhlArt.Services;

namespace LhlArt.Data;
public static class Seed
{
    public static AppData Create()
    {
        var d = new AppData
        {
            SchemaVersion = 2
        };
        d.CourseTypes.AddRange(new[] { new CourseType { Id = "LK01", Name = "Core Program" }, new CourseType { Id = "LK02", Name = "Chương trình bổ trợ" } });
        string[] names =
        {
            "Junior",
            "Foundation 1",
            "Pre-Basic 1",
            "Basic 1",
            "Intermediate 1",
            "Acrylic nhập môn"
        };
        int[] ages =
        {
            3,
            6,
            7,
            9,
            12,
            9
        };
        decimal[] fees =
        {
            3200000,
            3600000,
            3800000,
            4000000,
            4400000,
            2400000
        };
        string[] teacher =
        {
            "GV01",
            "GV02",
            "GV03",
            "GV02",
            "GV03",
            "GV03"
        };
        string[] rooms =
        {
            "P01",
            "P01",
            "P02",
            "P01",
            "P02",
            "P02"
        };
        int[] hours =
        {
            8,
            9,
            8,
            8,
            8,
            10
        };
        int[] mins =
        {
            0,
            45,
            0,
            0,
            0,
            15
        };
        d.Rooms.AddRange(new[] { new Room { Id = "P01", Name = "Phòng sáng tạo 1", Capacity = 10 }, new Room { Id = "P02", Name = "Phòng sáng tạo 2", Capacity = 10 } });
        d.Teachers.AddRange(new[] { new Teacher { Id = "GV01", Name = "Nguyễn Minh Anh", Phone = "0900000001", Specialty = "Junior" }, new Teacher { Id = "GV02", Name = "Trần Thu Hà", Phone = "0900000002", Specialty = "Foundation, Basic" }, new Teacher { Id = "GV03", Name = "Lê Quốc Bảo", Phone = "0900000003", Specialty = "Pre-Basic, Intermediate, Acrylic" }, new Teacher { Id = "TG01", Name = "Phạm Ngọc Mai", Phone = "0900000004", Specialty = "Trợ giảng Junior" } });
        for (int i = 0; i < 6; i++)
        {
            var k = new Course
            {
                Id = $"KH{i + 1:00}",
                Name = names[i],
                TypeId = i == 5 ? "LK02" : "LK01",
                MinAge = ages[i],
                Sessions = i == 5 ? 8 : 16,
                Minutes = i == 0 ? 90 : 120,
                Fee = fees[i],
                Level = "Cơ bản"
            };
            d.Courses.Add(k);
            var l = new Classroom
            {
                Id = $"L{i + 1:00}",
                Name = names[i] + (i < 3 ? " sáng thứ Bảy" : " sáng Chủ nhật"),
                CourseId = k.Id,
                TeacherId = teacher[i],
                AssistantId = i == 0 ? "TG01" : "",
                RoomId = rooms[i],
                Start = new DateTime(2026, 10, i < 3 ? 17 : 18, hours[i], mins[i], 0),
                Capacity = i == 0 ? 8 : 10,
                Fee = fees[i],
                Status = "Tuyển sinh",
                MaterialsReady = true
            };
            d.Classrooms.Add(l);
            for (int j = 0; j < k.Sessions; j++)
                d.Lessons.Add(new Lesson { Id = $"BH{i + 1:00}{j + 1:00}", ClassId = l.Id, Number = j + 1, Start = l.Start.AddDays(j * 7), End = l.Start.AddDays(j * 7).AddMinutes(k.Minutes), TeacherId = l.TeacherId, RoomId = l.RoomId, Topic = $"Nội dung {j + 1}: luyện tập mỹ thuật" });
        }

        string[] students =
        {
            "Nguyễn An Nhiên",
            "Trần Minh Khang",
            "Lê Bảo Ngọc",
            "Phạm Gia Hân",
            "Võ Đức Minh",
            "Đặng Khánh An",
            "Bùi Hoàng Nam",
            "Đỗ Tuệ Lâm"
        };
        for (int i = 0; i < students.Length; i++)
            d.Students.Add(new Student { Id = $"HV{i + 1:00}", Name = students[i], Birth = new DateTime(2016, 1 + i, 10), Guardian = $"Phụ huynh mẫu {i + 1}", Phone = $"09010000{i + 1:00}", Email = $"ph{i + 1}@example.com", Verified = true, ConsentStudy = true, MemberSince = new DateTime(2026, 10, 1), Consultation = "Quan tâm khóa Foundation, đã đánh giá phù hợp." });
        for (int i = 0; i < 6; i++)
        {
            d.Enrollments.Add(new Enrollment { Id = $"DK{i + 1:00}", StudentId = $"HV{i + 1:00}", ClassId = "L02", Created = new DateTime(2026, 10, 5, 9, 0, 0), Deadline = new DateTime(2026, 10, 5, 9, 0, 0).AddHours(24), OriginalFee = 3600000, Due = 3600000, Status = "Đã xác nhận" });
            d.Payments.Add(new Payment { Id = $"GD{i + 1:00}", EnrollmentId = $"DK{i + 1:00}", At = new DateTime(2026, 10, 5, 9, 0, 0), Amount = 3600000, Reference = $"PT-MAU-{i + 1:00}", Operator = "ketoan" });
        }

        d.Promotions.Add(new Promotion { Start = new DateTime(2026, 10, 1), End = new DateTime(2026, 10, 31), Id = "KM01", Name = "Thành viên mới – giảm 10% học phí" });
        d.Promotions.Add(new Promotion { Start = new DateTime(2026, 10, 1), End = new DateTime(2026, 10, 31), Id = "KM02", Name = "Giảm 10% họa cụ", Group = "Họa cụ", Cap = 30000, Limit = 20 });
        string[] supply =
        {
            "Sáp màu 24 màu",
            "Bút chì màu 12 màu",
            "Giấy vẽ A4 160 gsm",
            "Màu nước 12 màu",
            "Cọ tròn",
            "Toan vẽ 20 × 30 cm"
        };
        decimal[] prices =
        {
            45000,
            38000,
            3000,
            55000,
            25000,
            75000
        };
        for (int i = 0; i < supply.Length; i++)
        {
            var h = new Supply
            {
                Id = $"HC{i + 1:00}",
                Name = supply[i],
                Unit = i == 2 ? "Tờ" : "Bộ",
                UnitPrice = prices[i],
                Threshold = i == 2 ? 40 : 5
            };
            d.Supplys.Add(h);
            d.StockMoves.Add(new StockMove { Id = $"NK{i + 1:00}", SupplyId = h.Id, Quantity = i == 2 ? 500 : 30, At = new DateTime(2026, 10, 5, 9, 0, 0), Note = "Tồn đầu kỳ kiểm thử", Operator = "kho" });
        }

        foreach (var a in new[]
        {
            ("admin", "Quản lý", ""),
            ("hocvu", "Học vụ", ""),
            ("ketoan", "Kế toán", ""),
            ("giaovien", "Giáo viên", "GV02"),
            ("kho", "Họa cụ", "")
        }

        )
            d.Accounts.Add(new Account { Id = a.Item1, Name = a.Item1, Role = a.Item2, TeacherId = a.Item3, PasswordHash = Passwords.Hash("123456") });
        d.Audits.Add(new Audit { Id = "NK000", At = new DateTime(2026, 10, 5, 9, 0, 0), User = "test", Action = "Khởi tạo dữ liệu kiểm thử, không phải dữ liệu cơ sở thực tế." });
        return d;
    }
}
