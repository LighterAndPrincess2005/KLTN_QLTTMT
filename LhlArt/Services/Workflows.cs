using LhlArt.Models;

namespace LhlArt.Services;
public partial class CenterService
{
    public void ApproveClass(string id, bool start)
    {
        Need();
        var l = Get<Classroom>(id);
        Check(l.Status == (start ? "Tuyển sinh" : "Dự kiến"), "Trạng thái lớp không phù hợp.");
        foreach (var b in D.Lessons.Where(x => x.ClassId == id))
            CheckSchedule(b);
        Check(l.MaterialsReady, "Chưa xác nhận đủ họa cụ.");
        if (start)
        {
            Check(Now >= l.Start.AddHours(-48) && Now <= l.Start, "Chốt khai giảng trong khoảng 48 giờ trước buổi đầu.");
            Check(D.Enrollments.Count(x => x.ClassId == id && x.Status == "Đã xác nhận") >= 6, "Cần ít nhất 6 học viên đã thanh toán.");
        }

        Commit(start ? "Chốt khai giảng " + id : "Duyệt tuyển sinh " + id, () => l.Status = start ? "Đã chốt khai giảng" : "Tuyển sinh");
    }

    public void Enroll(string studentId, string classId, string promotionId)
    {
        Need("Học vụ");
        var s = Get<Student>(studentId);
        var l = Get<Classroom>(classId);
        var k = Get<Course>(l.CourseId);
        Check(s.Verified && s.ConsentStudy, "Cần xác minh thành viên / đại diện và đồng ý xử lý hồ sơ học.");
        Check(Age(s, l.Start) >= k.MinAge, "Học viên chưa đủ tuổi tối thiểu tại ngày khai giảng.");
        Check(!string.IsNullOrWhiteSpace(s.Level), "Cần có kết quả đánh giá đầu vào.");
        Check(l.Status == "Tuyển sinh" && Now >= l.Start.AddDays(-14) && Now < l.Start.AddHours(-48), "Lớp chưa đến hạn nhận hoặc đã đóng đăng ký (trước khai giảng 48 giờ).");
        Check(!D.Enrollments.Any(x => x.StudentId == studentId && x.ClassId == classId && Active(x)), "Học viên đã có đăng ký còn hiệu lực.");
        Check(Seats(classId) < l.Capacity, "Lớp đã hết chỗ (bao gồm chỗ đang giữ).");
        CheckStudentSchedule(studentId, classId);
        decimal discount = 0;
        if (promotionId != "")
        {
            var p = Get<Promotion>(promotionId);
            Check(p.Group == "Học phí", "Mã ưu đãi không áp dụng cho học phí.");
            Check(Now.Date >= p.Start.Date && Now.Date <= p.End.Date, "Mã ưu đãi ngoài thời gian áp dụng.");
            Check(s.MemberSince <= Now && Now <= s.MemberSince.AddDays(30), "Ưu đãi dành cho thành viên mới trong 30 ngày.");
            Check(D.Enrollments.Count(x => x.PromotionId == p.Id && (Active(x) || x.Status == "Hoàn thành" || x.Status == "Chưa hoàn thành" || x.Status == "Đã chuyển" || D.Payments.Any(t => t.EnrollmentId == x.Id && t.Kind == "Thu"))) < p.Limit, "Ưu đãi hết lượt.");
            discount = Math.Min(l.Fee, Math.Min(p.Cap, Math.Round(l.Fee * p.Percent / 100, 0, MidpointRounding.AwayFromZero)));
        }

        var r = new Enrollment
        {
            Id = NewId("DK"),
            StudentId = s.Id,
            ClassId = l.Id,
            Created = Now,
            Deadline = Now.AddHours(24) < l.Start.AddHours(-48) ? Now.AddHours(24) : l.Start.AddHours(-48),
            OriginalFee = l.Fee,
            Discount = discount,
            Due = l.Fee - discount,
            PromotionId = promotionId,
            Status = l.Fee - discount == 0 ? "Đã xác nhận" : "Chờ thanh toán"
        };
        Commit("Lập đăng ký " + r.Id, () =>
        {
            Expire();
            D.Enrollments.Add(r);
        });
    }

