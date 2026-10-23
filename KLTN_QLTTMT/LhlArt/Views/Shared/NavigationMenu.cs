using System.Drawing.Drawing2D;

namespace LhlArt.Views.Shared;
public sealed class NavigationMenu : Panel
{
    readonly FlowLayoutPanel list = new();
    readonly Dictionary<FlowLayoutPanel, NavButton> groups = new();
    readonly List<NavButton> items = new();
    readonly ToolTip tips = new();
    public NavigationMenu(string user, string role)
    {
        Dock = DockStyle.Fill;
        BackColor = Theme.Navy;
        Padding = new Padding(14, 18, 14, 12);
        var brand = new Panel
        {
            Dock = DockStyle.Top,
            Height = 118
        };
        var logo = Theme.Logo();
        logo.Location = new Point(10, 4);
        logo.Size = new Size(68, 68);
        brand.Controls.Add(logo);
        brand.Controls.Add(new Label { Text = "Mỹ Thuật Bụi", Location = new Point(90, 9), Size = new Size(142, 34), Font = Theme.Font(12, FontStyle.Bold), ForeColor = Color.White });
        brand.Controls.Add(new Label { Text = "Học vẽ cùng Bụi", Location = new Point(92, 45), Size = new Size(150, 23), Font = Theme.Font(9), ForeColor = Color.FromArgb(180, 200, 206) });
        brand.Controls.Add(new Label { Text = "Quản lý trung tâm", Location = new Point(11, 90), Size = new Size(220, 22), Font = Theme.Font(9), ForeColor = Color.FromArgb(152, 177, 185) });
        var account = new TableLayoutPanel
        {
            Dock = DockStyle.Bottom,
            Height = 64,
            ColumnCount = 1,
            RowCount = 2,
            Padding = new Padding(12, 10, 8, 6)
        };
        account.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
        account.RowStyles.Add(new RowStyle(SizeType.Percent, 55));
        account.RowStyles.Add(new RowStyle(SizeType.Percent, 45));
        account.Controls.Add(new Label { UseMnemonic = false, Text = user, Dock = DockStyle.Fill, ForeColor = Color.White, Font = Theme.Font(10, FontStyle.Bold), AutoEllipsis = true, Margin = Padding.Empty }, 0, 0);
        account.Controls.Add(new Label { UseMnemonic = false, Text = role, Dock = DockStyle.Fill, ForeColor = Color.FromArgb(169, 193, 197), Font = Theme.Font(9), Margin = Padding.Empty }, 0, 1);
        list.Dock = DockStyle.Fill;
        list.FlowDirection = FlowDirection.TopDown;
        list.WrapContents = false;
        list.AutoScroll = true;
        list.Padding = new Padding(0, 6, 0, 4);
        Controls.Add(list);
        Controls.Add(account);
        Controls.Add(brand);
        list.SizeChanged += (_, _) => ResizeItems();
    }

    NavButton MakeButton(string text, bool child)
    {
        var button = new NavButton(text, child);
        tips.SetToolTip(button, text);
        return button;
    }

    public void AddItem(string title, Action action)
    {
        var button = MakeButton(title, false);
        items.Add(button);
        list.Controls.Add(button);
        button.Click += (_, _) =>
        {
            SelectItem(button);
            action();
        };
        ResizeItems();
    }

    public void AddGroup(string title, params (string Title, Action Action)[] entries)
    {
        var toggle = MakeButton(title, false);
        toggle.IsGroup = true;
        var children = new FlowLayoutPanel
        {
            FlowDirection = FlowDirection.TopDown,
            WrapContents = false,
            AutoSize = true,
            AutoSizeMode = AutoSizeMode.GrowAndShrink,
            Width = 235,
            Margin = Padding.Empty,
            Padding = new Padding(0, 0, 0, 4),
            Visible = false
        };
        groups[children] = toggle;
        list.Controls.Add(toggle);
        list.Controls.Add(children);
        foreach (var entry in entries)
        {
            var item = MakeButton(entry.Title, true);
            items.Add(item);
            children.Controls.Add(item);
            item.Click += (_, _) =>
            {
                SelectItem(item);
                entry.Action();
            };
        }

        toggle.Click += (_, _) =>
        {
            bool open = !toggle.Expanded;
            list.SuspendLayout();
            foreach (var pair in groups)
            {
                pair.Key.Visible = false;
                pair.Value.Expanded = false;
                pair.Value.Invalidate();
            }

            children.Visible = open;
            toggle.Expanded = open;
            toggle.Invalidate();
            list.ResumeLayout();
            ResizeItems();
        };
        ResizeItems();
    }

