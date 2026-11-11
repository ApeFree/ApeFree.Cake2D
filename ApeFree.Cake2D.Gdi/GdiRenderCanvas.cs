using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using ApeFree.Cake2D.Math;
using ApeFree.Cake2D.Rendering;

namespace ApeFree.Cake2D.Gdi
{
    /// <summary>
    /// 基于 GDI+ (System.Drawing) 的 2D 渲染适配器，实现 <see cref="IRenderCanvas"/> 接口。
    /// 内置 Pen/Brush/Font 静态资源池，杜绝每帧高频申请与销毁 Windows GDI 原生句柄。
    /// </summary>
    public class GdiRenderCanvas : IRenderCanvas
    {
        private readonly Graphics _g;
        private readonly Stack<GraphicsState> _stateStack = new Stack<GraphicsState>();
        private readonly Stack<float> _scaleStack = new Stack<float>();
        private float _currentScale = 1.0f;

        // 静态预创建 9 种常用的 StringFormat，避免每帧高频分配与申请 GDI Handle
        private static readonly Dictionary<TextAlignment, StringFormat> StringFormatCache = InitializeStringFormats();

        // 静态对象池与容量安全保护：复用 Pen、SolidBrush 与 Font，杜绝高频 GDI 句柄创建与内存无界膨胀
        private const int MaxCacheCapacity = 512;
        private static readonly ConcurrentDictionary<int, SolidBrush> BrushCache = new ConcurrentDictionary<int, SolidBrush>();
        private static readonly ConcurrentDictionary<string, Font> FontCache = new ConcurrentDictionary<string, Font>();
        private static readonly ConcurrentDictionary<int, Pen> CosmeticPenCache = new ConcurrentDictionary<int, Pen>();
        private static readonly ConcurrentDictionary<PenKey, Pen> PenCache = new ConcurrentDictionary<PenKey, Pen>();

        private readonly struct PenKey : IEquatable<PenKey>
        {
            public readonly int Argb;
            public readonly int WidthKey;
            public readonly bool Dashed;

            public PenKey(int argb, float width, bool dashed)
            {
                Argb = argb;
                WidthKey = (int)System.Math.Round(width * 4f); // 0.25px 步长量化，大幅抑制缩放时 key 爆炸
                Dashed = dashed;
            }

            public bool Equals(PenKey other) => Argb == other.Argb && WidthKey == other.WidthKey && Dashed == other.Dashed;
            public override bool Equals(object obj) => obj is PenKey other && Equals(other);
            public override int GetHashCode() => (Argb * 397) ^ (WidthKey * 31) ^ (Dashed ? 1 : 0);
        }

        public Graphics Graphics => _g;

        public GdiRenderCanvas(Graphics g)
        {
            _g = g ?? throw new ArgumentNullException(nameof(g));
        }

        public void Clear(Color backgroundColor)
        {
            _g.Clear(backgroundColor);
        }

        public void PushTransform(Matrix3x2 matrix)
        {
            _stateStack.Push(_g.Save());
            _scaleStack.Push(_currentScale);

            // 计算当前附加矩阵的平均缩放比例
            float det = System.Math.Abs(matrix.M11 * matrix.M22 - matrix.M12 * matrix.M21);
            float scale = (float)System.Math.Sqrt(det > 1e-6f ? det : 1f);
            _currentScale *= scale;

            using (var gdiMatrix = new System.Drawing.Drawing2D.Matrix(matrix.M11, matrix.M12, matrix.M21, matrix.M22, matrix.M31, matrix.M32))
            {
                _g.MultiplyTransform(gdiMatrix, MatrixOrder.Prepend);
            }
        }

        public void PopTransform()
        {
            if (_stateStack.Count > 0)
            {
                _g.Restore(_stateStack.Pop());
                _currentScale = _scaleStack.Count > 0 ? _scaleStack.Pop() : 1.0f;
            }
        }

        public void DrawLine(PointF p1, PointF p2, Color color, float width, bool dashed = false)
        {
            Pen pen = GetOrCreatePen(color, width, dashed);
            _g.DrawLine(pen, p1, p2);
        }

