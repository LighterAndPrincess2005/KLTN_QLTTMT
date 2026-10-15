using LhlArt.Models;
using LhlArt.Views;
using LhlArt.Views.Shared;

namespace LhlArt.Controllers;
public partial class MainController
{
    void Classes()
    {
        Catalog<Classroom>("Lớp học", "Dự kiến → Tuyển sinh → Đã chốt khai giảng. Tự tạo lịch hàng tuần khi lưu lớp.");
        refresh = Classes;
        if (s.Is())
        {
            Button("Duyệt tuyển sinh", () =>
            {
                var c = Selected<Classroom>();
                if (Confirm("Duyệt tuyển sinh lớp " + c.Id + "?"))
                    Execute(() => s.ApproveClass(c.Id, false));
            });
            Button("Chốt khai giảng", () =>
            {
                var c = Selected<Classroom>();
                if (Confirm("Chốt khai giảng lớp " + c.Id + "?"))
                    Execute(() => s.ApproveClass(c.Id, true));
            });
        }

        Button("Xem sĩ số", () =>
        {
            var c = Selected<Classroom>();
            MessageBox.Show($"{c.Name}\nĐã xác nhận: {s.D.Enrollments.Count(x => x.ClassId == c.Id && x.Status == "Đã xác nhận")}\nChỗ đang sử dụng: {s.Seats(c.Id)}/{c.Capacity}\nCòn nhận: {c.Capacity - s.Seats(c.Id)}");
        });
    }

    void Enrollments()
    {
        Show("Đăng ký & thu học phí", "UCS02 • Giữ chỗ 24 giờ, tối đa đến mốc khai giảng − 48 giờ. Chỉ xác nhận khi đủ tiền.", s.D.Enrollments, Enrollments);
        if (s.Is("Học vụ"))
            Button("Lập đăng ký", () =>
            {
                using var d = new InputForm("Lập phiếu giữ chỗ");
                d.Add("student", "Học viên", "", Students());
                d.Add("class", "Lớp đăng ký", "", ClassesList());
                d.Add("promotion", "Ưu đãi học phí", "", new[] { new Choice("", "Không áp dụng") }.Concat(s.D.Promotions.Where(x => x.Group == "Học phí").Select(x => new Choice(x.Id, x.Id + " – " + x.Name))));
                if (d.ShowDialog(view) == DialogResult.OK)
                {
                    var c = s.Get<Classroom>(d.TextValue("class"));
                    var p = s.D.Promotions.FirstOrDefault(x => x.Id == d.TextValue("promotion"));
                    decimal discount = p == null ? 0 : Math.Min(c.Fee, Math.Min(p.Cap, Math.Round(c.Fee * p.Percent / 100, 0, MidpointRounding.AwayFromZero)));
                    if (Confirm($"Học phí: {c.Fee:N0} đ\nGiảm: {discount:N0} đ\nCần thu: {c.Fee - discount:N0} đ\n\nLập phiếu đăng ký?"))
                        Execute(() => s.Enroll(d.TextValue("student"), d.TextValue("class"), d.TextValue("promotion")));
                }
            });
        if (s.Is("Kế toán"))
        {
            Button("Ghi nhận tiền thu", () =>
            {
                var r = Selected<Enrollment>();
                using var d = new InputForm("Xác nhận tiền thực nhận");
                d.Add("amount", "Số tiền thu", Math.Max(0, r.Due - s.Paid(r.Id)));
                d.Add("method", "Phương thức", "Tiền mặt", new[] { new Choice("Tiền mặt", "Tiền mặt"), new Choice("Chuyển khoản", "Chuyển khoản") });
                d.Add("ref", "Số chứng từ / tham chiếu", "");
                d.Add("checked", "Đã đối chiếu tiền thực nhận", false);
                if (d.ShowDialog(view) == DialogResult.OK)
                {
                    Services.CenterService.Check(d.Flag("checked"), "Cần đối chiếu tiền thực nhận trước khi lưu.");
                    Execute(() => s.Receive(r.Id, d.Number("amount"), d.TextValue("method"), d.TextValue("ref")));
                }
            });
            Button("Hoàn phiếu hết hạn", () =>
            {
                var r = Selected<Enrollment>();
                using var d = new InputForm("Hoàn tiền phiếu hết hạn");
                d.Add("info", "Số tiền hoàn", s.Paid(r.Id), readOnly: true);
                d.Add("ref", "Chứng từ đã hoàn tiền", "");
                if (d.ShowDialog(view) == DialogResult.OK && Confirm("Xác nhận đã chi hoàn thực tế?"))
                    Execute(() => s.RefundExpired(r.Id, d.TextValue("ref")));
            });
        }

        Button("Chi tiết / phiếu", () =>
        {
            var r = Selected<Enrollment>();
            var h = s.Get<Student>(r.StudentId);
            var c = s.Get<Classroom>(r.ClassId);
            string text = $"PHIẾU ĐĂNG KÝ HỌC\nMã: {r.Id}\nHọc viên: {h.Name}\nĐại diện: {h.Guardian}\nLớp: {c.Name}\nKhai giảng: {c.Start:dd/MM/yyyy HH:mm}\nHọc phí gốc: {r.OriginalFee:N0} đ\nGiảm: {r.Discount:N0} đ\nHọc phí chốt: {r.Due:N0} đ\nThực thu ròng: {s.Paid(r.Id):N0} đ\nGiá trị chuyển đến: {r.Credit:N0} đ\nCòn thiếu: {Math.Max(0, r.Due - s.Paid(r.Id) - r.Credit):N0} đ\nHạn giữ chỗ: {r.Deadline:dd/MM/yyyy HH:mm}\nTrạng thái: {r.Status}";
            Receipt(text);
        });
    }