    void Reference(string reference)
    {
        Check(!string.IsNullOrWhiteSpace(reference), "Nhập số chứng từ đã đối chiếu.");
        Check(!D.Payments.Any(x => x.Reference.Equals(reference, StringComparison.OrdinalIgnoreCase)) && !D.MaterialSales.Any(x => x.Reference.Equals(reference, StringComparison.OrdinalIgnoreCase)), "Số chứng từ đã được ghi nhận, không ghi trùng.");
    }

    public void Receive(string id, decimal amount, string method, string reference)
    {
        Need("Kế toán");
        var r = Get<Enrollment>(id);
        Check(r.Status == "Chờ thanh toán" && r.Deadline > Now, "Phiếu không còn trong hạn chờ thanh toán.");
        Check(!D.ChangeRequests.Any(x => x.EnrollmentId == id && (x.Status == "Chờ duyệt" || x.Status == "Được duyệt")), "Đăng ký có yêu cầu thay đổi đang chờ. Rút yêu cầu trước khi thu thêm.");
        Check(amount > 0 && amount <= r.Due - Paid(id), "Tiền thu phải lớn hơn 0 và không vượt khoản còn thiếu. Tiền thừa trả lại ngoài phiếu.");
        Reference(reference);
        Commit("Xác nhận tiền thực nhận " + id, () =>
        {
            D.Payments.Add(new Payment { Id = NewId("GD"), EnrollmentId = id, Amount = amount, At = Now, Method = method, Reference = reference, Operator = User.Name });
            if (Paid(id) >= r.Due)
                r.Status = "Đã xác nhận";
        });
    }

    public void RefundExpired(string id, string reference)
    {
        Need("Kế toán");
        var r = Get<Enrollment>(id);
        Check((r.Status == "Hết hạn" || r.Status == "Chờ thanh toán" && r.Deadline <= Now) && Paid(id) > 0, "Chỉ hoàn khoản đã thu của phiếu hết hạn.");
        Reference(reference);
        decimal money = Paid(id);
        Commit("Hoàn khoản thiếu quá hạn " + id, () =>
        {
            r.Status = "Hết hạn";
            D.Payments.Add(new Payment { Id = NewId("GD"), EnrollmentId = id, At = Now, Kind = "Hoàn", Amount = money, Reference = reference, Operator = User.Name });
        });
    }

    public void Reschedule(string id, DateTime start, string room, string teacher, string reason)
    {
        Need("Học vụ");
        var old = Get<Lesson>(id);
        var c = Get<Classroom>(old.ClassId);
        Check(old.Start > Now && start >= Now.AddHours(24), "Chỉ đổi buổi chưa học và cần báo trước ít nhất 24 giờ.");
        Check(old.Number > 1, "Buổi khai giảng đã chốt với đăng ký. Chỉ đổi lịch từ buổi thứ 2.");
        Check(!string.IsNullOrWhiteSpace(reason), "Cần lý do đổi lịch.");
        var proposed = new Lesson
        {
            Id = old.Id,
            ClassId = old.ClassId,
            Number = old.Number,
            Start = start,
            End = start.AddMinutes(Get<Course>(c.CourseId).Minutes),
            RoomId = room,
            TeacherId = teacher
        };
        CheckSchedule(proposed);
        var before = D.Lessons.Where(x => x.ClassId == c.Id && x.Number < old.Number && x.Status != "Hủy").Max(x => x.End);
        var after = D.Lessons.Where(x => x.ClassId == c.Id && x.Number > old.Number && x.Status != "Hủy").Select(x => (DateTime? )x.Start).Min();
        Check(start > before && (after == null || proposed.End < after), "Lịch mới phải giữ đúng thứ tự nội dung buổi học.");
        var studentIds = D.Enrollments.Where(x => x.ClassId == c.Id && Active(x)).Select(x => x.StudentId).ToHashSet();
        var other = D.Enrollments.Where(x => x.ClassId != c.Id && studentIds.Contains(x.StudentId) && Active(x)).Select(x => x.ClassId).ToHashSet();
        Check(!D.Lessons.Any(x => other.Contains(x.ClassId) && x.Status != "Hủy" && Overlap(x, proposed)), "Lịch mới trùng lịch học viên.");
        Commit($"Đổi lịch {id}: {old.Start:g} → {start:g}. Lý do: {reason}", () =>
        {
            old.Start = start;
            old.End = proposed.End;
            old.RoomId = room;
            old.TeacherId = teacher;
        });
    }

