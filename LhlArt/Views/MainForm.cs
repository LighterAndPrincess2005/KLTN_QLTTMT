using System.Collections;
using System.Text;
using LhlArt.Models;
using LhlArt.Views.Shared;

namespace LhlArt.Views;
public sealed class MainForm : Form
{
    readonly DataGridView grid = new();
    readonly System.Windows.Forms.Timer clockTimer = new()
    {
        Interval = 30000
    };
    readonly Panel content = new();
    readonly FlowLayoutPanel actions = new();
    readonly Label heading = new() { UseMnemonic = false };
    readonly Label subtitle = new() { UseMnemonic = false };
    readonly Label status = new() { UseMnemonic = false };
    readonly Label clock = new() { UseMnemonic = false };
    readonly Label empty = new() { UseMnemonic = false };
    readonly TextBox search = new();
    readonly TableLayoutPanel body = new();
    readonly NavigationMenu menu;
    List<object> rows = new();
    Type? rowType;
    DateTime now;
    bool dashboard;
    public bool Logout { get; private set; }

    public event Action? RefreshRequested;
    public event Action? TimeRequested;
    public event Action<string>? NavigateRequested;
    public MainForm(string user, string role)
    {
        Text = "Mỹ Thuật Bụi | Quản lý Trung tâm Mỹ thuật sáng tạo";
        Size = new Size(1440, 920);
        MinimumSize = new Size(1100, 720);
        StartPosition = FormStartPosition.CenterScreen;
        Font = Theme.Font();
        BackColor = Theme.Paper;
        AutoScaleMode = AutoScaleMode.Dpi;
        menu = new NavigationMenu(user, role);
        clockTimer.Tick += (_, _) => TimeRequested?.Invoke();
        Shown += (_, _) => clockTimer.Start();
        var shell = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 2,
            RowCount = 1,
            Margin = Padding.Empty,
            Padding = Padding.Empty
        };
        shell.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
        shell.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 278));
        shell.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
        var workspace = new Panel
        {
            Dock = DockStyle.Fill,
            Margin = Padding.Empty
        };
        var top = new Panel
        {
            Dock = DockStyle.Top,
            Height = 66,
            BackColor = Color.White,
            Padding = new Padding(26, 0, 22, 0)
        };
        top.Controls.Add(new Label { Text = "Mỹ Thuật Bụi  /  Quản lý trung tâm mỹ thuật", Dock = DockStyle.Fill, ForeColor = Theme.Navy, Font = Theme.Font(10, FontStyle.Bold), TextAlign = ContentAlignment.MiddleLeft, AutoEllipsis = true });
        clock.Dock = DockStyle.Right;
        clock.Width = 195;
        clock.Font = Theme.Font(9);
        clock.ForeColor = Theme.Muted;
        clock.TextAlign = ContentAlignment.MiddleRight;
        top.Controls.Add(clock);
        body.Dock = DockStyle.Fill;
        body.ColumnCount = 1;
        body.RowCount = 4;
        body.Padding = new Padding(26, 20, 20, 8);
        body.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
        body.RowStyles.Add(new RowStyle(SizeType.Absolute, 79));
        body.RowStyles.Add(new RowStyle(SizeType.Absolute, 60));
        body.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
        body.RowStyles.Add(new RowStyle(SizeType.Absolute, 32));
        var title = new Panel
        {
            Dock = DockStyle.Fill
        };
        heading.Dock = DockStyle.Top;
        heading.Height = 41;
        heading.Font = Theme.Font(22, FontStyle.Bold);
        heading.ForeColor = Theme.Navy;
        heading.AutoEllipsis = true;
        subtitle.Dock = DockStyle.Bottom;
        subtitle.Height = 30;
        subtitle.Font = Theme.Font(9.5f);
        subtitle.ForeColor = Theme.Muted;
        subtitle.AutoEllipsis = true;
        title.Controls.Add(heading);
        title.Controls.Add(subtitle);
        actions.Dock = DockStyle.Fill;
        actions.WrapContents = true;
        actions.AutoScroll = true;
        actions.Margin = new Padding(0, 0, 0, 10);
        actions.SizeChanged += (_, _) => ResizeToolbar();
        actions.Paint += PaintToolbarAccent;
        actions.Layout += (_, _) => actions.Invalidate();
        Theme.Grid(grid);
        content.Dock = DockStyle.Fill;
        content.Margin = Padding.Empty;
        content.BackColor = Color.White;
        content.Padding = new Padding(1);
        empty.Text = "Không có dữ liệu phù hợp.";
        empty.Dock = DockStyle.Fill;
        empty.TextAlign = ContentAlignment.MiddleCenter;
        empty.ForeColor = Theme.Muted;
        empty.BackColor = Color.White;
        empty.Visible = false;
        content.Controls.Add(grid);
        content.Controls.Add(empty);
        status.Dock = DockStyle.Fill;
        status.ForeColor = Theme.Muted;
        status.Font = Theme.Font(8.5f);
        status.TextAlign = ContentAlignment.MiddleLeft;
        body.Controls.Add(title, 0, 0);
        body.Controls.Add(actions, 0, 1);
        body.Controls.Add(content, 0, 2);
        body.Controls.Add(status, 0, 3);
        workspace.Controls.Add(body);
        workspace.Controls.Add(top);
        shell.Controls.Add(menu, 0, 0);
        shell.Controls.Add(workspace, 1, 0);
        Controls.Add(shell);
        search.Width = 270;
        search.PlaceholderText = "Tìm theo mã, tên, nội dung…";
        search.Margin = new Padding(0, 10, 16, 8);
        search.TextChanged += (_, _) => Filter();
        FormClosing += (_, e) =>
        {
            if (!Logout && MessageBox.Show(this, "Đóng phần mềm?", "Xác nhận", MessageBoxButtons.YesNo, MessageBoxIcon.Question) != DialogResult.Yes)
                e.Cancel = true;
        };
    }

    public void AddNavigation(string title, Action action) => menu.AddItem(title, action);
    public void AddNavigationGroup(string title, params (string Title, Action Action)[] entries) => menu.AddGroup(title, entries);
    public void SelectNavigation(string title) => menu.SelectTitle(title);
    public void SignOut()
    {
        Logout = true;
        Close();
    }

    public void AddAction(string text, Action click)
    {
        bool primary = text.StartsWith("Thêm") || text.StartsWith("Lập") || text.StartsWith("Ghi nhận") || text.StartsWith("Tiếp nhận");
        var button = Theme.Button(text, primary);
        if (text == "Xóa")
            button.ForeColor = Color.FromArgb(180, 66, 63);
        button.Click += (_, _) => click();
        actions.Controls.Add(button);
        ResizeToolbar();
    }

    void ClearActions()
    {
        actions.Controls.Remove(search);
        foreach (Control control in actions.Controls.Cast<Control>().ToArray())
            control.Dispose();
    }

    void ClearDashboard()
    {
        foreach (var child in content.Controls.Cast<Control>().Where(x => x is DashboardView || x is StatisticsView).ToArray())
            child.Dispose();
    }

    public void RenderList<T>(string title, IEnumerable<T> data, DateTime time)
    {
        dashboard = false;
        now = time;
        ClearDashboard();
        heading.Text = title;
        subtitle.Text = "Tra cứu và cập nhật thông tin.";
        clock.Text = time.ToString("dd/MM/yyyy  •  HH:mm");
        ClearActions();
        rows = data.Cast<object>().ToList();
        rowType = typeof(T);
        search.Text = "";
        actions.Controls.Add(search);
        AddAction("Làm mới", () => RefreshRequested?.Invoke());
        AddAction("Xuất CSV", Export);
        grid.Visible = true;
        Bind(rows);
        ResizeToolbar();
    }

    public void RenderDashboard(DashboardData data, DateTime time)
    {
        dashboard = true;
        now = time;
        ClearDashboard();
        heading.Text = "Tổng quan";
        subtitle.Text = "Lịch học, học phí và công việc cần xử lý.";
        clock.Text = time.ToString("dd/MM/yyyy  •  HH:mm");
        ClearActions();
        AddAction("Làm mới", () => RefreshRequested?.Invoke());
        foreach (var shortcut in data.Shortcuts)
            AddAction(shortcut.Title, () => NavigateRequested?.Invoke(shortcut.Route));
        grid.Visible = false;
        empty.Visible = false;
        var view = new DashboardView(data, target => NavigateRequested?.Invoke(target));
        content.Controls.Add(view);
        view.BringToFront();
        status.Text = "Mỹ Thuật Bụi  •  " + time.ToString("dd/MM/yyyy HH:mm");
        ResizeToolbar();
    }

    public void RenderStatistics(IReadOnlyList<MonthlyFigures> data, DateTime time)
    {
        dashboard = true;
        now = time;
        ClearDashboard();
        ClearActions();
        heading.Text = "Biểu đồ doanh thu";
        subtitle.Text = "Thu học phí, hoàn học phí và bán họa cụ theo tháng.";
        clock.Text = time.ToString("dd/MM/yyyy · HH:mm");
        AddAction("Làm mới", () => RefreshRequested?.Invoke());
        AddAction("Sổ giao dịch", () => NavigateRequested?.Invoke("Thống kê & giao dịch"));
        grid.Visible = false;
        empty.Visible = false;
        var chart = new StatisticsView(data);
        content.Controls.Add(chart);
        chart.BringToFront();
        status.Text = "Rê chuột lên cột để xem số tiền chính xác.";
    }

    void PaintToolbarAccent(object? sender, PaintEventArgs e)
    {
        var controls = actions.Controls.Cast<Control>().Where(x => x.Visible).ToList();
        int y = controls.Count == 0 ? 0 : controls.Max(x => x.Top);
        int end = controls.Where(x => x.Top >= y - 6).Select(x => x.Right).DefaultIfEmpty(0).Max();
        float scale = DeviceDpi / 96f;
        int size = (int)(11 * scale);
        int x = actions.ClientSize.Width - (int)(82 * scale);
        if (x < end + 24 * scale)
            return;
        e.Graphics.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;
        Color[] colors =
        {
            Theme.Teal,
            Theme.Gold,
            Color.FromArgb(195, 133, 111)
        };
        for (int i = 0; i < colors.Length; i++)
        {
            using var fill = new SolidBrush(colors[i]);
            e.Graphics.FillEllipse(fill, x + i * (int)(20 * scale), y + (int)(14 * scale), size, size);
        }
    }

    void ResizeToolbar()
    {
        if (actions.ClientSize.Width <= 0)
            return;
        float scale = DeviceDpi / 96f;
        int used = 0, lineHeight = 0, total = 0;
        foreach (Control item in actions.Controls)
        {
            int width = item.Width + item.Margin.Horizontal;
            int height = item.Height + item.Margin.Vertical;
            if (used > 0 && used + width > actions.ClientSize.Width - SystemInformation.VerticalScrollBarWidth)
            {
                total += lineHeight;
                used = 0;
                lineHeight = 0;
            }

            used += width;
            lineHeight = Math.Max(lineHeight, height);
        }

        float preferred = total + lineHeight + actions.Margin.Vertical + 10 * scale;
        float heightNeeded = Math.Clamp(preferred, 56 * scale, 178 * scale);
        if (body.RowStyles[1].Height != heightNeeded)
            body.RowStyles[1].Height = heightNeeded;
    }

    void Bind(List<object> list)
    {
        grid.DataSource = null;
        if (rowType == null)
            return;
        var typed = (IList)Activator.CreateInstance(typeof(List<>).MakeGenericType(rowType))!;
        foreach (var row in list)
            typed.Add(row);
        grid.DataSource = typed;
        Theme.FormatColumns(grid);
        empty.Visible = list.Count == 0;
        if (empty.Visible)
            empty.BringToFront();
        status.Text = $"{list.Count:N0} / {rows.Count:N0} bản ghi  •  Cập nhật {now:dd/MM/yyyy HH:mm}";
    }

    void Filter()
    {
        if (dashboard)
            return;
        string term = search.Text.Trim();
        Bind(rows.Where(x => x.GetType().GetProperties().Any(p => (Convert.ToString(p.GetValue(x), Theme.Culture) ?? "").Contains(term, StringComparison.CurrentCultureIgnoreCase))).ToList());
    }

    public T Selected<T>() => grid.CurrentRow?.DataBoundItem is T value ? value : throw new Exception("Hãy chọn một dòng trong danh sách.");
    public string SelectedId()
    {
        var value = grid.CurrentRow?.DataBoundItem ?? throw new Exception("Hãy chọn một dòng trong danh sách.");
        return (string)value.GetType().GetProperty("Id")!.GetValue(value)!;
    }

    public void Export()
    {
        using var file = new SaveFileDialog
        {
            Filter = "CSV (*.csv)|*.csv",
            FileName = "BaoCao.csv"
        };
        if (file.ShowDialog(this) != DialogResult.OK)
            return;
        string Escape(object? value) => "\"" + (Convert.ToString(value) ?? "").Replace("\"", "\"\"") + "\"";
        var text = new StringBuilder();
        text.AppendLine(string.Join(",", grid.Columns.Cast<DataGridViewColumn>().Select(x => Escape(x.HeaderText))));
        foreach (DataGridViewRow row in grid.Rows)
            text.AppendLine(string.Join(",", row.Cells.Cast<DataGridViewCell>().Select(x => Escape(x.FormattedValue))));
        try
        {
            File.WriteAllText(file.FileName, text.ToString(), new UTF8Encoding(true));
            MessageBox.Show(this, "Đã xuất danh sách đang hiển thị.", "Xuất báo cáo");
        }
        catch (Exception ex)
        {
            MessageBox.Show(this, ex.Message, "Không thể xuất báo cáo", MessageBoxButtons.OK, MessageBoxIcon.Warning);
        }
    }

    public void UpdateTime(DateTime time)
    {
        clock.Text = time.ToString("dd/MM/yyyy · HH:mm");
    }

    protected override void Dispose(bool disposing)
    {
        if (disposing)
            clockTimer.Dispose();
        base.Dispose(disposing);
    }
}