    void Receipt(string text)
    {
        using var receipt = new ReceiptForm(text);
        receipt.ShowDialog(view);
    }

    void Lessons()
    {
        Show("Lịch học & điểm danh", "UCS03 • Chọn buổi học trước khi điểm danh. Giáo viên chỉ cập nhật buổi được phân công.", s.VisibleLessons().OrderBy(x => x.Start).ToList(), Lessons);
        if (s.Is("Học vụ"))
            Button("Đổi lịch buổi", () =>
            {
                var b = Selected<Lesson>();
                using var d = new InputForm("Đổi lịch buổi học");
                d.Add("time", "Ngày giờ mới", b.Start);
                d.Add("room", "Phòng", b.RoomId, s.D.Rooms.Select(x => new Choice(x.Id, x.Name)));
                d.Add("teacher", "Giáo viên", b.TeacherId, s.D.Teachers.Select(x => new Choice(x.Id, x.Name)));
                d.Add("reason", "Lý do", "");
                if (d.ShowDialog(view) == DialogResult.OK)
                    Execute(() => s.Reschedule(b.Id, d.Date("time"), d.TextValue("room"), d.TextValue("teacher"), d.TextValue("reason")));
            });
        Button("Điểm danh / nhận xét", () =>
        {
            var b = Selected<Lesson>();
            var enrolls = s.VisibleEnrollments().Where(x => x.ClassId == b.ClassId && x.Status == "Đã xác nhận").ToList();
            Services.CenterService.Check(enrolls.Count > 0, "Buổi này chưa có học viên đã xác nhận.");
            using var d = new InputForm("Điểm danh buổi " + b.Number + " – " + b.ClassId);
            d.Add("enroll", "Học viên", "", enrolls.Select(x => new Choice(x.Id, s.Get<Student>(x.StudentId).Name + " – " + x.Id)));
            d.Add("present", "Có mặt", true);
            d.Add("note", "Nhận xét", "");
            if (d.ShowDialog(view) == DialogResult.OK)
                Execute(() => s.Mark(d.TextValue("enroll"), b.Id, d.Flag("present"), d.TextValue("note")));
        });
        Button("Xem điểm danh", () =>
        {
            var b = Selected<Lesson>();
            Show("Điểm danh " + b.Id, "Ghi lại sẽ cập nhật đúng cặp đăng ký – buổi; không nhân đôi.", s.D.Attendances.Where(x => x.LessonId == b.Id).ToList(), Lessons);
            Button("Quay lại lịch", Lessons);
        });
    }

