using LhlArt.Models;
using LhlArt.Views.Shared;

namespace LhlArt.Controllers;
public partial class MainController
{
    void Dashboard()
    {
        s.RefreshTime();
        var metrics = new List<Metric>();
        if (s.Is("Học vụ"))
        {
            metrics.Add(new("Học viên", s.D.Students.Count.ToString("N0"), "Hồ sơ học viên"));
            metrics.Add(new("Khóa học", s.D.Courses.Count.ToString("N0"), "Chương trình hiện có"));
            metrics.Add(new("Lớp học", s.D.Classrooms.Count.ToString("N0"), "Lớp đã lập"));
        }

        if (s.Is("Học vụ", "Kế toán", "Giáo viên"))
            metrics.Add(new("Đăng ký hiệu lực", s.VisibleEnrollments().Count(s.Active).ToString("N0"), "Đã xác nhận / giữ chỗ"));
        if (s.Is("Học vụ", "Giáo viên"))
            metrics.Add(new("Buổi học hôm nay", s.VisibleLessons().Count(x => x.Start.Date == s.Now.Date && x.Status != "Hủy").ToString("N0"), "Theo lịch giảng dạy"));
        if (s.Is("Kế toán"))
        {
            metrics.Add(new("Thực thu học phí", Theme.Money(s.D.Payments.Sum(x => x.Kind == "Thu" ? x.Amount : -x.Amount)), "Đã trừ khoản hoàn"));
            metrics.Add(new("Doanh thu họa cụ", Theme.Money(s.D.MaterialSales.Sum(x => x.Total)), "Từ các phiếu bán"));
        }

        if (s.Is("Họa cụ"))
            metrics.Add(new("Họa cụ cần bổ sung", s.D.Supplys.Count(x => s.Stock(x.Id) < x.Threshold).ToString("N0"), "Dưới mức tồn tối thiểu"));
        if (s.User.Role == "Họa cụ")
        {
            metrics.Add(new("Danh mục họa cụ", s.D.Supplys.Count.ToString("N0"), "Mặt hàng đang quản lý"));
            metrics.Add(new("Phiếu kho", s.D.StockMoves.Count.ToString("N0"), "Lượt nhập, xuất và bán"));
        }

        if (s.User.Role == "Giáo viên")
            metrics.Add(new("Lớp phụ trách", s.D.Classrooms.Count(x => x.TeacherId == s.User.TeacherId).ToString("N0"), "Theo phân công giảng dạy"));
        string highlightTitle = "Lịch học sắp tới", highlightRoute = "Lịch học & điểm danh";
        var highlights = new List<DashboardLine>();
        if (s.Is("Học vụ", "Giáo viên"))
            highlights = s.VisibleLessons().Where(x => x.Start >= s.Now && x.Status != "Hủy").OrderBy(x => x.Start).Take(3).Select(x => new DashboardLine(s.Get<Classroom>(x.ClassId).Name, $"{x.Start:dd/MM/yyyy} · {x.RoomId} · Buổi {x.Number}", x.Start.ToString("HH:mm"))).ToList();
        else if (s.Is("Kế toán"))
        {
            highlightTitle = "Giao dịch gần đây";
            highlightRoute = "Thống kê & giao dịch";
            highlights = s.D.Payments.OrderByDescending(x => x.At).Take(3).Select(x => new DashboardLine(x.Reference, $"{x.At:dd/MM HH:mm} · {x.Kind} · {x.EnrollmentId}", Theme.Money(x.Amount))).ToList();
        }
        else if (s.Is("Họa cụ"))
        {
            highlightTitle = "Phiếu kho gần đây";
            highlightRoute = "Nhập / xuất họa cụ";
            highlights = s.D.StockMoves.OrderByDescending(x => x.At).Take(3).Select(x => new DashboardLine(s.Get<Supply>(x.SupplyId).Name, $"{x.At:dd/MM HH:mm} · {(x.Quantity >= 0 ? "Nhập" : "Xuất")}", x.Quantity.ToString("+0;-0;0"))).ToList();
        }

        var notices = new List<DashboardLine>();
        if (s.Is("Học vụ", "Kế toán"))
        {
            notices.Add(new("Đăng ký chờ thanh toán", "Cần đối chiếu học phí", s.D.Enrollments.Count(x => x.Status == "Chờ thanh toán" && x.Deadline > s.Now).ToString()));
            notices.Add(new("Yêu cầu chờ duyệt", "Hủy hoặc chuyển lớp", s.D.ChangeRequests.Count(x => x.Status == "Chờ duyệt").ToString()));
        }

        if (s.Is("Họa cụ"))
            notices.Add(new("Bổ sung họa cụ", "Mặt hàng dưới mức tồn", s.D.Supplys.Count(x => s.Stock(x.Id) < x.Threshold).ToString()));
        if (s.User.Role == "Kế toán")
            notices.Add(new("Yêu cầu đã duyệt", "Cần hoàn tất thu hoặc hoàn", s.D.ChangeRequests.Count(x => x.Status == "Được duyệt").ToString()));
        if (s.User.Role == "Học vụ")
            notices.Add(new("Lớp dự kiến", "Cần hoàn thiện và trình duyệt", s.D.Classrooms.Count(x => x.Status == "Dự kiến").ToString()));
        if (s.User.Role == "Họa cụ")
        {
            notices.Add(new("Họa cụ hết tồn", "Kiểm tra trước khi cấp phát", s.D.Supplys.Count(x => s.Stock(x.Id) == 0).ToString()));
            notices.Add(new("Phiếu kho hôm nay", "Nhập, xuất và bán họa cụ", s.D.StockMoves.Count(x => x.At.Date == s.Now.Date).ToString()));
        }

        if (s.User.Role == "Giáo viên")
        {
            var classes = s.D.Classrooms.Where(x => x.TeacherId == s.User.TeacherId).Select(x => x.Id).ToHashSet();
            var enrolls = s.D.Enrollments.Where(x => classes.Contains(x.ClassId)).Select(x => x.Id).ToHashSet();
            notices.Add(new("Buổi học hôm nay", "Theo phân công giảng dạy", s.VisibleLessons().Count(x => x.Start.Date == s.Now.Date && x.Status != "Hủy").ToString()));
            notices.Add(new("Kết quả chờ duyệt", "Điểm và nhận xét đã gửi", s.D.CourseResults.Count(x => enrolls.Contains(x.EnrollmentId) && x.Status == "Chờ duyệt").ToString()));
        }

        var shortcuts = new List<DashboardShortcut>();
        if (s.Is("Học vụ"))
        {
            shortcuts.Add(new("Học viên", "Học viên & tư vấn"));
            shortcuts.Add(new("Lớp học", "Lớp học"));
        }

        if (s.Is("Học vụ", "Kế toán"))
            shortcuts.Add(new("Đăng ký & học phí", "Đăng ký & thu học phí"));
        if (s.Is("Học vụ", "Giáo viên"))
            shortcuts.Add(new("Lịch học", "Lịch học & điểm danh"));
        if (s.User.Role == "Giáo viên")
            shortcuts.Add(new("Kết quả học tập", "Kết quả học tập"));
        if (s.Is("Kế toán") && !s.Is())
            shortcuts.Add(new("Báo cáo", "Thống kê & giao dịch"));
        if (s.User.Role == "Họa cụ")
        {
            shortcuts.Add(new("Danh mục họa cụ", "Danh mục họa cụ"));
            shortcuts.Add(new("Tồn kho", "Nhập / xuất họa cụ"));
            shortcuts.Add(new("Lịch sử bán", "Lịch sử bán"));
        }

        string progressTitle = "Sĩ số các lớp · chỗ dùng / tối đa";
        var progress = s.D.Classrooms.Where(x => s.User.Role != "Giáo viên" || x.TeacherId == s.User.TeacherId).OrderBy(x => x.Start < s.Now).ThenBy(x => x.Start).Take(3).Select(x => new DashboardProgress(x.Name, s.Seats(x.Id), x.Capacity)).ToList();
        if (s.User.Role == "Họa cụ")
        {
            progressTitle = "Họa cụ · tồn / mức tối thiểu";
            progress = s.D.Supplys.OrderBy(x => s.Stock(x.Id) - x.Threshold).Take(3).Select(x => new DashboardProgress(x.Name, s.Stock(x.Id), x.Threshold)).ToList();
        }

        refresh = Dashboard;
        view.RenderDashboard(new DashboardData(metrics, highlights, notices, "Hoạt động trung tâm", "LHL ART · " + s.Now.ToString("dd/MM/yyyy"), highlightTitle, highlightRoute, shortcuts, progressTitle, progress), s.Now);
        if (s.Is())
            Button("Sao lưu dữ liệu", Backup);
    }
}
