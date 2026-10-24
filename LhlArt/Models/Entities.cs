using System.ComponentModel;

namespace LhlArt.Models;
public abstract class Entity
{
    [DisplayName("Mã")]
    public string Id { get; set; } = "";
}

public class CourseType : Entity
{
    [DisplayName("Tên loại")]
    public string Name { get; set; } = "";

    [DisplayName("Mô tả")]
    public string Description { get; set; } = "";
}

public class Course : Entity
{
    [DisplayName("Tên khóa")]
    public string Name { get; set; } = "";

    [DisplayName("Loại khóa")]
    public string TypeId { get; set; } = "";

    [DisplayName("Tuổi tối thiểu")]
    public int MinAge { get; set; } = 3;

    [DisplayName("Số buổi")]
    public int Sessions { get; set; } = 16;

    [DisplayName("Phút mỗi buổi")]
    public int Minutes { get; set; } = 120;

    [DisplayName("Học phí")]
    public decimal Fee { get; set; } = 3600000;

    [DisplayName("Yêu cầu đầu vào")]
    public string Level { get; set; } = "Cơ bản";
}

public class Teacher : Entity
{
    [DisplayName("Họ tên")]
    public string Name { get; set; } = "";

    [DisplayName("Điện thoại")]
    public string Phone { get; set; } = "";

    [DisplayName("Chuyên môn")]
    public string Specialty { get; set; } = "";
}

public class Room : Entity
{
    [DisplayName("Tên phòng")]
    public string Name { get; set; } = "";

    [DisplayName("Sức chứa")]
    public int Capacity { get; set; } = 10;
}

public class Student : Entity
{
    [DisplayName("Họ tên học viên")]
    public string Name { get; set; } = "";

    [DisplayName("Ngày sinh")]
    public DateTime Birth { get; set; } = new DateTime(2017, 1, 1);

    [DisplayName("Thành viên / đại diện")]
    public string Guardian { get; set; } = "";

    [DisplayName("Quan hệ")]
    public string Relation { get; set; } = "Cha/mẹ";

    [DisplayName("Điện thoại liên hệ")]
    public string Phone { get; set; } = "";

    [DisplayName("Email liên hệ")]
    public string Email { get; set; } = "";

    [DisplayName("Đã xác minh đại diện")]
    public bool Verified { get; set; } = false;

    [DisplayName("Ngày xác nhận thành viên")]
    public DateTime MemberSince { get; set; } = LhlArt.Services.BusinessTime.Now.Date;

    [DisplayName("Đồng ý xử lý hồ sơ học")]
    public bool ConsentStudy { get; set; } = false;

    [DisplayName("Đồng ý nhận quảng bá")]
    public bool ConsentMarketing { get; set; } = false;

    [DisplayName("Đánh giá đầu vào")]
    public string Level { get; set; } = "Cơ bản";

    [DisplayName("Nhu cầu / tư vấn")]
    public string Consultation { get; set; } = "";
}

public class Classroom : Entity
{
    [DisplayName("Tên lớp")]
    public string Name { get; set; } = "";

    [DisplayName("Khóa học")]
    public string CourseId { get; set; } = "";

    [DisplayName("Giáo viên chính")]
    public string TeacherId { get; set; } = "";

    [DisplayName("Trợ giảng (Junior)")]
    public string AssistantId { get; set; } = "";

    [DisplayName("Phòng")]
    public string RoomId { get; set; } = "";

    [DisplayName("Ngày giờ khai giảng")]
    public DateTime Start { get; set; } = LhlArt.Services.BusinessTime.Now.Date.AddDays(7).AddHours(8);

    [DisplayName("Sĩ số tối đa")]
    public int Capacity { get; set; } = 10;

    [DisplayName("Học phí áp dụng")]
    public decimal Fee { get; set; } = 3600000;

    [DisplayName("Trạng thái")]
    public string Status { get; set; } = "Dự kiến";

    [DisplayName("Đã kiểm tra đủ họa cụ")]
    public bool MaterialsReady { get; set; } = false;
}

public class Lesson : Entity
{
    [DisplayName("Lớp")]
    public string ClassId { get; set; } = "";