    public void Mark(string enrollmentId, string lessonId, bool present, string note)
    {
        Need("Giáo viên", "Học vụ");
        var r = Get<Enrollment>(enrollmentId);
        var b = Get<Lesson>(lessonId);
        var c = Get<Classroom>(r.ClassId);
        Check(User.Role != "Giáo viên" || b.TeacherId == User.TeacherId, "Giáo viên chỉ điểm danh buổi được phân công.");
        Check(r.Status == "Đã xác nhận" && r.ClassId == b.ClassId, "Học viên chưa ghi danh hợp lệ trong lớp của buổi này.");
        Check(c.Status == "Đã chốt khai giảng" || c.Status == "Đang học", "Lớp chưa chốt khai giảng.");
        Check(b.Start.Date == Now.Date && Now >= b.Start && b.Status != "Hủy", "Chỉ điểm danh từ giờ bắt đầu và trong ngày diễn ra buổi học.");
        Check(!string.IsNullOrWhiteSpace(note), "Nhập nhận xét buổi học.");
        Commit("Điểm danh " + r.Id + " / " + b.Id, () =>
        {
            var a = D.Attendances.FirstOrDefault(x => x.EnrollmentId == r.Id && x.LessonId == b.Id);
            if (a == null)
            {
                a = new Attendance
                {
                    Id = NewId("DD"),
                    EnrollmentId = r.Id,
                    LessonId = b.Id
                };
                D.Attendances.Add(a);
            }

            a.Present = present;
            a.Note = note;
            a.Operator = User.Name;
            b.Status = "Đã tổ chức";
            c.Status = "Đang học";
        });
    }

    public int Attended(string id) => D.Attendances.Where(x => x.EnrollmentId == id && x.Present).Select(x => Get<Lesson>(x.LessonId).Number).Distinct().Count();
    public void Result(string id, decimal score, string note, bool approve)
    {
        if (approve)
            Need();
        else
            Need("Giáo viên", "Học vụ");
        var r = Get<Enrollment>(id);
        Check(r.Status == "Đã xác nhận", "Đăng ký không còn đang học.");
        Check(TeacherCan(r.ClassId), "Ngoài lớp được phân công.");
        Check(Now >= D.Lessons.Where(x => x.ClassId == r.ClassId).Max(x => x.End), "Chưa kết thúc lịch khóa học.");
        Check(score >= 0 && score <= 10, "Điểm từ 0 đến 10.");
        var k = Get<Course>(Get<Classroom>(r.ClassId).CourseId);
        var previous = D.CourseResults.FirstOrDefault(x => x.EnrollmentId == id);
        Check(!approve || previous != null, "Giáo viên cần lưu kết quả trước khi duyệt.");
        Commit("Kết quả khóa " + id, () =>
        {
            var result = previous;
            if (result == null)
            {
                result = new CourseResult
                {
                    Id = NewId("KQ"),
                    EnrollmentId = id
                };
                D.CourseResults.Add(result);
            }

            if (!approve)
            {
                result.Score = score;
                result.Note = note;
                result.Status = "Chờ duyệt";
            }
            else
            {
                result.Status = Attended(id) >= Math.Ceiling(k.Sessions * .8) && result.Score >= 5 && Paid(id) + r.Credit >= r.Due ? "Hoàn thành" : "Chưa hoàn thành";
                r.Status = result.Status;
            }
        });
    }

    public void RequestChange(string id, string kind, string target, string reason)
    {
        Need("Học vụ");
        var r = Get<Enrollment>(id);
        var c = Get<Classroom>(r.ClassId);
        Check(r.Status == "Đã xác nhận" || r.Status == "Chờ thanh toán" && r.Deadline > Now, "Đăng ký không phù hợp để gửi yêu cầu.");
        Check(!D.ChangeRequests.Any(x => x.EnrollmentId == id && (x.Status == "Chờ duyệt" || x.Status == "Được duyệt")), "Đã có yêu cầu đang xử lý.");
        Check(!string.IsNullOrWhiteSpace(reason), "Nhập lý do.");
        Check(r.SourceId == "", "Chức năng hiện tại chưa hỗ trợ hủy hoặc chuyển tiếp đăng ký đã nhận giá trị chuyển.");
        if (kind == "Chuyển")
        {
            Check(r.Status == "Đã xác nhận", "Chỉ chuyển đăng ký đã thanh toán.");
            ValidateTransfer(r, target);
        }

        var q = new ChangeRequest
        {
            Id = NewId("YC"),
            EnrollmentId = id,
            Kind = kind,
            TargetClassId = kind == "Chuyển" ? target : "",
            Reason = reason,
            At = Now
        };
        q.Refund = kind == "Hủy" ? CancelRefund(r, Now) : 0;
        q.Credit = kind == "Chuyển" ? Paid(id) : 0;
        Commit("Tiếp nhận yêu cầu " + q.Id, () => D.ChangeRequests.Add(q));
    }