    void Results()
    {
        Show("Kết quả học tập", "UCS03 • Hoàn thành: ≥ 80% số buổi, điểm ≥ 5 và đủ học phí; quản lý duyệt kết quả.", s.VisibleEnrollments().Where(x => x.Status == "Đã xác nhận" || x.Status == "Hoàn thành" || x.Status == "Chưa hoàn thành").ToList(), Results);
        Button("Xem tiến độ", () =>
        {
            var r = Selected<Enrollment>();
            var k = s.Get<Course>(s.Get<Classroom>(r.ClassId).CourseId);
            var result = s.D.CourseResults.FirstOrDefault(x => x.EnrollmentId == r.Id);
            MessageBox.Show($"{s.Get<Student>(r.StudentId).Name}\nCó mặt: {s.Attended(r.Id)}/{k.Sessions}\nTối thiểu: {Math.Ceiling(k.Sessions * .8)} buổi\nĐiểm: {result?.Score.ToString() ?? "Chưa nhập"}\nKết quả: {result?.Status ?? "Chưa có"}\nNhận xét: {result?.Note}");
        });
        Button("Nhập điểm cuối khóa", () =>
        {
            var r = Selected<Enrollment>();
            var result = s.D.CourseResults.FirstOrDefault(x => x.EnrollmentId == r.Id);
            using var d = new InputForm("Kết quả cuối khóa");
            d.Add("score", "Điểm /10", result?.Score ?? 5m);
            d.Add("note", "Nhận xét tổng kết", result?.Note ?? "");
            if (d.ShowDialog(view) == DialogResult.OK)
                Execute(() => s.Result(r.Id, d.Number("score"), d.TextValue("note"), false));
        });
        if (s.Is())
            Button("Duyệt kết quả", () =>
            {
                var r = Selected<Enrollment>();
                if (Confirm("Chốt xét hoàn thành cho " + r.Id + "?"))
                    Execute(() => s.Result(r.Id, 0, "", true));
            });
    }

    void Changes()
    {
        Show("Yêu cầu hủy / chuyển lớp", "UCS04 • Tiếp nhận → Quản lý duyệt → Kế toán thực hiện thu/hoàn. Chuyển lớp trước khai giảng.", s.D.ChangeRequests, Changes);
        if (s.Is("Học vụ"))
        {
            Button("Tiếp nhận yêu cầu", () =>
            {
                using var d = new InputForm("Yêu cầu thay đổi đăng ký");
                d.Add("enroll", "Đăng ký", "", s.D.Enrollments.Where(x => s.Active(x)).Select(x => new Choice(x.Id, x.Id + " – " + s.Get<Student>(x.StudentId).Name + " / " + x.ClassId)));
                d.Add("kind", "Loại yêu cầu", "Hủy", new[] { new Choice("Hủy", "Hủy đăng ký"), new Choice("Chuyển", "Chuyển lớp") });
                d.Add("target", "Lớp đích (chuyển)", "", new[] { new Choice("", "Không có – yêu cầu hủy") }.Concat(ClassesList()));
                d.Add("reason", "Lý do", "");
                if (d.ShowDialog(view) == DialogResult.OK)
                    Execute(() => s.RequestChange(d.TextValue("enroll"), d.TextValue("kind"), d.TextValue("target"), d.TextValue("reason")));
            });
            Button("Rút yêu cầu", () =>
            {
                var q = Selected<ChangeRequest>();
                if (Confirm("Rút yêu cầu và giữ nguyên đăng ký gốc?"))
                    Execute(() => s.WithdrawRequest(q.Id));
            });
        }

        if (s.Is())
            Button("Duyệt / từ chối", () =>
            {
                var q = Selected<ChangeRequest>();
                using var d = new InputForm("Quyết định yêu cầu " + q.Id);
                d.Add("approve", "Đồng ý phê duyệt", true);
                d.Add("note", "Ý kiến quản lý", "");
                if (d.ShowDialog(view) == DialogResult.OK)
                    Execute(() => s.Decide(q.Id, d.Flag("approve"), d.TextValue("note")));
            });
        if (s.Is("Kế toán"))
            Button("Thực hiện thu / hoàn", () =>
            {
                var q = Selected<ChangeRequest>();
                decimal amount = q.Kind == "Hủy" ? q.Refund : s.Get<Classroom>(q.TargetClassId).Fee - s.Paid(q.EnrollmentId);
                using var d = new InputForm("Chốt yêu cầu đã duyệt");
                d.Add("money", q.Kind == "Hủy" ? "Số tiền phải hoàn" : "Số tiền phải thu thêm", amount, readOnly: true);
                d.Add("ref", "Chứng từ (nếu có tiền)", "");
                d.Add("done", "Đã thực hiện giao dịch thực tế", false);
                if (d.ShowDialog(view) == DialogResult.OK)
                {
                    Services.CenterService.Check(d.Flag("done"), "Xác nhận đã đối chiếu giao dịch trước khi chốt.");
                    Execute(() => s.FinishChange(q.Id, d.TextValue("ref")));
                }
            });
    }

