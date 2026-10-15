using System.Drawing.Drawing2D;

namespace LhlArt.Views.Shared;

internal class StudioCard : Panel
{
    public Color Fill { get; set; } = Color.White;
    public bool Outline { get; set; } = true;
    public StudioCard() { DoubleBuffered=true; BackColor=Color.Transparent; Padding=new Padding(20); }
    protected override void OnPaintBackground(PaintEventArgs e)
    {
        e.Graphics.Clear(Parent?.BackColor??Theme.Paper);
        e.Graphics.SmoothingMode=SmoothingMode.AntiAlias;
        using var path=IconPainter.Rounded(new RectangleF(.5f,.5f,Width-1,Height-1),18*DeviceDpi/96f);
        using var fill=new SolidBrush(Fill); e.Graphics.FillPath(fill,path);
        if(Outline) { using var pen=new Pen(Theme.Line);e.Graphics.DrawPath(pen,path); }
    }
}

internal sealed class StudioInput : Panel
{
    public StudioInput(Control editor,string icon="",bool compact=false)
    {
        Height=compact?44:52;MinimumSize=new Size(120,Height);BackColor=Color.White;
        Padding=new Padding(icon.Length>0?42:15,12,15,10);Margin=new Padding(0,5,0,16);
        editor.Dock=DockStyle.Fill;editor.Font=Theme.Font(11);editor.ForeColor=Theme.Ink;editor.BackColor=Color.White;
        if(editor is TextBox text)text.BorderStyle=BorderStyle.None;
        if(editor is ComboBox select)select.FlatStyle=FlatStyle.Flat;
        Controls.Add(editor);DoubleBuffered=true;
        editor.GotFocus+=(_,_)=>Invalidate();editor.LostFocus+=(_,_)=>Invalidate();
        Click+=(_,_)=>editor.Focus();
        Paint+=(_,e)=>
        {
            e.Graphics.SmoothingMode=SmoothingMode.AntiAlias;
            using var path=IconPainter.Rounded(new RectangleF(1,1,Width-3,Height-3),10*DeviceDpi/96f);
            using var pen=new Pen(editor.Focused?Theme.Teal:Theme.Line,editor.Focused?2:1);
            e.Graphics.DrawPath(pen,path);
            if(icon.Length>0)IconPainter.Draw(e.Graphics,icon,new RectangleF(14,Height/2f-9,18,18),editor.Focused?Theme.Teal:Theme.Muted);
        };
    }
}

internal sealed class StudioArt : Control
{
    public StudioArt(){DoubleBuffered=true;SetStyle(ControlStyles.SupportsTransparentBackColor,true);BackColor=Color.Transparent;}
    protected override void OnPaint(PaintEventArgs e)
    {
        base.OnPaint(e);var g=e.Graphics;g.SmoothingMode=SmoothingMode.AntiAlias;
        var state=g.Save();float scale=Math.Min(Width/340f,Height/340f);
        g.TranslateTransform((Width-340*scale)/2,(Height-340*scale)/2);g.ScaleTransform(scale,scale);
        void Ell(Color c,float x,float y,float w,float h){using var b=new SolidBrush(c);g.FillEllipse(b,x,y,w,h);}
        using var pale=new SolidBrush(Color.FromArgb(241,232,213));g.FillEllipse(pale,22,30,290,290);
        var paperState=g.Save();g.TranslateTransform(170,180);g.RotateTransform(-8);
        using(var shadow=new SolidBrush(Color.FromArgb(218,218,199)))g.FillRectangle(shadow,-106,-132,224,270);
        using(var white=new SolidBrush(Color.White))g.FillRectangle(white,-114,-140,224,270);
        using(var bg=new SolidBrush(Color.FromArgb(234,241,230)))g.FillRectangle(bg,-99,-125,194,232);
        using(var vase=new SolidBrush(Theme.Coral))
        {
            using var v=new GraphicsPath();v.AddBezier(-47,13,-64,113,63,113,44,13);v.CloseFigure();g.FillPath(vase,v);
        }
        using(var stem=new Pen(Theme.Teal,4))
        {
            g.DrawBezier(stem,0,20,-3,-20,-29,-55,-26,-79);
            g.DrawBezier(stem,0,20,20,-30,50,-42,45,-90);
        }
        Ell(Theme.Teal,-48,-44,38,17);Ell(Theme.Teal,16,-27,39,18);
        for(int i=0;i<7;i++){var a=i*Math.PI*2/7;Ell(Theme.Coral,-26+(float)Math.Cos(a)*17-12,-80+(float)Math.Sin(a)*17-12,24,24);}
        Ell(Theme.Gold,-37,-91,22,22);
        for(int i=0;i<6;i++){var a=i*Math.PI*2/6;Ell(Theme.Gold,45+(float)Math.Cos(a)*16-10,-92+(float)Math.Sin(a)*16-10,20,20);}
        Ell(Theme.Teal,36,-101,18,18);g.Restore(paperState);
        using(var tape=new SolidBrush(Color.FromArgb(220,239,201,133)))g.FillRectangle(tape,126,25,84,24);
        Ell(Theme.Coral,290,50,13,13);Ell(Theme.Gold,14,261,19,19);
        using(var p=new Pen(Theme.Teal,3)) {g.DrawArc(p,273,263,36,28,20,280);g.DrawLine(p,30,76,42,65);g.DrawLine(p,27,58,43,57);}
        g.Restore(state);
    }
}