    decimal CancelRefund(Enrollment r, DateTime requestedAt)
    {
        var start = Get<Classroom>(r.ClassId).Start;
        return Math.Round(Paid(r.Id) * (requestedAt <= start.AddHours(-48) ? 1m : requestedAt < start ? 0.9m : 0m), 0, MidpointRounding.AwayFromZero);
    }

    void ValidateTransfer(Enrollment r, string target, string exceptRequest = "")
    {
        var old = Get<Classroom>(r.ClassId);
        var c = Get<Classroom>(target);
        Check(r.SourceId == "" && !D.Enrollments.Any(x => x.SourceId == r.Id), "Chỉ chuyển tối đa 1 lần toàn chuỗi.");
        Check(Now < old.Start && Now < c.Start.AddHours(-48), "Chỉ chuyển trước khai giảng lớp hiện tại và trước hạn đóng đăng ký lớp đích.");
        Check(c.Id != old.Id && c.CourseId == old.CourseId && c.Status == "Tuyển sinh", "Chọn lớp khác, cùng khóa và đang tuyển sinh.");
        Check(Seats(target, exceptRequest) < c.Capacity, "Lớp đích hết chỗ.");
        CheckStudentSchedule(r.StudentId, target, r.Id);
        Check(c.Fee >= Paid(r.Id), "Chưa hỗ trợ chuyển sang lớp có học phí thấp hơn khoản đã thu.");
    }

    public void Decide(string id, bool approve, string note)
    {
        Need();
        var q = Get<ChangeRequest>(id);
        Check(q.Status == "Chờ duyệt", "Yêu cầu đã được xử lý.");
        Check(note.Trim() != "", "Nhập ý kiến quyết định.");
        if (approve && q.Kind == "Chuyển")
            ValidateTransfer(Get<Enrollment>(q.EnrollmentId), q.TargetClassId);
        Commit("Duyệt yêu cầu " + id, () =>
        {
            q.Status = approve ? "Được duyệt" : "Từ chối";
            q.Decision = note;
            q.ApprovedAt = Now;
            if (approve && q.Kind == "Hủy")
                q.Refund = CancelRefund(Get<Enrollment>(q.EnrollmentId), q.At);
        });
    }

    public void FinishChange(string id, string reference)
    {
        Need("Kế toán");
        var q = Get<ChangeRequest>(id);
        Check(q.Status == "Được duyệt", "Cần quản lý duyệt trước.");
        var r = Get<Enrollment>(q.EnrollmentId);
        Check(r.Status == "Đã xác nhận" || r.Status == "Chờ thanh toán", "Đăng ký nguồn không còn hợp lệ.");
        if (q.Kind == "Chuyển")
        {
            Check(Now < q.ApprovedAt.AddHours(24), "Quá 24 giờ sau duyệt. Hãy hủy yêu cầu và lập lại; đăng ký gốc được giữ nguyên.");
            ValidateTransfer(r, q.TargetClassId, q.Id);
        }

        decimal delta = q.Kind == "Chuyển" ? Get<Classroom>(q.TargetClassId).Fee - Paid(r.Id) : q.Refund;
        if (delta > 0)
            Reference(reference);
        Commit("Thực hiện yêu cầu " + id, () =>
        {
            if (q.Kind == "Hủy")
            {
                if (q.Refund > 0)
                    D.Payments.Add(new Payment { Id = NewId("GD"), EnrollmentId = r.Id, Kind = "Hoàn", Amount = q.Refund, At = Now, Reference = reference, Operator = User.Name });
                r.Status = "Đã hủy";
            }
            else
            {
                var c = Get<Classroom>(q.TargetClassId);
                var n = new Enrollment
                {
                    Id = NewId("DK"),
                    StudentId = r.StudentId,
                    ClassId = c.Id,
                    Created = Now,
                    Deadline = Now,
                    OriginalFee = c.Fee,
                    Due = c.Fee,
                    Credit = Paid(r.Id),
                    Status = "Đã xác nhận",
                    SourceId = r.Id
                };
                D.Enrollments.Add(n);
                if (delta > 0)
                    D.Payments.Add(new Payment { Id = NewId("GD"), EnrollmentId = n.Id, Amount = delta, At = Now, Reference = reference, Operator = User.Name });
                r.Status = "Đã chuyển";
            }

            q.Status = "Đã thực hiện";
        });
    }

