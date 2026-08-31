using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using ApeFree.Cake2D.Rendering;

namespace ApeFree.Cake2D.Gdi
{
    /// <summary>
    /// 基于 GDI+ (System.Drawing) 的 2D 渲染适配器，实现 <see cref="IRenderCanvas"/> 接口。
    /// </summary>
    public class GdiRenderCanvas : IRenderCanvas
    {
        private readonly Graphics _g;

        public Graphics Graphics => _g;

        public GdiRenderCanvas(Graphics g)
        {
            _g = g ?? throw new ArgumentNullException(nameof(g));
        }

        public void Clear(Color backgroundColor)
        {
            _g.Clear(backgroundColor);
        }

        public void DrawLine(PointF p1, PointF p2, Color color, float width, bool dashed = false)
        {
            using (Pen pen = CreatePen(color, width, dashed))
            {
                _g.DrawLine(pen, p1, p2);
            }
        }

        public void DrawPolyline(PointF[] points, Color color, float width)
        {
            if (points == null || points.Length < 2) return;
            using (Pen pen = CreatePen(color, width))
            {
                _g.DrawLines(pen, points);
            }
        }

        public void DrawPolygon(PointF[] points, Color strokeColor, float width)
        {
            if (points == null || points.Length < 2) return;
            using (Pen pen = CreatePen(strokeColor, width))
            {
                _g.DrawPolygon(pen, points);
            }
        }

        public void FillPolygon(PointF[] points, Color fillColor)
        {
            if (points == null || points.Length < 3) return;
            using (SolidBrush brush = new SolidBrush(fillColor))
            {
                _g.FillPolygon(brush, points);
            }
        }

        public void FillAndStrokePolygon(PointF[] points, Color fillColor, Color strokeColor, float strokeWidth)
        {
            if (points == null || points.Length < 3) return;
            using (SolidBrush brush = new SolidBrush(fillColor))
            using (Pen pen = CreatePen(strokeColor, strokeWidth))
            {
                _g.FillPolygon(brush, points);
                _g.DrawPolygon(pen, points);
            }
        }

        public void DrawRectangle(RectangleF rect, Color color, float width)
        {
            using (Pen pen = CreatePen(color, width))
            {
                _g.DrawRectangle(pen, rect.X, rect.Y, rect.Width, rect.Height);
            }
        }

        public void FillRectangle(RectangleF rect, Color color)
        {
            using (SolidBrush brush = new SolidBrush(color))
            {
                _g.FillRectangle(brush, rect);
            }
        }

        public void FillAndStrokeRectangle(RectangleF rect, Color fillColor, Color strokeColor, float strokeWidth)
        {
            using (SolidBrush brush = new SolidBrush(fillColor))
            using (Pen pen = CreatePen(strokeColor, strokeWidth))
            {
                _g.FillRectangle(brush, rect);
                _g.DrawRectangle(pen, rect.X, rect.Y, rect.Width, rect.Height);
            }
        }

        public void DrawCircle(PointF center, float radius, Color color, float width)
        {
            using (Pen pen = CreatePen(color, width))
            {
                _g.DrawEllipse(pen, center.X - radius, center.Y - radius, radius * 2f, radius * 2f);
            }
        }

        public void FillCircle(PointF center, float radius, Color color)
        {
            using (SolidBrush brush = new SolidBrush(color))
            {
                _g.FillEllipse(brush, center.X - radius, center.Y - radius, radius * 2f, radius * 2f);
            }
        }

        public void DrawEllipse(PointF center, float radiusX, float radiusY, Color color, float width)
        {
            using (Pen pen = CreatePen(color, width))
            {
                _g.DrawEllipse(pen, center.X - radiusX, center.Y - radiusY, radiusX * 2f, radiusY * 2f);
            }
        }

        public void FillEllipse(PointF center, float radiusX, float radiusY, Color color)
        {
            using (SolidBrush brush = new SolidBrush(color))
            {
                _g.FillEllipse(brush, center.X - radiusX, center.Y - radiusY, radiusX * 2f, radiusY * 2f);
            }
        }

        public void DrawText(string text, PointF position, Color color, string fontName, float fontSize, bool isBold, TextAlignment alignment)
        {
            if (string.IsNullOrEmpty(text)) return;

            FontStyle style = isBold ? FontStyle.Bold : FontStyle.Regular;
            using (Font font = new Font(string.IsNullOrEmpty(fontName) ? "Arial" : fontName, fontSize > 0 ? fontSize : 10f, style))
            using (SolidBrush brush = new SolidBrush(color))
            using (StringFormat sf = CreateStringFormat(alignment))
            {
                _g.DrawString(text, font, brush, position, sf);
            }
        }

        public void DrawImage(object image, RectangleF destRect)
        {
            if (image is Image img)
            {
                _g.DrawImage(img, destRect);
            }
        }

        private static Pen CreatePen(Color color, float width, bool dashed = false)
        {
            Pen pen = new Pen(color, width < 0.5f ? 0.5f : width);
            if (dashed)
            {
                pen.DashStyle = DashStyle.Dash;
            }
            return pen;
        }

        private static StringFormat CreateStringFormat(TextAlignment alignment)
        {
            StringFormat sf = new StringFormat();
            switch (alignment)
            {
                case TextAlignment.TopLeft:
                    sf.Alignment = StringAlignment.Near;
                    sf.LineAlignment = StringAlignment.Near;
                    break;
                case TextAlignment.TopCenter:
                    sf.Alignment = StringAlignment.Center;
                    sf.LineAlignment = StringAlignment.Near;
                    break;
                case TextAlignment.TopRight:
                    sf.Alignment = StringAlignment.Far;
                    sf.LineAlignment = StringAlignment.Near;
                    break;
                case TextAlignment.MiddleLeft:
                    sf.Alignment = StringAlignment.Near;
                    sf.LineAlignment = StringAlignment.Center;
                    break;
                case TextAlignment.MiddleCenter:
                    sf.Alignment = StringAlignment.Center;
                    sf.LineAlignment = StringAlignment.Center;
                    break;
                case TextAlignment.MiddleRight:
                    sf.Alignment = StringAlignment.Far;
                    sf.LineAlignment = StringAlignment.Center;
                    break;
                case TextAlignment.BottomLeft:
                    sf.Alignment = StringAlignment.Near;
                    sf.LineAlignment = StringAlignment.Far;
                    break;
                case TextAlignment.BottomCenter:
                    sf.Alignment = StringAlignment.Center;
                    sf.LineAlignment = StringAlignment.Far;
                    break;
                case TextAlignment.BottomRight:
                    sf.Alignment = StringAlignment.Far;
                    sf.LineAlignment = StringAlignment.Far;
                    break;
            }
            return sf;
        }
    }
}