    void Stock()
    {
        Show("Nhập / xuất họa cụ", "", s.D.Supplys.Select(x => new StockRow(x.Id, x.Name, x.Unit, x.UnitPrice, s.Stock(x.Id), x.Threshold, s.Stock(x.Id) < x.Threshold ? "Cần bổ sung" : "Đủ tồn")).ToList(), Stock);
        Button("Lập phiếu nhập / xuất", () =>
        {
            string id = SelectedId();
            using var d = new InputForm("Giao nhận họa cụ " + id);
            d.Add("kind", "Loại phiếu", "Nhập", new[] { new Choice("Nhập", "Nhập kho"), new Choice("Xuất", "Xuất cho lớp") });
            d.Add("quantity", "Số lượng", 1);
            d.Add("class", "Lớp nhận (xuất)", "", new[] { new Choice("", "Không có") }.Concat(ClassesList()));
            d.Add("note", "Chứng từ / nội dung", "");
            if (d.ShowDialog(view) == DialogResult.OK)
            {
                Services.CenterService.Check(d.Number("quantity") > 0, "Số lượng phải lớn hơn 0.");
                Execute(() => s.MoveStock(id, (int)d.Number("quantity") * (d.TextValue("kind") == "Xuất" ? -1 : 1), d.TextValue("class"), d.TextValue("note")));
            }
        });
        Button("Lịch sử phiếu kho", () =>
        {
            Show("Phiếu kho", "Các phiếu đã xác nhận làm thay đổi tồn.", s.D.StockMoves.OrderByDescending(x => x.At).ToList(), Stock);
            Button("Quay lại tồn kho", Stock);
        });
        Button("Bán họa cụ", () =>
        {
            string id = SelectedId();
            var h = s.Get<Supply>(id);
            using var d = new InputForm("Bán họa cụ – " + h.Name);
            d.Add("buyer", "Khách mua", "");
            d.Add("quantity", "Số lượng", 1);
            d.Add("promotion", "Ưu đãi họa cụ", "", new[] { new Choice("", "Không áp dụng") }.Concat(s.D.Promotions.Where(x => x.Group == "Họa cụ").Select(x => new Choice(x.Id, x.Id + " – " + x.Name))));
            d.Add("reference", "Số chứng từ", "");
            d.Add("paid", "Đã nhận tiền", false);
            if (d.ShowDialog(view) == DialogResult.OK)
            {
                Services.CenterService.Check(d.Flag("paid"), "Cần xác nhận đã nhận tiền.");
                int quantity = checked((int)d.Number("quantity"));
                decimal subtotal = h.UnitPrice * quantity;
                var p = s.D.Promotions.FirstOrDefault(x => x.Id == d.TextValue("promotion"));
                decimal discount = p == null ? 0 : Math.Min(subtotal, Math.Min(p.Cap, Math.Round(subtotal * p.Percent / 100, 0, MidpointRounding.AwayFromZero)));
                if (Confirm($"Thành tiền: {subtotal:N0} đ\nGiảm: {discount:N0} đ\nThực thu: {subtotal - discount:N0} đ\n\nGhi nhận phiếu bán?"))
                    Execute(() => s.SellMaterial(id, quantity, d.TextValue("buyer"), d.TextValue("promotion"), d.TextValue("reference")));
            }
        });
        Button("Lịch sử bán", MaterialSales);
    }

