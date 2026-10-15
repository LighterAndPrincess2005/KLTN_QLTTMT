using LhlArt.Views.Shared;

namespace LhlArt.Views;
public sealed class ReceiptForm : Form
{
    public ReceiptForm(string text)
    {
        Text = "Phiếu đăng ký | LHL ART";
        Size = new Size(760, 780);
        MinimumSize = new Size(650, 550);
        StartPosition = FormStartPosition.CenterParent;
        Font = Theme.Font();
        BackColor = Color.White;
        AutoScaleMode = AutoScaleMode.Dpi;
        var header = new Panel
        {
            Dock = DockStyle.Top,
            Height = 110,
            Padding = new Padding(22, 12, 22, 12),
            BackColor = Theme.Paper
        };
        var logo = Theme.Logo();
        logo.Width = 88;
        logo.Dock = DockStyle.Left;
        header.Controls.Add(new Label { Text = "LHL ART\nTRUNG TÂM MỸ THUẬT SÁNG TẠO", Dock = DockStyle.Fill, Padding = new Padding(20, 12, 0, 0), ForeColor = Theme.Navy, Font = Theme.Font(13, FontStyle.Bold) });
        header.Controls.Add(logo);
        var box = new TextBox
        {
            Multiline = true,
            ReadOnly = true,
            Dock = DockStyle.Fill,
            BorderStyle = BorderStyle.None,
            BackColor = Color.White,
            ScrollBars = ScrollBars.Vertical,
            Text = text.Replace("\n", Environment.NewLine),
            Font = Theme.Font(11)
        };
        var body = new Panel
        {
            Dock = DockStyle.Fill,
            Padding = new Padding(28)
        };
        body.Controls.Add(box);
        var footer = new FlowLayoutPanel
        {
            Dock = DockStyle.Bottom,
            Height = 68,
            FlowDirection = FlowDirection.RightToLeft,
            Padding = new Padding(16, 6, 16, 6),
            BackColor = Theme.Paper
        };
        var button = Theme.Button("In phiếu", true);
        footer.Controls.Add(button);
        Controls.Add(body);
        Controls.Add(footer);
        Controls.Add(header);
        button.Click += (_, _) =>
        {
            try
            {
                using var document = new System.Drawing.Printing.PrintDocument();
                document.PrintPage += (_, e) =>
                {
                    var bounds = e.MarginBounds;
                    using var titleFont = Theme.Font(14, FontStyle.Bold);
                    e.Graphics!.DrawImage(logo.Image!, bounds.Left, bounds.Top, 84, 84);
                    e.Graphics.DrawString("LHL ART\nTrung tâm Mỹ thuật sáng tạo", titleFont, Brushes.Black, bounds.Left + 104, bounds.Top + 15);
                    e.Graphics.DrawString(text, box.Font, Brushes.Black, new RectangleF(bounds.Left, bounds.Top + 110, bounds.Width, bounds.Height - 110));
                };
                using var dialog = new PrintDialog
                {
                    Document = document
                };
                if (dialog.ShowDialog(this) == DialogResult.OK)
                    document.Print();
            }
            catch (Exception ex)
            {
                MessageBox.Show(this, ex.Message, "Không thể in phiếu", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        };
    }
}
