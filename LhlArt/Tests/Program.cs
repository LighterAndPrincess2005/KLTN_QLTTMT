using LhlArt.Data;
using LhlArt.Models;
using LhlArt.Services;

int passed = 0;
var temp = Path.Combine(Path.GetTempPath(), "MyThuatTests_" + Guid.NewGuid());
void Assert(bool condition, string name)
{
    if (!condition)
        throw new Exception("FAIL: " + name);
    Console.WriteLine("PASS: " + name);
    passed++;
}

void Reject(Action action, string name)
{
    bool rejected = false;
    try
    {
        action();
    }
    catch
    {
        rejected = true;
    }

    Assert(rejected, name);
}

try
{
    var path = Path.Combine(temp, "data.json");
    new JsonStore(path).Save(Seed.Create());
    DateTime time = new DateTime(2026, 10, 5, 9, 0, 0);
    var s = new CenterService(new JsonStore(path), () => time);
    Assert(s.Login("admin", "123456"), "Dang nhap admin");
    Assert(s.D.Courses.Count == 6 && s.D.Lessons.Count == 88, "6 khoa, 88 buoi");
    s.Enroll("HV07", "L02", "KM01");
    var r = s.D.Enrollments.Last();
    string id = r.Id;
    Assert(r.Discount == 300000 && r.Due == 3300000, "Tran uu dai 300.000");
    Reject(() => s.Enroll("HV07", "L02", ""), "Chan dang ky trung");
    s.Receive(id, 1000000, "Tiền mặt", "TEST01");
    Assert(s.Get<Enrollment>(id).Status == "Chờ thanh toán", "Thu thieu chua xac nhan");
    Reject(() => s.Receive(id, 1000000, "Tiền mặt", "TEST01"), "Chan chung tu trung");
    Reject(() => s.Receive(id, 3000000, "Tiền mặt", "TEST02"), "Chan thu vuot");
    s.Receive(id, 2300000, "Tiền mặt", "TEST02");
    Assert(s.Get<Enrollment>(id).Status == "Đã xác nhận", "Thu du xac nhan");
    s.RequestChange(id, "Hủy", "", "Kiểm thử hủy trước khai giảng");
    string q = s.D.ChangeRequests.Last().Id;
    s.Decide(q, true, "Duyệt hoàn");
    s.FinishChange(q, "TEST-REFUND");
    Assert(s.Paid(id) == 0 && s.Get<Enrollment>(id).Status == "Đã hủy", "Hoan du truoc 48 gio");
    Reject(() => s.FinishChange(q, "TEST-REFUND2"), "Chan hoan lan hai");
    int stock = s.Stock("HC03");
    Reject(() => s.MoveStock("HC03", -(stock + 1), "L02", "Test"), "Chan ton am");
    Assert(s.Stock("HC03") == stock, "Ton giu nguyen khi loi");
    s.MoveStock("HC03", -20, "L02", "Xuất kiểm thử");
    Assert(s.Stock("HC03") == stock - 20, "Xuat kho dung");
    int crayons = s.Stock("HC01");
    Reject(() => s.SellMaterial("HC01", crayons + 1, "Khách thử", "KM02", "SALE-OVER"), "Chan ban vuot ton");
    Reject(() => s.SellMaterial("HC01", 1, "Khách thử", "KM01", "SALE-WRONG"), "Chan uu dai sai nhom");
    var sale = s.SellMaterial("HC01", 2, "Khách thử", "KM02", "SALE-01");
    Assert(sale.Subtotal == 90000 && sale.Discount == 9000 && sale.Total == 81000, "Tinh uu dai hoa cu");
    Assert(s.Stock("HC01") == crayons - 2 && s.D.StockMoves.Last().Quantity == -2, "Ban tru ton kho");
    Reject(() => s.SellMaterial("HC01", 1, "Khách thử", "KM02", "SALE-01"), "Chan chung tu ban trung");
    Reject(() => s.Enroll("HV08", "L02", "KM02"), "Chan uu dai hoa cu cho hoc phi");
    var future = new Classroom
    {
        Id = "L07",
        Name = "Foundation 1 chiều Chủ nhật",
        CourseId = "KH02",
        TeacherId = "GV02",
        RoomId = "P01",
        Start = new DateTime(2026, 10, 18, 13, 0, 0),
        Capacity = 10,
        Fee = 3600000,
        MaterialsReady = true
    };
    s.Save(future, true);
    s.ApproveClass("L07", false);
    s.Enroll("HV07", "L02", "KM01");
    string source = s.D.Enrollments.Last().Id;
    s.Receive(source, 3300000, "Tiền mặt", "TRANSFER-SOURCE");
    s.RequestChange(source, "Chuyển", "L07", "Đổi ca học");
    string transfer = s.D.ChangeRequests.Last().Id;
    s.Decide(transfer, true, "Đồng ý đổi ca");
    s.FinishChange(transfer, "TRANSFER-DELTA");
    var target = s.D.Enrollments.Single(x => x.SourceId == source);
    Assert(s.Get<Enrollment>(source).Status == "Đã chuyển" && target.Credit == 3300000 && s.Paid(target.Id) == 300000, "Chuyen gia tri va thu chenh lech");
    Assert(s.D.Payments.Sum(x => x.Kind == "Thu" ? x.Amount : -x.Amount) == 6 * 3600000 + 3300000 + 300000, "Khong tinh chuyen noi bo thanh thu moi");
    Reject(() => s.FinishChange(transfer, "TRANSFER-AGAIN"), "Chan chuyen lan hai");
    var reloaded = new CenterService(new JsonStore(path));
    Assert(reloaded.Stock("HC03") == stock - 20, "Luu va doc lai JSON");
    Assert(reloaded.D.MaterialSales.Single().Total == 81000 && reloaded.Stock("HC01") == crayons - 2, "Luu phieu ban va ton kho");
    var old = Seed.Create();
    old.SchemaVersion = 1;
    old.Promotions.RemoveAll(x => x.Id == "KM02");
    old.Supplys.Single(x => x.Id == "HC01").UnitPrice = 0;
    var oldPath = Path.Combine(temp, "old.json");
    new JsonStore(oldPath).Save(old);
    var upgraded = new CenterService(new JsonStore(oldPath));
    Assert(!upgraded.D.Promotions.Any(x => x.Id == "KM02") && upgraded.D.Supplys.Single(x => x.Id == "HC01").UnitPrice == 0 && upgraded.D.Enrollments.Count == 6, "Doc JSON cu khong tu them uu dai hay doi gia");
    s.Login("hocvu", "123456");
    Reject(() => s.Receive("DK01", 1, "Tiền mặt", "BAD"), "Hoc vu khong duoc thu tien");
    s.Login("admin", "123456");
    s.Enroll("HV08", "L02", "");
    string pending = s.D.Enrollments.Last().Id;
    s.Receive(pending, 100000, "Tiền mặt", "PARTIAL");
    time = new DateTime(2026, 10, 6, 10, 0, 0);
    s.RefreshTime();
    Assert(s.Get<Enrollment>(pending).Status == "Hết hạn", "Tu het han sau 24 gio");
    s.RefundExpired(pending, "REFUND-PARTIAL");
    Assert(s.Paid(pending) == 0, "Hoan phan thu thieu");
    time = new DateTime(2026, 10, 15, 9, 45, 0);
    s.RefreshTime();
    s.ApproveClass("L02", true);
    time = new DateTime(2026, 10, 17, 10, 0, 0);
    s.RefreshTime();
    s.Mark("DK01", "BH0201", true, "Tốt");
    s.Mark("DK01", "BH0201", false, "Cập nhật vắng");
    Assert(s.D.Attendances.Count(x => x.EnrollmentId == "DK01" && x.LessonId == "BH0201") == 1, "Diem danh khong nhan doi");
    s.Login("giaovien", "123456");
    Reject(() => s.MoveStock("HC03", 10, "", "Test"), "Giao vien khong duoc sua kho");
    var emptyPath = Path.Combine(temp, "fresh.json");
    var fresh = new CenterService(new JsonStore(emptyPath));
    Assert(fresh.D.Accounts.Count == 0 && fresh.D.Students.Count == 0 && fresh.D.Courses.Count == 0 && fresh.D.Payments.Count == 0, "Cai moi khong tao tai khoan hoac du lieu san");
    Reject(() => fresh.InitializeAdministrator("admin", "123456"), "Chan mat khau khoi tao qua ngan");
    fresh.InitializeAdministrator("quanly", "Matkhau123!");
    Assert(fresh.Login("quanly", "Matkhau123!") && fresh.Is(), "Khoi tao va dang nhap quan tri");
    Reject(() => fresh.InitializeAdministrator("khac", "Matkhau123!"), "Khong khoi tao lai quan tri");
    Assert(new CenterService(new JsonStore(emptyPath)).Login("quanly", "Matkhau123!"), "Tai khoan quan tri duoc luu");
    Assert(Math.Abs((fresh.Now - BusinessTime.Now).TotalSeconds) < 2, "Ung dung su dung gio Viet Nam hien tai");
    var raw = File.ReadAllText(oldPath).Replace("\"SchemaVersion\": 1", "\"SchemaVersion\": 1, \"DemoNow\": \"2026-10-05T09:00:00\"");
    File.WriteAllText(oldPath, raw);
    var historical = new CenterService(new JsonStore(oldPath));
    Assert(historical.D.Students.Count == 8 && Math.Abs((historical.Now - BusinessTime.Now).TotalSeconds) < 2, "Giu ho so cu va bo qua dong ho cu");
    s.Login("admin", "123456");
    s.SaveAccount("NVTEST", "nhanvien", "Học vụ", "", true, "Matkhau123!", true);
    Assert(s.D.Accounts.Any(x => x.Name == "nhanvien" && x.Role == "Học vụ"), "Tao tai khoan hoc vu");
    Reject(() => s.SaveAccount("NV2", "NHANVIEN", "Kế toán", "", true, "Matkhau123!", true), "Chan ten tai khoan trung khac chu hoa");
    Reject(() => s.SaveAccount("GVTEST", "gvtest", "Giáo viên", "", true, "Matkhau123!", true), "Giao vien phai lien ket ho so");
    Reject(() => s.SaveAccount("GVTEST", "gvtest", "Giáo viên", "GV02", true, "Matkhau123!", true), "Chan hai tai khoan gan cung giao vien");
    s.SaveAccount("NVTEST", "nhanvien", "Kế toán", "", false, "", false);
    Assert(!s.Login("nhanvien", "Matkhau123!"), "Tai khoan khoa khong dang nhap");
    s.Login("admin", "123456");
    s.SaveAccount("NVTEST", "nhanvien", "Kế toán", "", true, "", false);
    s.ResetAccountPassword("NVTEST", "MatkhauMoi123!");
    Assert(!s.Login("nhanvien", "Matkhau123!") && s.Login("nhanvien", "MatkhauMoi123!"), "Dat lai mat khau va mo khoa");
    Reject(() => s.SaveAccount("BAD", "nguoila", "Quản lý", "", true, "Matkhau123!", true), "Ke toan khong tao tai khoan");
    Reject(() => s.Save(new Account { Id = "BAD", Name = "nguoila", Role = "Quản lý" }, true), "Chan sua tai khoan qua danh muc");
    s.Login("admin", "123456");
    Reject(() => s.SaveAccount(s.User.Id, s.User.Name, "Quản lý", "", false, "", false), "Khong tu khoa tai khoan hien tai");
    Reject(() => s.SaveAccount(s.User.Id, s.User.Name, "Học vụ", "", true, "", false), "Khong tu ha quyen quan tri");
    Assert(new CenterService(new JsonStore(path)).Login("nhanvien", "MatkhauMoi123!"), "Luu tai khoan quyen va mat khau moi");
    var monthly = s.MonthlyReport();
    Assert(monthly.Count == 6 && monthly.Last().Month.Month == time.Month && monthly.Last().Tuition - monthly.Last().Refunds == s.D.Payments.Sum(x => x.Kind == "Thu" ? x.Amount : -x.Amount), "Bieu do tinh dung thu hoan khong cong chuyen lop");
    Assert(monthly.Last().Materials == 81000, "Bieu do tach rieng doanh thu hoa cu");
    s.Login("giaovien", "123456");
    Reject(() => s.MonthlyReport(), "Giao vien khong xem bao cao tai chinh");
    Console.WriteLine($"\nHoan tat {passed} kiem tra.");
}
finally
{
    if (Directory.Exists(temp))
        Directory.Delete(temp, true);
}