        public void DrawPolyline(PointF[] points, Color color, float width)
        {
            if (points == null || points.Length < 2) return;
            Pen pen = GetOrCreatePen(color, width);
            _g.DrawLines(pen, points);
        }

        public void DrawPolygon(PointF[] points, Color strokeColor, float width)
        {
            if (points == null || points.Length < 2) return;
            Pen pen = GetOrCreatePen(strokeColor, width);
            _g.DrawPolygon(pen, points);
        }

        public void FillPolygon(PointF[] points, Color fillColor)
        {
            if (points == null || points.Length < 3) return;
            SolidBrush brush = GetOrCreateBrush(fillColor);
            _g.FillPolygon(brush, points);
        }

        public void FillAndStrokePolygon(PointF[] points, Color fillColor, Color strokeColor, float strokeWidth)
        {
            if (points == null || points.Length < 3) return;
            SolidBrush brush = GetOrCreateBrush(fillColor);
            Pen pen = GetOrCreatePen(strokeColor, strokeWidth);
            _g.FillPolygon(brush, points);
            _g.DrawPolygon(pen, points);
        }

        public void DrawRectangle(RectangleF rect, Color color, float width)
        {
            Pen pen = GetOrCreatePen(color, width);
            _g.DrawRectangle(pen, rect.X, rect.Y, rect.Width, rect.Height);
        }

        public void FillRectangle(RectangleF rect, Color color)
        {
            SolidBrush brush = GetOrCreateBrush(color);
            _g.FillRectangle(brush, rect);
        }

        public void FillAndStrokeRectangle(RectangleF rect, Color fillColor, Color strokeColor, float strokeWidth)
        {
            SolidBrush brush = GetOrCreateBrush(fillColor);
            Pen pen = GetOrCreatePen(strokeColor, strokeWidth);
            _g.FillRectangle(brush, rect);
            _g.DrawRectangle(pen, rect.X, rect.Y, rect.Width, rect.Height);
        }

        public void DrawCircle(PointF center, float radius, Color color, float width)
        {
            Pen pen = GetOrCreatePen(color, width);
            _g.DrawEllipse(pen, center.X - radius, center.Y - radius, radius * 2f, radius * 2f);
        }

        public void FillCircle(PointF center, float radius, Color color)
        {
            SolidBrush brush = GetOrCreateBrush(color);
            _g.FillEllipse(brush, center.X - radius, center.Y - radius, radius * 2f, radius * 2f);
        }

        public void DrawEllipse(PointF center, float radiusX, float radiusY, Color color, float width)
        {
            Pen pen = GetOrCreatePen(color, width);
            _g.DrawEllipse(pen, center.X - radiusX, center.Y - radiusY, radiusX * 2f, radiusY * 2f);
        }

        public void FillEllipse(PointF center, float radiusX, float radiusY, Color color)
        {
            SolidBrush brush = GetOrCreateBrush(color);
            _g.FillEllipse(brush, center.X - radiusX, center.Y - radiusY, radiusX * 2f, radiusY * 2f);
        }

        public void DrawText(string text, PointF position, Color color, string fontName, float fontSize, bool isBold, TextAlignment alignment)
        {
            if (string.IsNullOrEmpty(text)) return;

            FontStyle style = isBold ? FontStyle.Bold : FontStyle.Regular;
            StringFormat sf = GetStringFormat(alignment);
            Font font = GetOrCreateFont(fontName, fontSize, style);
            SolidBrush brush = GetOrCreateBrush(color);

            _g.DrawString(text, font, brush, position, sf);
        }

        public void DrawImage(object image, RectangleF destRect)
        {
            if (image is Image img)
            {
                _g.DrawImage(img, destRect);
            }
        }