    [DisplayName("Buổi nội dung")]
    public int Number { get; set; } = 1;

    [DisplayName("Bắt đầu")]
    public DateTime Start { get; set; } = LhlArt.Services.BusinessTime.Now.Date.AddDays(7).AddHours(8);

    [DisplayName("Kết thúc")]
    public DateTime End { get; set; } = LhlArt.Services.BusinessTime.Now.Date.AddDays(7).AddHours(10);

    [DisplayName("Giáo viên")]
    public string TeacherId { get; set; } = "";

    [DisplayName("Phòng")]
    public string RoomId { get; set; } = "";

    [DisplayName("Chủ đề")]
    public string Topic { get; set; } = "";

    [DisplayName("Trạng thái")]
    public string Status { get; set; } = "Dự kiến";
}

public class Promotion : Entity
{
    [DisplayName("Tên ưu đãi")]
    public string Name { get; set; } = "";

    [DisplayName("Áp dụng cho")]
    public string Group { get; set; } = "Học phí";

    [DisplayName("Từ ngày")]
    public DateTime Start { get; set; } = LhlArt.Services.BusinessTime.Now.Date;

    [DisplayName("Đến ngày")]
    public DateTime End { get; set; } = LhlArt.Services.BusinessTime.Now.Date.AddMonths(1);

    [DisplayName("Giảm %")]
    public decimal Percent { get; set; } = 10;

    [DisplayName("Giảm tối đa")]
    public decimal Cap { get; set; } = 300000;

    [DisplayName("Tổng lượt")]
    public int Limit { get; set; } = 20;
}

public class Supply : Entity
{
    [DisplayName("Tên họa cụ")]
    public string Name { get; set; } = "";

    [DisplayName("Đơn vị")]
    public string Unit { get; set; } = "Tờ";

    [DisplayName("Giá bán")]
    public decimal UnitPrice { get; set; } = 0;

    [DisplayName("Ngưỡng bổ sung")]
    public int Threshold { get; set; } = 40;
}

public class MaterialSale : Entity
{
    [DisplayName("Họa cụ")]
    public string SupplyId { get; set; } = "";

    [DisplayName("Khách mua")]
    public string Buyer { get; set; } = "";

    [DisplayName("Số lượng")]
    public int Quantity { get; set; } = 0;

    [DisplayName("Đơn giá")]
    public decimal UnitPrice { get; set; } = 0;

    [DisplayName("Thành tiền")]
    public decimal Subtotal { get; set; } = 0;

    [DisplayName("Ưu đãi")]
    public string PromotionId { get; set; } = "";

    [DisplayName("Giảm giá")]
    public decimal Discount { get; set; } = 0;

    [DisplayName("Thực thu")]
    public decimal Total { get; set; } = 0;

    [DisplayName("Số chứng từ")]
    public string Reference { get; set; } = "";

    [DisplayName("Thời điểm")]
    public DateTime At { get; set; } = LhlArt.Services.BusinessTime.Now;

    [DisplayName("Người ghi")]
    public string Operator { get; set; } = "";
}

public class Enrollment : Entity
{
    [DisplayName("Học viên")]
    public string StudentId { get; set; } = "";

    [DisplayName("Lớp")]
    public string ClassId { get; set; } = "";

    [DisplayName("Ngày lập")]
    public DateTime Created { get; set; } = LhlArt.Services.BusinessTime.Now;

    [DisplayName("Hạn thanh toán")]
    public DateTime Deadline { get; set; } = LhlArt.Services.BusinessTime.Now;

    [DisplayName("Học phí gốc")]
    public decimal OriginalFee { get; set; } = 0;

    [DisplayName("Giảm giá")]
    public decimal Discount { get; set; } = 0;

    [DisplayName("Học phí chốt")]
    public decimal Due { get; set; } = 0;

    [DisplayName("Mã ưu đãi")]
    public string PromotionId { get; set; } = "";

    [DisplayName("Trạng thái")]
    public string Status { get; set; } = "Chờ thanh toán";

    [DisplayName("Đăng ký nguồn")]
    public string SourceId { get; set; } = "";

