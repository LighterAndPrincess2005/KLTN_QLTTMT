using System.Drawing.Drawing2D;
using System.Reflection;

namespace LhlArt.Views.Shared;

// Uses the same classroom photograph as the website home page.
internal sealed class StudioPhoto : Control
{
    static readonly Lazy<Image> Photograph = new(() => {
        using var stream = Assembly.GetExecutingAssembly().GetManifestResourceStream("LhlArt.Assets.classroom.jpg")!;
        using var source = Image.FromStream(stream);
        return new Bitmap(source);
    });
    public StudioPhoto()
    {
        DoubleBuffered = true;
        SetStyle(ControlStyles.SupportsTransparentBackColor | ControlStyles.ResizeRedraw, true);
        BackColor = Color.Transparent;
        AccessibleName = "Không gian lớp học Mỹ Thuật Bụi";
    }
    protected override void OnPaint(PaintEventArgs e)
    {
        base.OnPaint(e);
        if(Width < 20 || Height < 20) return;
        var g=e.Graphics;g.SmoothingMode=SmoothingMode.AntiAlias;
        g.InterpolationMode=InterpolationMode.HighQualityBicubic;
        float scale=DeviceDpi/96f;
        var bounds=new RectangleF(1,1,Width-2,Height-2);
        using var shape=IconPainter.Rounded(bounds,18*scale);
        var state=g.Save();g.SetClip(shape);
        var photo=Photograph.Value;
        float factor=Math.Max(bounds.Width/photo.Width,bounds.Height/photo.Height);
        var imageBounds=new RectangleF(bounds.X+(bounds.Width-photo.Width*factor)/2,
            bounds.Y+(bounds.Height-photo.Height*factor)/2,photo.Width*factor,photo.Height*factor);
        g.DrawImage(photo,imageBounds);
        float band=Math.Min(48*scale,Height/3f);
        using var shade=new SolidBrush(Color.FromArgb(220,23,79,69));
        g.FillRectangle(shade,0,Height-band,Width,band);
        using var font=Theme.Font(9,FontStyle.Bold);
        TextRenderer.DrawText(g,"Một chút màu. Một chút bạn.",font,
            new Rectangle((int)(12*scale),(int)(Height-band),Width-(int)(24*scale),(int)band),Color.White,
            TextFormatFlags.VerticalCenter|TextFormatFlags.HorizontalCenter|TextFormatFlags.EndEllipsis|TextFormatFlags.SingleLine);
        g.Restore(state);
    }
}