    public void SelectTitle(string title)
    {
        var item = items.FirstOrDefault(x => x.Text == title);
        if (item == null)
            return;
        SelectItem(item);
        foreach (var pair in groups)
        {
            bool selected = pair.Key.Controls.Contains(item);
            pair.Key.Visible = selected;
            pair.Value.Expanded = selected;
            pair.Value.Invalidate();
        }
    }

    void SelectItem(NavButton selected)
    {
        foreach (var item in items)
        {
            item.Selected = item == selected;
            item.Invalidate();
        }

        foreach (var pair in groups)
        {
            pair.Value.Selected = pair.Key.Controls.Contains(selected);
            pair.Value.Invalidate();
        }
    }

    void ResizeItems()
    {
        int width = Math.Max(170, list.ClientSize.Width - SystemInformation.VerticalScrollBarWidth - 2);
        foreach (Control control in list.Controls)
            control.Width = width;
        foreach (var group in groups.Keys)
            foreach (Control control in group.Controls)
                control.Width = width;
    }

    protected override void Dispose(bool disposing)
    {
        if (disposing)
            tips.Dispose();
        base.Dispose(disposing);
    }

    sealed class NavButton : Button
    {
        readonly bool child;
        bool hovered;
        public bool Selected { get; set; }
        public bool Expanded { get; set; }
        public bool IsGroup { get; set; }

        public NavButton(string text, bool child)
        {
            this.child = child;
            UseMnemonic = false;
            Text = text;
            AccessibleName = text;
            Width = 235;
            Height = child ? 37 : 44;
            Font = Theme.Font(child ? 9.5f : 10, child ? FontStyle.Regular : FontStyle.Bold);
            Margin = new Padding(0, 2, 0, 2);
            FlatStyle = FlatStyle.Flat;
            FlatAppearance.BorderSize = 0;
            BackColor = Theme.Navy;
            ForeColor = Color.FromArgb(224, 234, 234);
            Cursor = Cursors.Hand;
            SetStyle(ControlStyles.UserPaint | ControlStyles.AllPaintingInWmPaint | ControlStyles.OptimizedDoubleBuffer | ControlStyles.ResizeRedraw, true);
        }

        protected override void OnMouseEnter(EventArgs e)
        {
            hovered = true;
            Invalidate();
            base.OnMouseEnter(e);
        }

        protected override void OnMouseLeave(EventArgs e)
        {
            hovered = false;
            Invalidate();
            base.OnMouseLeave(e);
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            float scale = DeviceDpi / 96f;
            int S(float value) => (int)Math.Round(value * scale);
            e.Graphics.Clear(BackColor);
            e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
            var color = Selected ? Color.White : ForeColor;
            if (Selected || hovered)
            {
                using var shape = IconPainter.Rounded(new RectangleF(1, 1, Width - 2, Height - 2), S(9));
                using var fill = new SolidBrush(Selected && !IsGroup ? Theme.Teal : Color.FromArgb(43, 67, 76));
                e.Graphics.FillPath(fill, shape);
            }

            if (child)
            {
                using var dot = new SolidBrush(Selected ? Color.FromArgb(245, 203, 114) : Color.FromArgb(128, 157, 164));
                e.Graphics.FillEllipse(dot, S(23), Height / 2f - S(2), S(4), S(4));
            }
            else
                IconPainter.Draw(e.Graphics, Text, new RectangleF(S(12), (Height - S(20)) / 2f, S(20), S(20)), Selected ? Color.FromArgb(231, 204, 153) : color);
            TextRenderer.DrawText(e.Graphics, Text, Font, new Rectangle(S(43), 0, Width - S(IsGroup ? 72 : 51), Height), color, TextFormatFlags.Left | TextFormatFlags.VerticalCenter | TextFormatFlags.NoPrefix | TextFormatFlags.SingleLine | TextFormatFlags.EndEllipsis | TextFormatFlags.NoPadding);
            if (IsGroup)
            {
                using var pen = new Pen(color, 1.5f * scale);
                int x = Width - S(19), y = Height / 2;
                if (Expanded)
                    e.Graphics.DrawLines(pen, new[] { new Point(x - S(4), y - S(2)), new Point(x, y + S(2)), new Point(x + S(4), y - S(2)) });
                else
                    e.Graphics.DrawLines(pen, new[] { new Point(x - S(2), y - S(4)), new Point(x + S(2), y), new Point(x - S(2), y + S(4)) });
            }

            if (Focused && ShowFocusCues)
                ControlPaint.DrawFocusRectangle(e.Graphics, Rectangle.Inflate(ClientRectangle, -S(4), -S(4)));
        }
    }
}