    record StockRow([property: System.ComponentModel.DisplayName("Mã")] string Id, [property: System.ComponentModel.DisplayName("Họa cụ")] string Name, [property: System.ComponentModel.DisplayName("Đơn vị")] string Unit, [property: System.ComponentModel.DisplayName("Giá bán")] decimal UnitPrice, [property: System.ComponentModel.DisplayName("Tồn")] int Quantity, [property: System.ComponentModel.DisplayName("Ngưỡng")] int Threshold, [property: System.ComponentModel.DisplayName("Cảnh báo")] string Warning);
    void MaterialSales()
    {
        Show("Bán họa cụ", $"Số phiếu: {s.D.MaterialSales.Count} • Thực thu: {s.D.MaterialSales.Sum(x => x.Total):N0} đ", s.D.MaterialSales.OrderByDescending(x => x.At).ToList(), MaterialSales);
        if (s.Is("Họa cụ"))
            Button("Quay lại tồn kho", Stock);
    }

    void Reports()
    {
        Show("Thống kê & giao dịch", "Sổ tiền thực nhận / thực hoàn. Phần giá trị chuyển lớp không phải khoản thu mới.", s.D.Payments.OrderByDescending(x => x.At).ToList(), Reports);
        Button("Biểu đồ doanh thu", Charts);
        Button("Lọc thời gian", () =>
        {
            using var d = new InputForm("Lọc giao dịch");
            d.Add("from", "Từ ngày", new DateTime(s.Now.Year, s.Now.Month, 1));
            d.Add("to", "Đến hết ngày", s.Now.Date);
            if (d.ShowDialog(view) == DialogResult.OK)
            {
                Services.CenterService.Check(d.Date("from").Date <= d.Date("to").Date, "Khoảng ngày không hợp lệ.");
                var data = s.D.Payments.Where(x => x.At.Date >= d.Date("from").Date && x.At.Date <= d.Date("to").Date).ToList();
                Show("Giao dịch theo thời gian", $"Từ {d.Date("from"):dd/MM/yyyy} đến {d.Date("to"):dd/MM/yyyy} • Thu ròng: {data.Sum(x => x.Kind == "Thu" ? x.Amount : -x.Amount):N0} đ", data, Reports);
            }
        });
        Button("Tổng hợp theo lớp", () => Show("Tổng hợp theo lớp", "Thu / hoàn tính theo giao dịch; chuyển nội bộ được trình bày riêng.", s.D.Classrooms.Select(c =>
        {
            var ids = s.D.Enrollments.Where(x => x.ClassId == c.Id).Select(x => x.Id).ToHashSet();
            return new ClassReport(c.Id, c.Name, s.Seats(c.Id), c.Capacity, s.D.Payments.Where(x => ids.Contains(x.EnrollmentId) && x.Kind == "Thu").Sum(x => x.Amount), s.D.Payments.Where(x => ids.Contains(x.EnrollmentId) && x.Kind == "Hoàn").Sum(x => x.Amount), s.D.Enrollments.Where(x => x.ClassId == c.Id).Sum(x => x.Credit));
        }).ToList(), Reports));
        Button("Doanh thu họa cụ", MaterialSales);
    }

    record ClassReport([property: System.ComponentModel.DisplayName("Mã lớp")] string Id, [property: System.ComponentModel.DisplayName("Lớp")] string Name, [property: System.ComponentModel.DisplayName("Chỗ sử dụng")] int Seats, [property: System.ComponentModel.DisplayName("Tối đa")] int Max, [property: System.ComponentModel.DisplayName("Thực thu")] decimal Receipts, [property: System.ComponentModel.DisplayName("Thực hoàn")] decimal Refunds, [property: System.ComponentModel.DisplayName("Giá trị chuyển đến")] decimal Credit);
}
