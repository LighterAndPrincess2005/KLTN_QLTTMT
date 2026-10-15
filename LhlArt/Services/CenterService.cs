using System.Text.Json;
using LhlArt.Models;
using LhlArt.Data;

namespace LhlArt.Services;
public partial class CenterService
{
    readonly JsonStore store;
    public AppData D { get; private set; }
    public Account User { get; private set; } = new();

    readonly Func<DateTime> clock;
    public DateTime Now => clock();
    public string DataPath => store.PathName;

    public CenterService(JsonStore store, Func<DateTime>? clock = null)
    {
        this.store = store;
        this.clock = clock ?? (() => BusinessTime.Now);
        D = store.Load();
        store.Save(D);
    }

    public bool Login(string name, string password)
    {
        var a = D.Accounts.FirstOrDefault(x => x.Enabled && x.Name.Equals(name.Trim(), StringComparison.OrdinalIgnoreCase) && Passwords.Verify(password, x.PasswordHash));
        if (a == null)
            return false;
        User = a;
        return true;
    }

    public bool Is(params string[] roles) => User.Enabled && (User.Role == "Quản lý" || roles.Contains(User.Role));
    void Need(params string[] roles)
    {
        if (!Is(roles))
            throw new Exception("Tài khoản không có quyền thực hiện thao tác này.");
    }

    public static void Check(bool ok, string message)
    {
        if (!ok)
            throw new Exception(message);
    }

    public static string NewId(string prefix) => prefix + Guid.NewGuid().ToString("N")[..8].ToUpperInvariant();
    // Chỉ thay dữ liệu trên bộ nhớ khi cả xử lý và ghi tệp thành công.
    public void Commit(string action, Action change)
    {
        string snapshot = JsonSerializer.Serialize(D);
        try
        {
            change();
            D.Audits.Add(new Audit { Id = NewId("NK"), At = Now, User = User.Name, Action = action });
            store.Save(D);
        }
        catch
        {
            D = JsonSerializer.Deserialize<AppData>(snapshot)!;
            User = D.Accounts.FirstOrDefault(x => x.Id == User.Id) ?? new Account();
            throw;
        }
    }

    public void ChangePassword(string old, string password)
    {
        Check(Passwords.Verify(old, User.PasswordHash), "Mật khẩu cũ không đúng.");
        Check(password.Length >= 6, "Mật khẩu mới cần ít nhất 6 ký tự.");
        Commit("Đổi mật khẩu", () =>
        {
            D.Accounts.Single(x => x.Id == User.Id).PasswordHash = Passwords.Hash(password);
        });
        User = D.Accounts.Single(x => x.Id == User.Id);
    }

    public void RefreshTime()
    {
        if (D.Enrollments.Any(x => x.Status == "Chờ thanh toán" && x.Deadline <= Now))
            Commit("Cập nhật đăng ký hết hạn", Expire);
    }

    public void InitializeAdministrator(string username, string password)
    {
        Check(D.Accounts.Count == 0, "Tài khoản quản trị đã được thiết lập.");
        Check(!string.IsNullOrWhiteSpace(username), "Nhập tên đăng nhập.");
        Check(username.Length >= 3 && username.Length <= 40 && username.All(x => char.IsLetterOrDigit(x) || x == '_' || x == '.'), "Tên đăng nhập gồm 3–40 chữ, số, dấu chấm hoặc gạch dưới.");
        Check(password.Length >= 8, "Mật khẩu cần ít nhất 8 ký tự.");
        Commit("Thiết lập tài khoản quản trị", () => D.Accounts.Add(new Account { Id = NewId("TK"), Name = username, Role = "Quản lý", PasswordHash = Passwords.Hash(password) }));
    }

    public void Expire()
    {
        foreach (var r in D.Enrollments.Where(x => x.Status == "Chờ thanh toán" && x.Deadline <= Now))
            r.Status = "Hết hạn";
    }

    public bool Active(Enrollment r) => r.Status == "Đã xác nhận" || (r.Status == "Chờ thanh toán" && r.Deadline > Now);
    public int Seats(string id, string exceptRequest = "") => D.Enrollments.Count(x => x.ClassId == id && Active(x)) + D.ChangeRequests.Count(x => x.Id != exceptRequest && x.Kind == "Chuyển" && x.TargetClassId == id && x.Status == "Được duyệt" && Now < x.ApprovedAt.AddHours(24));
    public decimal Paid(string id) => D.Payments.Where(x => x.EnrollmentId == id).Sum(x => x.Kind == "Thu" ? x.Amount : -x.Amount);
    public int Stock(string id) => D.StockMoves.Where(x => x.SupplyId == id).Sum(x => x.Quantity);
    public T Get<T>(string id)
        where T : Entity => List<T>().Single(x => x.Id == id);
    public List<T> List<T>()
        where T : Entity => (List<T>)typeof(AppData).GetProperty(typeof(T).Name + "s")!.GetValue(D)!;
    public bool TeacherCan(string classId) => Is("Học vụ") || D.Classrooms.Any(x => x.Id == classId && x.TeacherId == User.TeacherId);
    public IEnumerable<Lesson> VisibleLessons() => D.Lessons.Where(x => User.Role != "Giáo viên" || TeacherCan(x.ClassId));
    public IEnumerable<Enrollment> VisibleEnrollments() => D.Enrollments.Where(x => User.Role != "Giáo viên" || TeacherCan(x.ClassId));
    public int Age(Student s, DateTime at)
    {
        int n = at.Year - s.Birth.Year;
        if (s.Birth.Date.AddYears(n) > at.Date)
            n--;
        return n;
    }

