using System.Drawing.Drawing2D;
using LhlArt.Models;
using LhlArt.Views.Shared;

namespace LhlArt.Views;
public sealed class DashboardView : UserControl
{
    readonly TableLayoutPanel layout = new();
    readonly TableLayoutPanel metrics = new();
    readonly List<Control> tiles = new();
    int lastColumns;
    public DashboardView(DashboardData data, Action<string> navigate)
    {
        Dock = DockStyle.Fill;
        BackColor = Theme.Paper;
        AutoScroll = true;
        layout.Dock = DockStyle.Top;
        layout.AutoSize = true;
        layout.ColumnCount = 1;
        layout.RowCount = 4;
        layout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
        layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 110));
        layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 260));
        layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 286));
        layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 166));
        layout.Controls.Add(new WelcomeCard(data.Greeting, data.Period) { Dock = DockStyle.Fill, Margin = new Padding(0, 0, 0, 16) }, 0, 0);
        metrics.Dock = DockStyle.Fill;
        metrics.Margin = Padding.Empty;
        int index = 0;
        foreach (var metric in data.Metrics)
        {
            var color = new[]
            {
                Theme.Teal,
                Color.FromArgb(74, 103, 157),
                Color.FromArgb(179, 125, 59),
                Color.FromArgb(156, 107, 133)
            }[index++ % 4];
            tiles.Add(new MetricCard(metric, color) { Dock = DockStyle.Fill, Margin = new Padding(0, 0, 14, 14) });
        }

        layout.Controls.Add(metrics, 0, 1);
        var lower = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 2,
            RowCount = 1,
            Margin = Padding.Empty
        };
        lower.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 57));
        lower.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 43));
        lower.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
        lower.Controls.Add(Section(data.HighlightTitle, data.Highlights, data.HighlightRoute, navigate), 0, 0);
        lower.Controls.Add(Section("Việc cần xử lý", data.Notices, "", navigate), 1, 0);
        layout.Controls.Add(lower, 0, 2);
        layout.Controls.Add(new ProgressCard(data.ProgressTitle, data.Progress) { Dock = DockStyle.Fill, Margin = new Padding(0, 14, 14, 0) }, 0, 3);
        Controls.Add(layout);
        SizeChanged += (_, _) => Reflow();
        DpiChangedAfterParent += (_, _) =>
        {
            lastColumns = 0;
            Reflow();
        };
        Reflow();
    }

    Control Section(string title, IReadOnlyList<DashboardLine> rows, string target, Action<string> navigate)
    {
        var panel = new SurfacePanel
        {
            Dock = DockStyle.Fill,
            Margin = new Padding(0, 0, 14, 0),
            Padding = new Padding(18, 14, 18, 14)
        };
        var table = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 1,
            RowCount = 2,
            BackColor = Color.White,
            Margin = Padding.Empty
        };
        table.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
        table.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
        table.RowStyles.Add(new RowStyle(SizeType.Absolute, target == "" ? 0 : 41));
        table.Controls.Add(new DetailList(title, rows) { Dock = DockStyle.Fill, Margin = Padding.Empty }, 0, 0);
        if (target != "")
        {
            var button = Theme.Button(target == "Lịch học & điểm danh" ? "Xem lịch học" : "Xem chi tiết");
            button.Dock = DockStyle.Fill;
            button.Margin = new Padding(0, 3, 0, 0);
            button.MinimumSize = new Size(0, 34);
            button.Click += (_, _) => navigate(target);
            table.Controls.Add(button, 0, 1);
        }

        panel.Controls.Add(table);
        return panel;
    }

    void Reflow()
    {
        float scale = DeviceDpi / 96f;
        int columns = ClientSize.Width / scale >= 970 ? 4 : ClientSize.Width / scale >= 710 ? 3 : 2;
        if (columns == lastColumns)
            return;
        lastColumns = columns;
        metrics.SuspendLayout();
        metrics.Controls.Clear();
        metrics.ColumnStyles.Clear();
        metrics.RowStyles.Clear();
        metrics.ColumnCount = 12;
        metrics.RowCount = Math.Max(1, (tiles.Count + columns - 1) / columns);
        for (int c = 0; c < 12; c++)
            metrics.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100f / 12));
        int index = 0;
        for (int row = 0; row < metrics.RowCount; row++)
        {
            metrics.RowStyles.Add(new RowStyle(SizeType.Absolute, 126 * scale));
            int count = tiles.Count / metrics.RowCount + (row < tiles.Count % metrics.RowCount ? 1 : 0);
            int span = count == 0 ? 12 : 12 / count;
            for (int column = 0; column < count; column++)
            {
                var tile = tiles[index++];
                metrics.Controls.Add(tile, column * span, row);
                metrics.SetColumnSpan(tile, span);
            }
        }

        layout.RowStyles[0].Height = 110 * scale;
        layout.RowStyles[1].Height = metrics.RowCount * 126 * scale;
        layout.RowStyles[2].Height = 286 * scale;
        layout.RowStyles[3].Height = 166 * scale;
        metrics.ResumeLayout();
    }

    sealed class SurfacePanel : Panel
    {
        public SurfacePanel()
        {
            DoubleBuffered = true;
            BackColor = Color.White;
        }

        protected override void OnPaintBackground(PaintEventArgs e)
        {
            e.Graphics.Clear(Theme.Paper);
            e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
            using var path = IconPainter.Rounded(new RectangleF(0.5f, 0.5f, Width - 1, Height - 1), 12 * DeviceDpi / 96f);
            using var brush = new SolidBrush(Color.White);
            using var pen = new Pen(Theme.Line);
            e.Graphics.FillPath(brush, path);
            e.Graphics.DrawPath(pen, path);
        }
    }

    abstract class PaintedCard : Control
    {
        protected PaintedCard()
        {
            DoubleBuffered = true;
            BackColor = Theme.Paper;
            SetStyle(ControlStyles.ResizeRedraw, true);
            AccessibleRole = AccessibleRole.StaticText;
        }

        protected int S(float value) => (int)Math.Round(value * DeviceDpi / 96f);
        protected void TextLine(Graphics graphics, string text, Font font, Color color, Rectangle rectangle, bool right = false)
        {
            TextRenderer.DrawText(graphics, text, font, rectangle, color, TextFormatFlags.NoPrefix | TextFormatFlags.SingleLine | TextFormatFlags.VerticalCenter | TextFormatFlags.EndEllipsis | TextFormatFlags.NoPadding | (right ? TextFormatFlags.Right : TextFormatFlags.Left));
        }

        protected void Background(Graphics graphics, Color color)
        {
            graphics.Clear(Theme.Paper);
            graphics.SmoothingMode = SmoothingMode.AntiAlias;
            using var shape = IconPainter.Rounded(new RectangleF(0.5f, 0.5f, Width - 1, Height - 1), S(12));
            using var fill = new SolidBrush(color);
            graphics.FillPath(fill, shape);
            using var border = new Pen(color == Color.White ? Theme.Line : color);
            graphics.DrawPath(border, shape);
        }
    }

    sealed class MetricCard : PaintedCard
    {
        readonly Metric item;
        readonly Color accent;
        public MetricCard(Metric item, Color accent)
        {
            this.item = item;
            this.accent = accent;
            AccessibleName = $"{item.Name}: {item.Value}. {item.Detail}";
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            Background(e.Graphics, Color.White);
            using var title = Theme.Font(10);
            using var value = Theme.Font(item.Value.Length > 10 ? 18 : 24, FontStyle.Bold);
            using var note = Theme.Font(8.5f);
            TextLine(e.Graphics, item.Name, title, Theme.Muted, new Rectangle(S(18), S(12), Width - S(64), S(25)));
            using var tint = new SolidBrush(Color.FromArgb(238, 244, 241));
            e.Graphics.FillEllipse(tint, Width - S(44), S(12), S(30), S(30));
            IconPainter.Draw(e.Graphics, item.Name, new RectangleF(Width - S(38), S(18), S(18), S(18)), accent);
            TextLine(e.Graphics, item.Value, value, Theme.Ink, new Rectangle(S(18), S(40), Width - S(32), S(38)));
            TextLine(e.Graphics, item.Detail, note, Theme.Muted, new Rectangle(S(18), S(79), Width - S(32), S(22)));
        }
    }

    sealed class DetailList : PaintedCard
    {
        readonly string title;
        readonly IReadOnlyList<DashboardLine> rows;
        public DetailList(string title, IReadOnlyList<DashboardLine> rows)
        {
            this.title = title;
            this.rows = rows;
            BackColor = Color.White;
            AccessibleName = title + ". " + string.Join(". ", rows.Select(x => $"{x.Title}, {x.Detail}, {x.State}"));
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            e.Graphics.Clear(Color.White);
            using var heading = Theme.Font(12, FontStyle.Bold);
            using var main = Theme.Font(10, FontStyle.Bold);
            using var detail = Theme.Font(9);
            using var number = Theme.Font(10, FontStyle.Bold);
            TextLine(e.Graphics, title, heading, Theme.Navy, new Rectangle(0, 0, Width, S(29)));
            if (rows.Count == 0)
            {
                using var tint = new SolidBrush(Color.FromArgb(240, 244, 237));
                e.Graphics.FillEllipse(tint, Width / 2f - S(27), S(51), S(54), S(54));
                IconPainter.Draw(e.Graphics, title.Contains("Lịch") ? "Lịch học" : "Báo cáo", new RectangleF(Width / 2f - S(13), S(65), S(26), S(26)), Theme.Teal);
                TextLine(e.Graphics, "Chưa có dữ liệu để hiển thị", detail, Theme.Muted, new Rectangle(0, S(118), Width, S(25)));
                return;
            }

            for (int i = 0; i < Math.Min(3, rows.Count); i++)
            {
                int y = S(39 + i * 57);
                var row = rows[i];
                int trailing = string.IsNullOrWhiteSpace(row.State) ? 0 : Math.Min(Width / 2, TextRenderer.MeasureText(row.State, number).Width + S(8));
                TextLine(e.Graphics, row.Title, main, Theme.Ink, new Rectangle(0, y, Width - trailing - S(7), S(24)));
                TextLine(e.Graphics, row.State, number, Theme.Teal, new Rectangle(Width - trailing, y, trailing, S(24)), true);
                TextLine(e.Graphics, row.Detail, detail, Theme.Muted, new Rectangle(0, y + S(24), Width, S(21)));
                if (i + 1 < Math.Min(3, rows.Count))
                {
                    using var line = new Pen(Theme.Line);
                    e.Graphics.DrawLine(line, 0, y + S(52), Width, y + S(52));
                }
            }
        }
    }

    sealed class ProgressCard : PaintedCard
    {
        readonly string title;
        readonly IReadOnlyList<DashboardProgress> rows;
        public ProgressCard(string title, IReadOnlyList<DashboardProgress> rows)
        {
            this.title = title;
            this.rows = rows;
            AccessibleName = title + ". " + string.Join(". ", rows.Select(x => $"{x.Title}: {x.Value}/{x.Total}"));
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            Background(e.Graphics, Color.White);
            using var heading = Theme.Font(11, FontStyle.Bold);
            using var text = Theme.Font(9.5f);
            TextLine(e.Graphics, title, heading, Theme.Navy, new Rectangle(S(18), S(11), Width - S(36), S(28)));
            if (rows.Count == 0)
            {
                IconPainter.Draw(e.Graphics, "Đào tạo", new RectangleF(S(20), S(60), S(44), S(44)), Theme.Teal);
                TextLine(e.Graphics, "Thông tin sẽ được tổng hợp khi có dữ liệu.", text, Theme.Muted, new Rectangle(S(82), S(60), Width - S(100), S(32)));
                return;
            }

            int count = Math.Min(3, rows.Count);
            int col = (Width - S(48)) / count;
            for (int i = 0; i < count; i++)
            {
                var row = rows[i];
                int x = S(18) + i * col;
                int width = col - S(20);
                TextLine(e.Graphics, row.Title, text, Theme.Ink, new Rectangle(x, S(48), width, S(23)));
                TextLine(e.Graphics, $"{row.Value:N0} / {row.Total:N0}", text, Theme.Muted, new Rectangle(x, S(76), width, S(21)));
                using var track = new SolidBrush(Color.FromArgb(233, 238, 231));
                e.Graphics.FillRectangle(track, x, S(109), width, S(7));
                float ratio = row.Total <= 0 ? 0 : Math.Clamp((float)row.Value / row.Total, 0, 1);
                using var fill = new SolidBrush(i % 2 == 0 ? Theme.Teal : Theme.Gold);
                e.Graphics.FillRectangle(fill, x, S(109), width * ratio, S(7));
            }
        }
    }

    sealed class WelcomeCard : PaintedCard
    {
        readonly string title;
        readonly string period;
        public WelcomeCard(string title, string period)
        {
            this.title = title;
            this.period = period;
            AccessibleName = title + ". " + period;
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            Background(e.Graphics, Color.FromArgb(233, 239, 231));
            using var font = Theme.Font(19, FontStyle.Bold);
            using var small = Theme.Font(10);
            TextLine(e.Graphics, title, font, Theme.Navy, new Rectangle(S(22), S(16), Width - S(205), S(33)));
            TextLine(e.Graphics, period, small, Theme.Muted, new Rectangle(S(23), S(51), Width - S(205), S(24)));
            var state = e.Graphics.Save();
            e.Graphics.TranslateTransform(Width - S(158), S(7));
            e.Graphics.ScaleTransform(DeviceDpi / 96f, DeviceDpi / 96f);
            using var teal = new SolidBrush(Theme.Teal);
            using var gold = new SolidBrush(Color.FromArgb(217, 174, 91));
            using var clay = new SolidBrush(Color.FromArgb(193, 129, 108));
            e.Graphics.FillPie(teal, 12, 5, 80, 80, 180, 180);
            e.Graphics.FillRectangle(teal, 12, 45, 80, 22);
            e.Graphics.FillEllipse(gold, 64, 37, 44, 44);
            e.Graphics.FillPolygon(clay, new[] { new Point(104, 4), new Point(139, 63), new Point(101, 63) });
            using var stroke = new Pen(Color.FromArgb(111, 145, 135), 2);
            e.Graphics.DrawArc(stroke, 1, 8, 72, 72, 90, 220);
            e.Graphics.Restore(state);
        }
    }
}