    public void WithdrawRequest(string id)
    {
        Need("Học vụ");
        var q = Get<ChangeRequest>(id);
        Check(q.Status == "Chờ duyệt" || q.Status == "Được duyệt", "Yêu cầu đã kết thúc.");
        Commit("Rút yêu cầu " + id, () => q.Status = "Đã rút");
    }

    public void MoveStock(string id, int quantity, string classId, string note)
    {
        Need("Họa cụ");
        Get<Supply>(id);
        Check(quantity != 0, "Số lượng phải khác 0.");
        Check(Stock(id) + quantity >= 0, "Không đủ tồn kho để xuất.");
        Check(note.Trim() != "", "Nhập chứng từ / nội dung giao nhận.");
        Check(quantity > 0 || D.Classrooms.Any(x => x.Id == classId), "Xuất kho cần lớp nhận.");
        Commit("Giao dịch họa cụ " + id, () => D.StockMoves.Add(new StockMove { Id = NewId("PK"), SupplyId = id, Quantity = quantity, ClassId = quantity < 0 ? classId : "", At = Now, Note = note, Operator = User.Name }));
    }

    public MaterialSale SellMaterial(string supplyId, int quantity, string buyer, string promotionId, string reference)
    {
        Need("Họa cụ");
        var supply = Get<Supply>(supplyId);
        Check(quantity > 0 && Stock(supplyId) >= quantity, "Số lượng bán không hợp lệ hoặc không đủ tồn kho.");
        Check(supply.UnitPrice > 0, "Cần nhập giá bán cho họa cụ trước khi lập phiếu.");
        Check(!string.IsNullOrWhiteSpace(buyer), "Nhập tên khách mua.");
        Check(!string.IsNullOrWhiteSpace(reference), "Nhập số chứng từ.");
        Check(!D.MaterialSales.Any(x => x.Reference.Equals(reference, StringComparison.OrdinalIgnoreCase)) && !D.Payments.Any(x => x.Reference.Equals(reference, StringComparison.OrdinalIgnoreCase)), "Số chứng từ đã được ghi nhận.");
        decimal subtotal = supply.UnitPrice * quantity, discount = 0;
        if (promotionId != "")
        {
            var p = Get<Promotion>(promotionId);
            Check(p.Group == "Họa cụ", "Mã ưu đãi không áp dụng cho họa cụ.");
            Check(Now.Date >= p.Start.Date && Now.Date <= p.End.Date, "Mã ưu đãi ngoài thời gian áp dụng.");
            Check(D.MaterialSales.Count(x => x.PromotionId == p.Id) < p.Limit, "Ưu đãi hết lượt.");
            discount = Math.Min(subtotal, Math.Min(p.Cap, Math.Round(subtotal * p.Percent / 100, 0, MidpointRounding.AwayFromZero)));
        }

        var sale = new MaterialSale
        {
            Id = NewId("BH"),
            SupplyId = supplyId,
            Quantity = quantity,
            Buyer = buyer.Trim(),
            UnitPrice = supply.UnitPrice,
            Subtotal = subtotal,
            PromotionId = promotionId,
            Discount = discount,
            Total = subtotal - discount,
            Reference = reference.Trim(),
            At = Now,
            Operator = User.Name
        };
        Commit("Bán họa cụ " + sale.Id, () =>
        {
            D.MaterialSales.Add(sale);
            D.StockMoves.Add(new StockMove { Id = NewId("PK"), SupplyId = supplyId, Quantity = -quantity, At = Now, Note = "Xuất bán – " + sale.Reference, Operator = User.Name });
        });
        return sale;
    }
}
