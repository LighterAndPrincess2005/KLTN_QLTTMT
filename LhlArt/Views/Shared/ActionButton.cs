using System.Drawing.Drawing2D;

namespace LhlArt.Views.Shared;
internal sealed class ActionButton : Button
{
    public string IconKey { get; set; } = "";
    public bool Navigation { get; set; }
    public bool IsSelected { get; set; }
    bool hovered;
    bool pressed;
    public ActionButton()
    {
        UseMnemonic = false;
        SetStyle(ControlStyles.UserPaint | ControlStyles.AllPaintingInWmPaint | ControlStyles.OptimizedDoubleBuffer | ControlStyles.ResizeRedraw, true);
    }
    public override Size GetPreferredSize(Size proposedSize)
    {
        var size=base.GetPreferredSize(proposedSize);
        if(IconKey.Length>0&&!Navigation)size.Width=Math.Max(MinimumSize.Width,TextRenderer.MeasureText(Text,Font).Width+Padding.Horizontal+(int)(36*DeviceDpi/96f));
        return size;
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
        pressed = false;
        Invalidate();
        base.OnMouseLeave(e);
    }

    protected override void OnMouseDown(MouseEventArgs e)
    {
        pressed = true;
        Invalidate();
        base.OnMouseDown(e);
    }

    protected override void OnMouseUp(MouseEventArgs e)
    {
        pressed = false;
        Invalidate();
        base.OnMouseUp(e);
    }

    protected override void OnPaint(PaintEventArgs e)
    {
        e.Graphics.Clear(Parent is StudioCard card ? card.Fill : Parent?.BackColor is { A: 255 } background ? background : Theme.Paper);
        e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
        float scale = DeviceDpi / 96f;
        using var path = IconPainter.Rounded(new RectangleF(0.5f, 0.5f, Width - 1, Height - 1), 12 * scale);
        bool primary = BackColor == Theme.Teal;
        Color fill = !Enabled ? Theme.Sage : IsSelected ? Theme.Teal : primary ? pressed ? Theme.Navy : hovered ? Theme.Navy : Theme.Teal : pressed ? Theme.Sage : hovered ? Theme.Sage : BackColor;
        using var brush = new SolidBrush(fill);
        using var border = new Pen(primary || Navigation ? fill : Theme.Line);
        e.Graphics.FillPath(brush, path);
        e.Graphics.DrawPath(border, path);
        var color = !Enabled ? Theme.Muted : IsSelected ? Color.White : ForeColor;
        var textBounds = Rectangle.Inflate(ClientRectangle, -(int)(14 * scale), 0);
        if(IconKey.Length>0)
        {
            IconPainter.Draw(e.Graphics,IconKey,new RectangleF(15*scale,(Height-20*scale)/2,20*scale,20*scale),color);
            textBounds.X+=(int)(32*scale);textBounds.Width-=(int)(32*scale);
        }
        TextRenderer.DrawText(e.Graphics, Text, Font, textBounds, color, (Navigation ? TextFormatFlags.Left : TextFormatFlags.HorizontalCenter) | TextFormatFlags.VerticalCenter | TextFormatFlags.NoPrefix | TextFormatFlags.SingleLine | TextFormatFlags.EndEllipsis);
        if (Focused && ShowFocusCues)
            ControlPaint.DrawFocusRectangle(e.Graphics, Rectangle.Inflate(ClientRectangle, -(int)(5 * scale), -(int)(5 * scale)));
    }
}
