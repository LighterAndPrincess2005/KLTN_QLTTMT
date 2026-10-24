using System.Drawing.Drawing2D;

namespace LhlArt.Views.Shared;
internal static class IconPainter
{
    public static void Draw(Graphics graphics, string key, RectangleF bounds, Color color)
    {
        var state = graphics.Save();
        graphics.TranslateTransform(bounds.X, bounds.Y);
        graphics.ScaleTransform(bounds.Width / 24f, bounds.Height / 24f);
        graphics.SmoothingMode = SmoothingMode.AntiAlias;
        using var pen = new Pen(color, 1.65f)
        {
            StartCap = LineCap.Round,
            EndCap = LineCap.Round,
            LineJoin = LineJoin.Round
        };
        void Line(float x, float y, float x2, float y2) => graphics.DrawLine(pen, x, y, x2, y2);
        void Box(float x, float y, float w, float h) => graphics.DrawRectangle(pen, x, y, w, h);
        switch (key)
        {
            case "Tìm kiếm":
                graphics.DrawEllipse(pen,3,3,12,12);Line(13,13,21,21);
                break;
            case "Làm mới":
                graphics.DrawArc(pen,4,4,16,16,35,285);Line(20,4,20,10);Line(15,10,20,10);
                break;
            case "Đăng xuất":
                Line(10,3,4,3);Line(4,3,4,21);Line(4,21,10,21);Line(10,12,22,12);Line(18,8,22,12);Line(18,16,22,12);
                break;
            case "Tổng quan":
                Box(3, 3, 7, 7);
                Box(14, 3, 7, 7);
                Box(3, 14, 7, 7);
                Box(14, 14, 7, 7);
                break;
            case "Đào tạo":
            case "Khóa học":
                graphics.DrawPolygon(pen, new[] { new PointF(2, 8), new PointF(12, 3), new PointF(22, 8), new PointF(12, 13) });
                Line(6, 11, 6, 17);
                graphics.DrawArc(pen, 6, 13, 12, 7, 0, 180);
                Line(18, 11, 18, 17);
                Line(22, 8, 22, 16);
                break;
            case "Hồ sơ":
            case "Học viên":
            case "Tài khoản":
                graphics.DrawEllipse(pen, 8, 3, 8, 8);
                graphics.DrawArc(pen, 4, 13, 16, 14, 180, 180);
                Line(4, 20, 20, 20);
                break;
            case "Đăng ký & học phí":
            case "Đăng ký hiệu lực":
                Box(5, 4, 14, 17);
                Box(9, 2, 6, 4);
                Line(8, 11, 10, 13);
                Line(10, 13, 15, 9);
                Line(8, 17, 16, 17);
                break;
            case "Theo dõi học tập":
            case "Lịch học":
                Box(3, 5, 18, 16);
                Line(3, 10, 21, 10);
                Line(7, 2, 7, 7);
                Line(17, 2, 17, 7);
                Line(7, 14, 9, 14);
                Line(14, 14, 16, 14);
                Line(7, 18, 9, 18);
                break;
            case "Họa cụ & kho":
            case "Họa cụ cần bổ sung":
                graphics.DrawPolygon(pen, new[] { new PointF(3, 7), new PointF(12, 3), new PointF(21, 7), new PointF(21, 17), new PointF(12, 21), new PointF(3, 17) });
                Line(3, 7, 12, 11);
                Line(21, 7, 12, 11);
                Line(12, 11, 12, 21);
                Line(8, 5, 17, 9);
                break;
            case "Ưu đãi":
                graphics.DrawPolygon(pen, new[] { new PointF(3, 3), new PointF(13, 3), new PointF(22, 12), new PointF(12, 22), new PointF(3, 13) });
                graphics.DrawEllipse(pen, 7, 7, 3, 3);
                break;
            case "Báo cáo":
            case "Doanh thu họa cụ":
                Line(3, 3, 3, 21);
                Line(3, 21, 22, 21);
                Box(7, 12, 3, 6);
                Box(13, 8, 3, 10);
                Box(19, 4, 3, 14);
                break;
            case "Quản trị":
                graphics.DrawPolygon(pen, new[] { new PointF(12, 2), new PointF(21, 6), new PointF(19, 16), new PointF(12, 22), new PointF(5, 16), new PointF(3, 6) });
                Line(8, 11, 11, 14);
                Line(11, 14, 16, 9);
                break;
            case "Thực thu học phí":
                Box(3, 6, 18, 14);
                Line(3, 6, 17, 3);
                Box(14, 11, 7, 5);
                Line(17, 13, 18, 13);
                break;
            case "Lớp học":
            case "Lớp phụ trách":
                Box(3, 4, 18, 13);
                Line(8, 21, 16, 21);
                Line(12, 17, 12, 21);
                Line(7, 8, 16, 8);
                Line(7, 12, 12, 12);
                break;
            default:
                graphics.DrawEllipse(pen, 5, 5, 14, 14);
                Line(9, 12, 15, 12);
                break;
        }

        graphics.Restore(state);
    }

    public static GraphicsPath Rounded(RectangleF rectangle, float radius)
    {
        var path = new GraphicsPath();
        float size = radius * 2;
        path.AddArc(rectangle.Left, rectangle.Top, size, size, 180, 90);
        path.AddArc(rectangle.Right - size, rectangle.Top, size, size, 270, 90);
        path.AddArc(rectangle.Right - size, rectangle.Bottom - size, size, size, 0, 90);
        path.AddArc(rectangle.Left, rectangle.Bottom - size, size, size, 90, 90);
        path.CloseFigure();
        return path;
    }
}
