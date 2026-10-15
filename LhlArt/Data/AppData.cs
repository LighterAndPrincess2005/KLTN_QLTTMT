using LhlArt.Models;

namespace LhlArt.Data;
public class AppData
{
    public int SchemaVersion { get; set; } = 3;
    public List<CourseType> CourseTypes { get; set; } = new();
    public List<Course> Courses { get; set; } = new();
    public List<Teacher> Teachers { get; set; } = new();
    public List<Room> Rooms { get; set; } = new();
    public List<Student> Students { get; set; } = new();
    public List<Classroom> Classrooms { get; set; } = new();
    public List<Lesson> Lessons { get; set; } = new();
    public List<Promotion> Promotions { get; set; } = new();
    public List<Supply> Supplys { get; set; } = new();
    public List<Enrollment> Enrollments { get; set; } = new();
    public List<Payment> Payments { get; set; } = new();
    public List<Attendance> Attendances { get; set; } = new();
    public List<ChangeRequest> ChangeRequests { get; set; } = new();
    public List<StockMove> StockMoves { get; set; } = new();
    public List<MaterialSale> MaterialSales { get; set; } = new();
    public List<CourseResult> CourseResults { get; set; } = new();
    public List<Audit> Audits { get; set; } = new();
    public List<Account> Accounts { get; set; } = new();
}
