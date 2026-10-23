using System.Globalization;
using System.Reflection;

namespace LhlArt.Views.Shared;
internal static class Theme
{
    public static readonly Color Navy = Color.FromArgb(23, 79, 69);
    public static readonly Color Teal = Color.FromArgb(31, 107, 91);
    public static readonly Color Paper = Color.FromArgb(252, 250, 245);
    public static readonly Color Ink = Color.FromArgb(33, 51, 47);
    public static readonly Color Muted = Color.FromArgb(82, 100, 91);
    public static readonly Color Line = Color.FromArgb(232, 229, 220);
    public static readonly Color Coral = Color.FromArgb(174, 67, 28);
    public static readonly Color Sage = Color.FromArgb(239, 243, 234);
    public static readonly Color Gold = Color.FromArgb(241, 189, 75);
    public static readonly CultureInfo Culture = CultureInfo.GetCultureInfo("vi-VN");
    static readonly string FontFamilyName = System.Drawing.FontFamily.Families.Any(f => f.Name == "Be Vietnam Pro") ? "Be Vietnam Pro" : "Segoe UI";
    public static Font Font(float size = 10, FontStyle style = FontStyle.Regular) => new(FontFamilyName, size, style);
    public static string Money(decimal amount) => amount.ToString("N0", Culture) + " ₫";
    public static bool IsMoney(string name) => new[]
    {
        "HocPhi", "HocPhiApDung", "GiaBanThamKhao", "TranGiam", "SoTien",
        "Fee",
        "Cap",
        "UnitPrice",
        "Subtotal",
        "Discount",
        "Total",
        "OriginalFee",
        "Due",
        "Credit",
        "Amount",
        "Refund",
        "Receipts",
        "Refunds",
        "amount",
        "money",
        "info"
    }.Contains(name);
    public static Button Button(string text, bool primary = false)
    {
        var button = new ActionButton
        {
            Text = text,
            AutoSize = true,
            MinimumSize = new Size(100, 40),
            Height = 40,
            Padding = new Padding(14, 5, 14, 5),
            Margin = new Padding(0, 4, 10, 6),
            FlatStyle = FlatStyle.Flat,
            BackColor = primary ? Teal : Color.White,
            ForeColor = primary ? Color.White : Ink,
            Font = Font(10),
            Cursor = Cursors.Hand,
            UseVisualStyleBackColor = false
        };
        button.FlatAppearance.BorderColor = primary ? Teal : Line;
        button.FlatAppearance.MouseOverBackColor = primary ? Navy : Sage;
        return button;
    }

    public static PictureBox Logo() => new()
    {
        Image = LogoImage.Value,
        SizeMode = PictureBoxSizeMode.Zoom,
        BackColor = Color.FromArgb(250, 251, 239)
    };
    static readonly Lazy<Image> LogoImage = new(() =>
    {
        using var stream = Assembly.GetExecutingAssembly().GetManifestResourceStream("LhlArt.Assets.lhl-art.jpg")!;
        using var original = Image.FromStream(stream);
        return new Bitmap(original);
    });
    public static void Grid(DataGridView grid)
    {
        grid.Dock = DockStyle.Fill;
        grid.ReadOnly = true;
        grid.AllowUserToAddRows = false;
        grid.AllowUserToDeleteRows = false;
        grid.AllowUserToOrderColumns = true;
        grid.AutoGenerateColumns = true;
        grid.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
        grid.MultiSelect = false;
        grid.BackgroundColor = Color.White;
        grid.BorderStyle = BorderStyle.None;
        grid.RowHeadersVisible = false;
        grid.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.None;
        grid.ColumnHeadersHeight = 48;
        grid.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.DisableResizing;
        grid.ColumnHeadersDefaultCellStyle = new DataGridViewCellStyle
        {
            BackColor = Color.FromArgb(239, 243, 237),
            ForeColor = Navy,
            Font = Font(9.5f, FontStyle.Bold),
            Padding = new Padding(10, 0, 10, 0),
            WrapMode = DataGridViewTriState.False
        };
        grid.EnableHeadersVisualStyles = false;
        grid.RowTemplate.Height = 44;
        grid.GridColor = Line;
        grid.CellBorderStyle = DataGridViewCellBorderStyle.SingleHorizontal;
        grid.ColumnHeadersBorderStyle = DataGridViewHeaderBorderStyle.None;
        grid.DefaultCellStyle = new DataGridViewCellStyle
        {
            Font = Font(10),
            ForeColor = Ink,
            BackColor = Color.White,
            SelectionBackColor = Color.FromArgb(230, 239, 227),
            SelectionForeColor = Navy,
            Padding = new Padding(10, 0, 10, 0),
            FormatProvider = Culture
        };
        grid.AlternatingRowsDefaultCellStyle.BackColor = Color.FromArgb(252, 250, 245);
        grid.CellFormatting += (_, e) =>
        {
            if (e.RowIndex < 0)
                return;
            if (e.Value is DateTime date && date.Year < 1900)
            {
                e.Value = "—";
                e.FormattingApplied = true;
            }

            if (grid.Columns[e.ColumnIndex].DataPropertyName is "Status" or "Warning" && e.Value is string state)
            {
                e.CellStyle!.ForeColor = state.Contains("Chờ") || state.Contains("Cần") || state == "Dự kiến" ? Color.FromArgb(155, 100, 22) : state.Contains("Hủy") || state.Contains("Hết hạn") || state.Contains("Chưa") ? Color.FromArgb(167, 64, 61) : Teal;
            }
        };
    }

    public static void FormatColumns(DataGridView grid)
    {
        foreach (DataGridViewColumn column in grid.Columns)
        {
            var type = Nullable.GetUnderlyingType(column.ValueType ?? typeof(string)) ?? column.ValueType;
            string key = column.DataPropertyName;
            bool numeric = type == typeof(int) || type == typeof(decimal) || type == typeof(double) || type == typeof(long);
            if (numeric)
            {
                column.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleRight;
                column.HeaderCell.Style.Alignment = DataGridViewContentAlignment.MiddleRight;
                column.DefaultCellStyle.Format = IsMoney(key) ? "#,##0' ₫';-#,##0' ₫';0' ₫'" : key == "Percent" ? "0.##' %'" : type == typeof(int) || type == typeof(long) ? "N0" : "0.##";
            }

            if (type == typeof(DateTime))
                column.DefaultCellStyle.Format = key is "Birth" or "MemberSince" ? "dd/MM/yyyy" : "dd/MM/yyyy HH:mm";
            column.MinimumWidth = 95;
            int headerWidth = TextRenderer.MeasureText(column.HeaderText, grid.ColumnHeadersDefaultCellStyle.Font).Width + 38;
            column.Width = Math.Max(headerWidth, type == typeof(DateTime) ? 175 : numeric ? 145 : key is "Name" or "Note" or "Reason" or "Action" ? 250 : key is "Status" or "Warning" ? 170 : 140);
        }
    }
}
