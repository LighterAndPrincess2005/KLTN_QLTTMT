using System.Text.Json;
using LhlArt.Models;
using LhlArt.Services;
using LhlArt.Views;
using LhlArt.Views.Shared;

namespace LhlArt.Controllers;
public partial class MainController
{
    readonly CenterService s;
    readonly MainForm view;
    Action refresh = () =>
    {
    };
    readonly Dictionary<string, Action> routes = new();
    public MainController(CenterService service, MainForm form)
    {
        s = service;
        view = form;
        view.RefreshRequested += () => Safe(refresh);
        view.NavigateRequested += Navigate;
        view.TimeRequested += () => view.UpdateTime(s.Now);
        ConfigureNavigation();
        Dashboard();
        view.SelectNavigation("Tổng quan");
    }

    void Navigate(string title)
    {
        if (!routes.TryGetValue(title, out var action))
            return;
        view.SelectNavigation(title);
        Safe(action);
    }

    void ConfigureNavigation()
    {
        void Item(string title, Action action)
        {
            routes[title] = action;
            view.AddNavigation(title, () => Safe(action));
        }

        void Group(string title, params (string Title, Action Action)[] entries)
        {
            foreach (var entry in entries)
                routes[entry.Title] = entry.Action;
            view.AddNavigationGroup(title, entries.Select(x => (x.Title, (Action)(() => Safe(x.Action)))).ToArray());
        }

        Item("Tổng quan", Dashboard);
        if (s.Is("Học vụ"))
        {
            Group("Đào tạo", ("Loại khóa học", () => Catalog<CourseType>("Loại khóa học", "")), ("Khóa học", () => Catalog<Course>("Khóa học", "")), ("Lớp học", Classes), ("Phòng học", () => Catalog<Room>("Phòng học", "")));
            Group("Hồ sơ", ("Học viên & tư vấn", () => Catalog<Student>("Học viên & tư vấn", "")), ("Giáo viên", () => Catalog<Teacher>("Giáo viên", "")));
        }

        if (s.Is("Học vụ", "Kế toán"))
            Group("Đăng ký & học phí", ("Đăng ký & thu học phí", Enrollments), ("Yêu cầu hủy / chuyển", Changes));
        if (s.Is("Học vụ", "Giáo viên"))
            Group("Theo dõi học tập", ("Lịch học & điểm danh", Lessons), ("Kết quả học tập", Results));
        if (s.Is("Họa cụ"))
            Group("Họa cụ & kho", ("Danh mục họa cụ", () => Catalog<Supply>("Danh mục họa cụ", "")), ("Nhập / xuất họa cụ", Stock), ("Lịch sử bán", MaterialSales));
        if (s.Is("Học vụ", "Họa cụ"))
            Item("Ưu đãi", () => Catalog<Promotion>("Ưu đãi", ""));
        if (s.Is("Kế toán"))
            Group("Báo cáo", ("Thống kê & giao dịch", Reports), ("Biểu đồ doanh thu", Charts), ("Doanh thu họa cụ", MaterialSales));
        if (s.Is())
            Group("Quản trị", ("Tài khoản nhân viên", Accounts), ("Nhật ký thao tác", ShowAudit), ("Sao lưu dữ liệu", Backup));
        Group("Tài khoản", ("Đổi mật khẩu", Password), ("Đăng xuất", () =>
        {
            if (Confirm("Đăng xuất tài khoản hiện tại?"))
                view.SignOut();
        }));
    }

    void Charts()
    {
        var data = s.MonthlyReport();
        refresh = Charts;
        view.RenderStatistics(data, s.Now);
    }

    void Safe(Action action)
    {
        try
        {
            action();
        }
        catch (Exception ex)
        {
            MessageBox.Show(view, ex.Message, "Kiểm tra thông tin", MessageBoxButtons.OK, MessageBoxIcon.Warning);
        }
    }

    void Button(string text, Action action) => view.AddAction(text, () => Safe(action));
    void Show<T>(string title, string note, IEnumerable<T> data, Action reload)
    {
        s.RefreshTime();
        refresh = reload;
        view.RenderList(title, data, s.Now);
    }

    T Selected<T>() => view.Selected<T>();
    string SelectedId() => view.SelectedId();
    bool Confirm(string text) => MessageBox.Show(view, text, "Xác nhận thao tác", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes;
    void Execute(Action action)
    {
        action();
        refresh();
    }

    IEnumerable<Choice> Students() => s.D.Students.Select(x => new Choice(x.Id, x.Id + " – " + x.Name));
    IEnumerable<Choice> ClassesList() => s.D.Classrooms.Select(x => new Choice(x.Id, x.Id + " – " + x.Name));
    void ShowAudit() => Show("Nhật ký thao tác", "", s.D.Audits.OrderByDescending(x => x.At).ToList(), ShowAudit);
    void Catalog<T>(string title, string note)
        where T : Entity, new()
    {
        Show(title, note, s.List<T>(), () => Catalog<T>(title, note));
        if (typeof(T) == typeof(Promotion) && !s.Is())
            return;
        void Edit(bool create)
        {
            T item = create ? new T() : JsonSerializer.Deserialize<T>(JsonSerializer.Serialize(Selected<T>()))!;
            if (create)
                item.Id = CenterService.NewId(typeof(T).Name[..2].ToUpperInvariant());
            using var d = FormFactory.Edit(s, item, create, title);
            d.ValidateAndSave = () =>
            {
                d.Fill(item);
                s.Save(item, create);
            };
            if (d.ShowDialog(view) == DialogResult.OK)
                refresh();
        }

        Button("Thêm", () => Edit(true));
        Button("Sửa", () => Edit(false));
        Button("Xóa", () =>
        {
            var x = Selected<T>();
            if (Confirm("Xóa hồ sơ " + x.Id + "?"))
                Execute(() => s.Delete(x));
        });
    }

    void Backup()
    {
        using var f = new SaveFileDialog
        {
            Filter = "JSON (*.json)|*.json",
            FileName = "MyThuat_Backup.json"
        };
        if (f.ShowDialog(view) == DialogResult.OK)
            File.Copy(s.DataPath, f.FileName, true);
    }

    void Password()
    {
        using var d = new InputForm("Đổi mật khẩu");
        d.Add("old", "Mật khẩu cũ", "", password: true);
        d.Add("new", "Mật khẩu mới", "", password: true);
        d.Add("again", "Nhập lại", "", password: true);
        if (d.ShowDialog(view) == DialogResult.OK)
        {
            CenterService.Check(d.TextValue("new") == d.TextValue("again"), "Hai mật khẩu mới không khớp.");
            s.ChangePassword(d.TextValue("old"), d.TextValue("new"));
            MessageBox.Show("Đã đổi mật khẩu.");
        }
    }
}
