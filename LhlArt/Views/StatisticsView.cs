using System.Drawing.Drawing2D;
using LhlArt.Models;
using LhlArt.Views.Shared;

namespace LhlArt.Views;
public sealed class StatisticsView : Control
{
    readonly IReadOnlyList<MonthlyFigures> data;
    readonly bool combinedReceipts;
    readonly ToolTip tip = new();
    readonly List<(RectangleF Bounds, string Text)> bars = new();
    string hover = "";
    public StatisticsView(IReadOnlyList<MonthlyFigures> data, bool combinedReceipts = false)
    {
        this.data = data;
        this.combinedReceipts = combinedReceipts;
        Dock = DockStyle.Fill;
        BackColor = Color.White;
        DoubleBuffered = true;
        SetStyle(ControlStyles.ResizeRedraw, true);
        AccessibleName = combinedReceipts ? "Tổng thu và hoàn tiền đã phân bổ trong sáu tháng" : "Thu và hoàn học phí, doanh thu họa cụ trong sáu tháng gần nhất";
        MouseMove += (_, e) =>
        {
            string value = bars.FirstOrDefault(x => x.Bounds.Contains(e.Location)).Text ?? "";
            if (value != hover)
            {
                hover = value;
                tip.SetToolTip(this, value);
            }
        };
    }

    protected override void OnPaint(PaintEventArgs e)
    {
        base.OnPaint(e);
        e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
        bars.Clear();
        float scale = DeviceDpi / 96f;
        int S(float value) => (int)(value * scale);
        using var heading = Theme.Font(15, FontStyle.Bold);
        using var label = Theme.Font(9);
        using var number = Theme.Font(11, FontStyle.Bold);
        TextRenderer.DrawText(e.Graphics, combinedReceipts ? "Thu và hoàn tiền đã phân bổ" : "Thu chi và doanh thu họa cụ", heading, new Rectangle(S(24), S(18), Width - S(48), S(32)), Theme.Navy, TextFormatFlags.Left | TextFormatFlags.EndEllipsis);
        string[] names =
        {
            "Thu học phí",
            "Hoàn học phí",
            "Bán họa cụ"
        };
        if (combinedReceipts) names = ["Tổng thu đã phân bổ", "Tổng hoàn đã phân bổ"];
        Color[] colors =
        {
            Theme.Teal,
            Color.FromArgb(194, 127, 105),
            Theme.Gold
        };
        decimal[] totals =
        {
            data.Sum(x => x.Tuition),
            data.Sum(x => x.Refunds),
            data.Sum(x => x.Materials)
        };
        for (int i = 0; i < names.Length; i++)
        {
            int x = S(24) + i * (Width - S(48)) / names.Length;
            using var fill = new SolidBrush(colors[i]);
            e.Graphics.FillEllipse(fill, x, S(65), S(9), S(9));
            TextRenderer.DrawText(e.Graphics, names[i], label, new Point(x + S(16), S(60)), Theme.Muted);
            TextRenderer.DrawText(e.Graphics, Theme.Money(totals[i]), number, new Point(x + S(16), S(84)), Theme.Ink);
        }

        var plot = new RectangleF(S(75), S(153), Math.Max(S(80), Width - S(103)), Math.Max(S(60), Height - S(225)));
        decimal maximum = Math.Max(1000000m, data.SelectMany(x => new[] { x.Tuition, x.Refunds, x.Materials }).DefaultIfEmpty().Max());
        maximum = Math.Ceiling(maximum / 1000000m) * 1000000m;
        for (int i = 0; i <= 4; i++)
        {
            float y = plot.Bottom - i * plot.Height / 4;
            using var line = new Pen(Theme.Line);
            e.Graphics.DrawLine(line, plot.Left, y, plot.Right, y);
            string value = (maximum * i / 4 / 1000000m).ToString("0.##", Theme.Culture);
            TextRenderer.DrawText(e.Graphics, value, label, new Rectangle(S(8), (int)y - S(10), S(56), S(22)), Theme.Muted, TextFormatFlags.Right | TextFormatFlags.VerticalCenter);
        }

        TextRenderer.DrawText(e.Graphics, "Triệu đồng", label, new Point(S(16), S(125)), Theme.Muted);
        float group = plot.Width / Math.Max(1, data.Count), barWidth = group * .18f;
        for (int i = 0; i < data.Count; i++)
        {
            decimal[] values =
            {
                data[i].Tuition,
                data[i].Refunds,
                data[i].Materials
            };
            for (int j = 0; j < names.Length; j++)
            {
                float h = (float)(values[j] / maximum) * plot.Height;
                var bar = new RectangleF(plot.Left + i * group + group * .19f + j * barWidth, plot.Bottom - h, barWidth * .8f, h);
                using var fill = new SolidBrush(colors[j]);
                e.Graphics.FillRectangle(fill, bar);
                bars.Add((new RectangleF(bar.X, bar.Y - S(3), bar.Width, Math.Max(S(6), h)), $"{data[i].Month:MM/yyyy} · {names[j]}: {Theme.Money(values[j])}"));
            }

            TextRenderer.DrawText(e.Graphics, data[i].Month.ToString("MM/yyyy"), label, new Rectangle((int)(plot.Left + i * group), (int)plot.Bottom + S(9), (int)group, S(25)), Theme.Ink, TextFormatFlags.HorizontalCenter);
        }

        if (totals.All(x => x == 0))
            TextRenderer.DrawText(e.Graphics, "Chưa có giao dịch trong kỳ này.", label, Rectangle.Round(plot), Theme.Muted, TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter);
        TextRenderer.DrawText(e.Graphics, "Số liệu 6 tháng gần nhất · Giá trị chuyển lớp không tính vào tiền thu mới.", label, new Rectangle(S(24), Height - S(31), Width - S(48), S(25)), Theme.Muted, TextFormatFlags.EndEllipsis);
    }

    protected override void Dispose(bool disposing)
    {
        if (disposing)
            tip.Dispose();
        base.Dispose(disposing);
    }
}