    [DisplayName("Giá trị chuyển đến")]
    public decimal Credit { get; set; } = 0;
}

public class Payment : Entity
{
    [DisplayName("Đăng ký")]
    public string EnrollmentId { get; set; } = "";

    [DisplayName("Thời điểm")]
    public DateTime At { get; set; } = LhlArt.Services.BusinessTime.Now;

    [DisplayName("Loại giao dịch")]
    public string Kind { get; set; } = "Thu";

    [DisplayName("Số tiền")]
    public decimal Amount { get; set; } = 0;

    [DisplayName("Phương thức")]
    public string Method { get; set; } = "Tiền mặt";

    [DisplayName("Số chứng từ")]
    public string Reference { get; set; } = "";

    [DisplayName("Người ghi")]
    public string Operator { get; set; } = "";
}

public class Attendance : Entity
{
    [DisplayName("Đăng ký")]
    public string EnrollmentId { get; set; } = "";

    [DisplayName("Buổi học")]
    public string LessonId { get; set; } = "";

    [DisplayName("Có mặt")]
    public bool Present { get; set; } = false;

    [DisplayName("Nhận xét")]
    public string Note { get; set; } = "";

    [DisplayName("Người ghi")]
    public string Operator { get; set; } = "";
}

public class ChangeRequest : Entity
{
    [DisplayName("Đăng ký gốc")]
    public string EnrollmentId { get; set; } = "";

    [DisplayName("Loại yêu cầu")]
    public string Kind { get; set; } = "Hủy";

    [DisplayName("Lớp đích")]
    public string TargetClassId { get; set; } = "";

    [DisplayName("Lý do")]
    public string Reason { get; set; } = "";

    [DisplayName("Ngày gửi")]
    public DateTime At { get; set; } = LhlArt.Services.BusinessTime.Now;

    [DisplayName("Ngày duyệt")]
    public DateTime ApprovedAt { get; set; } = DateTime.MinValue;

    [DisplayName("Tiền hoàn")]
    public decimal Refund { get; set; } = 0;

    [DisplayName("Giá trị chuyển")]
    public decimal Credit { get; set; } = 0;

    [DisplayName("Trạng thái")]
    public string Status { get; set; } = "Chờ duyệt";

    [DisplayName("Ý kiến duyệt")]
    public string Decision { get; set; } = "";
}

public class StockMove : Entity
{
    [DisplayName("Họa cụ")]
    public string SupplyId { get; set; } = "";

    [DisplayName("Lớp nhận (nếu xuất)")]
    public string ClassId { get; set; } = "";

    [DisplayName("Số lượng (+nhập / -xuất)")]
    public int Quantity { get; set; } = 0;

    [DisplayName("Ngày ghi")]
    public DateTime At { get; set; } = LhlArt.Services.BusinessTime.Now;

    [DisplayName("Nội dung / chứng từ")]
    public string Note { get; set; } = "";

    [DisplayName("Người ghi")]
    public string Operator { get; set; } = "";
}

public class CourseResult : Entity
{
    [DisplayName("Đăng ký")]
    public string EnrollmentId { get; set; } = "";

    [DisplayName("Điểm cuối khóa")]
    public decimal Score { get; set; } = 0;

    [DisplayName("Nhận xét tổng kết")]
    public string Note { get; set; } = "";

    [DisplayName("Kết quả")]
    public string Status { get; set; } = "Chờ duyệt";
}

public class Audit : Entity
{
    [DisplayName("Thời điểm")]
    public DateTime At { get; set; } = LhlArt.Services.BusinessTime.Now;

    [DisplayName("Tài khoản")]
    public string User { get; set; } = "";

    [DisplayName("Thao tác")]
    public string Action { get; set; } = "";
}

public class Account : Entity
{
    [DisplayName("Đang hoạt động")]
    public bool Enabled { get; set; } = true;

    [DisplayName("Tài khoản")]
    public string Name { get; set; } = "";

    [DisplayName("Vai trò")]
    public string Role { get; set; } = "";

    [DisplayName("Mã giáo viên")]
    public string TeacherId { get; set; } = "";

    [DisplayName("Mật khẩu băm")]
    public string PasswordHash { get; set; } = "";
}