    bool Overlap(Lesson a, Lesson b, int gap = 0) => a.Start < b.End.AddMinutes(gap) && b.Start < a.End.AddMinutes(gap);
    void CheckSchedule(Lesson b)
    {
        var c = Get<Classroom>(b.ClassId);
        var k = Get<Course>(c.CourseId);
        Check(b.End > b.Start && (b.End - b.Start).TotalMinutes == k.Minutes, "Thời lượng buổi phải đúng khóa học.");
        Check(Get<Room>(b.RoomId).Capacity >= c.Capacity, "Phòng không đủ sức chứa.");
        Check(D.Teachers.Any(x => x.Id == b.TeacherId), "Giáo viên không tồn tại.");
        Check(!D.Lessons.Any(x => x.Id != b.Id && x.Status != "Hủy" && Overlap(x, b, 15) && (x.RoomId == b.RoomId || x.TeacherId == b.TeacherId || x.ClassId == b.ClassId)), "Trùng phòng / giáo viên / lớp hoặc khoảng nghỉ dưới 15 phút.");
    }

    void CheckStudentSchedule(string studentId, string classId, string except = "")
    {
        var existing = D.Enrollments.Where(x => x.StudentId == studentId && x.Id != except && Active(x)).Select(x => x.ClassId).ToHashSet();
        Check(!D.Lessons.Any(a => a.ClassId == classId && a.Status != "Hủy" && a.End > Now && D.Lessons.Any(b => existing.Contains(b.ClassId) && b.Status != "Hủy" && Overlap(a, b))), "Học viên bị trùng lịch với lớp đã đăng ký.");
    }