        private Pen GetOrCreatePen(Color color, float width, bool dashed = false)
        {
            // 针对 1px 屏幕线宽（最普遍情况），采用 GDI+ 原生 Cosmetic Pen（width=0），线宽恒为 1px 设备像素且与仿射缩放脱钩
            if (System.Math.Abs(width - 1.0f) < 1e-3f)
            {
                int cosmeticKey = color.ToArgb() ^ (dashed ? 1 : 0);
                if (CosmeticPenCache.TryGetValue(cosmeticKey, out Pen cPen))
                    return cPen;

                if (CosmeticPenCache.Count < MaxCacheCapacity)
                {
                    Pen newCPen = new Pen(color, 0f); // 0f 触发 GDI+ 原生 Cosmetic 模式
                    if (dashed) newCPen.DashStyle = DashStyle.Dash;
                    return CosmeticPenCache.GetOrAdd(cosmeticKey, newCPen);
                }
            }

            // 对于非 1px 线宽，计算当前变换下的本地线宽（0.25px 离散化量化，有效遏制连续缩放下的 Key 膨胀）
            float effectiveWidth = width / _currentScale;
            if (effectiveWidth < 0.0001f) effectiveWidth = 0.0001f;

            var key = new PenKey(color.ToArgb(), effectiveWidth, dashed);
            if (PenCache.TryGetValue(key, out Pen pen))
            {
                return pen;
            }

            Pen newPen = new Pen(color, effectiveWidth);
            if (dashed) newPen.DashStyle = DashStyle.Dash;

            // 容量保护：当达到上限时不再将非核心临时实例入池，杜绝清仓颠簸与 Use-After-Free 崩溃
            if (PenCache.Count < MaxCacheCapacity)
            {
                return PenCache.GetOrAdd(key, newPen);
            }

            return newPen;
        }

        private static SolidBrush GetOrCreateBrush(Color color)
        {
            int argb = color.ToArgb();
            if (BrushCache.TryGetValue(argb, out SolidBrush brush))
            {
                return brush;
            }

            SolidBrush newBrush = new SolidBrush(Color.FromArgb(argb));
            if (BrushCache.Count < MaxCacheCapacity)
            {
                return BrushCache.GetOrAdd(argb, newBrush);
            }

            return newBrush;
        }

        private static Font GetOrCreateFont(string fontName, float fontSize, FontStyle style)
        {
            string name = string.IsNullOrEmpty(fontName) ? "Arial" : fontName;
            float size = fontSize > 0 ? fontSize : 10f;
            string key = $"{name}_{size}_{style}";

            if (FontCache.TryGetValue(key, out Font font))
            {
                return font;
            }

            Font newFont = new Font(name, size, style);
            if (FontCache.Count < MaxCacheCapacity)
            {
                return FontCache.GetOrAdd(key, newFont);
            }

            return newFont;
        }

        private static StringFormat GetStringFormat(TextAlignment alignment)
        {
            return StringFormatCache.TryGetValue(alignment, out var sf) ? sf : StringFormat.GenericDefault;
        }

        private static Dictionary<TextAlignment, StringFormat> InitializeStringFormats()
        {
            var dict = new Dictionary<TextAlignment, StringFormat>();
            dict[TextAlignment.TopLeft] = new StringFormat { Alignment = StringAlignment.Near, LineAlignment = StringAlignment.Near };
            dict[TextAlignment.TopCenter] = new StringFormat { Alignment = StringAlignment.Center, LineAlignment = StringAlignment.Near };
            dict[TextAlignment.TopRight] = new StringFormat { Alignment = StringAlignment.Far, LineAlignment = StringAlignment.Near };
            dict[TextAlignment.MiddleLeft] = new StringFormat { Alignment = StringAlignment.Near, LineAlignment = StringAlignment.Center };
            dict[TextAlignment.MiddleCenter] = new StringFormat { Alignment = StringAlignment.Center, LineAlignment = StringAlignment.Center };
            dict[TextAlignment.MiddleRight] = new StringFormat { Alignment = StringAlignment.Far, LineAlignment = StringAlignment.Center };
            dict[TextAlignment.BottomLeft] = new StringFormat { Alignment = StringAlignment.Near, LineAlignment = StringAlignment.Far };
            dict[TextAlignment.BottomCenter] = new StringFormat { Alignment = StringAlignment.Center, LineAlignment = StringAlignment.Far };
            dict[TextAlignment.BottomRight] = new StringFormat { Alignment = StringAlignment.Far, LineAlignment = StringAlignment.Far };
            return dict;
        }
    }
}