    public void Save<T>(T item, bool isNew)
        where T : Entity
    {
        if (typeof(T) == typeof(Account))
            throw new Exception("Sử dụng chức năng quản lý tài khoản.");
        if (typeof(T) == typeof(Supply))
            Need("Họa cụ");
        else if (typeof(T) == typeof(Promotion))
            Need();
        else
            Need("Học vụ");
        Check(!string.IsNullOrWhiteSpace(item.Id), "Mã không được trống.");
        Check(!isNew || !List<T>().Any(x => x.Id.Equals(item.Id, StringComparison.OrdinalIgnoreCase)), "Mã đã tồn tại.");
        foreach (var p in typeof(T).GetProperties().Where(x => x.Name == "Name"))
            Check(!string.IsNullOrWhiteSpace((string? )p.GetValue(item)), "Tên không được trống.");
        if (item is Student s)
        {
            Check(s.Birth.Date <= Now.Date, "Ngày sinh không hợp lệ.");
            Check(s.Phone.Length >= 10 && s.Phone.Length <= 11 && s.Phone.All(char.IsDigit), "Điện thoại phải gồm 10–11 chữ số.");
            Check(s.Email == "" || System.Net.Mail.MailAddress.TryCreate(s.Email, out _), "Email không hợp lệ.");
            Check(!D.Students.Any(x => x.Id != s.Id && x.Name.Equals(s.Name, StringComparison.OrdinalIgnoreCase) && x.Birth.Date == s.Birth.Date && x.Phone == s.Phone), "Có hồ sơ trùng họ tên, ngày sinh và liên hệ. Hãy đối chiếu hồ sơ cũ.");
            Check(!s.Verified || !string.IsNullOrWhiteSpace(s.Guardian), "Cần nhập người đại diện / thành viên đã xác minh.");
        }

        if (item is Course k)
        {
            Check(D.CourseTypes.Any(x => x.Id == k.TypeId), "Chọn loại khóa.");
            Check(k.Sessions > 0 && k.Minutes > 0 && k.Fee >= 0 && k.MinAge >= 0, "Số buổi, thời lượng, tuổi và học phí không hợp lệ.");
            Check(isNew || !D.Classrooms.Any(x => x.CourseId == k.Id), "Khóa đã có lớp: giữ phiên bản đã chốt. Tạo mã khóa mới để đổi chương trình.");
        }

        if (item is Room room)
            Check(room.Capacity > 0 && !D.Classrooms.Any(x => x.RoomId == room.Id && x.Capacity > room.Capacity), "Sức chứa không hợp lệ hoặc nhỏ hơn sĩ số lớp.");
        if (item is Supply h)
            Check(h.Threshold >= 0 && h.UnitPrice >= 0 && !string.IsNullOrWhiteSpace(h.Unit), "Đơn vị, giá bán hoặc ngưỡng không hợp lệ.");
        if (item is Promotion km)
        {
            Check(km.Group is "Học phí" or "Họa cụ" && km.End >= km.Start && km.Percent > 0 && km.Percent <= 100 && km.Cap >= 0 && km.Limit > 0, "Thông số ưu đãi không hợp lệ.");
            Check(isNew || !D.Enrollments.Any(x => x.PromotionId == km.Id) && !D.MaterialSales.Any(x => x.PromotionId == km.Id), "Ưu đãi đã dùng: tạo mã mới để giữ lịch sử.");
        }

        if (item is Classroom l)
        {
            var course = Get<Course>(l.CourseId);
            Check(l.Capacity >= 6 && l.Capacity <= (course.Name.Contains("Junior") ? 8 : 10), "Sĩ số phải từ 6 đến 8 (Junior), hoặc 6 đến 10.");
            Check(l.Fee >= 0 && Get<Room>(l.RoomId).Capacity >= l.Capacity, "Học phí hoặc sức chứa phòng không hợp lệ.");
            Check(D.Teachers.Any(x => x.Id == l.TeacherId), "Chọn giáo viên chính.");
            Check(!course.Name.Contains("Junior") || D.Teachers.Any(x => x.Id == l.AssistantId && x.Id != l.TeacherId), "Junior cần trợ giảng khác giáo viên chính.");
            Check(isNew || (!D.Enrollments.Any(x => x.ClassId == l.Id) && !D.ChangeRequests.Any(x => x.TargetClassId == l.Id)), "Lớp đã có đăng ký: không sửa thông tin gốc. Dùng chức năng lịch hoặc duyệt khai giảng.");
            l.Status = "Dự kiến";
            Check(l.Start > Now, "Ngày khai giảng phải sau thời điểm hiện tại.");
        }

        Commit("Lưu " + typeof(T).Name + " " + item.Id, () =>
        {
            if (!isNew)
                List<T>().RemoveAll(x => x.Id == item.Id);
            List<T>().Add(item);
            if (item is Classroom savedClass)
            {
                D.Lessons.RemoveAll(x => x.ClassId == savedClass.Id);
                var savedCourse = Get<Course>(savedClass.CourseId);
                for (int n = 1; n <= savedCourse.Sessions; n++)
                {
                    var b = new Lesson
                    {
                        Id = NewId("BH"),
                        ClassId = savedClass.Id,
                        Number = n,
                        Start = savedClass.Start.AddDays((n - 1) * 7),
                        End = savedClass.Start.AddDays((n - 1) * 7).AddMinutes(savedCourse.Minutes),
                        TeacherId = savedClass.TeacherId,
                        RoomId = savedClass.RoomId,
                        Topic = "Nội dung " + n
                    };
                    CheckSchedule(b);
                    D.Lessons.Add(b);
                }
            }
        });
    }

    public void Delete<T>(T item)
        where T : Entity
    {
        if (typeof(T) == typeof(Account))
            throw new Exception("Sử dụng chức năng quản lý tài khoản.");
        if (typeof(T) == typeof(Supply))
            Need("Họa cụ");
        else if (typeof(T) == typeof(Promotion))
            Need();
        else
            Need("Học vụ");
        bool used = item switch
        {
            Student => D.Enrollments.Any(x => x.StudentId == item.Id),
            CourseType => D.Courses.Any(x => x.TypeId == item.Id),
            Course => D.Classrooms.Any(x => x.CourseId == item.Id),
            Teacher => D.Accounts.Any(x => x.TeacherId == item.Id) || D.Lessons.Any(x => x.TeacherId == item.Id) || D.Classrooms.Any(x => x.TeacherId == item.Id || x.AssistantId == item.Id),
            Room => D.Lessons.Any(x => x.RoomId == item.Id),
            Classroom => D.Enrollments.Any(x => x.ClassId == item.Id) || D.StockMoves.Any(x => x.ClassId == item.Id) || D.ChangeRequests.Any(x => x.TargetClassId == item.Id),
            Supply => D.StockMoves.Any(x => x.SupplyId == item.Id) || D.MaterialSales.Any(x => x.SupplyId == item.Id),
            Promotion => D.Enrollments.Any(x => x.PromotionId == item.Id) || D.MaterialSales.Any(x => x.PromotionId == item.Id),
            _ => true
        };
        Check(!used, "Hồ sơ đã được sử dụng. Không xóa để bảo toàn lịch sử.");
        Commit("Xóa " + item.Id, () =>
        {
            List<T>().RemoveAll(x => x.Id == item.Id);
            if (item is Classroom)
                D.Lessons.RemoveAll(x => x.ClassId == item.Id);
        });
    }
}
