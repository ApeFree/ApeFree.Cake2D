using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Runtime.CompilerServices;
using ApeFree.Cake2D.Math;
using ApeFree.Cake2D.Rendering;
using SkiaSharp;

#if CAKE2D_WINFORMS
using System.Drawing.Imaging;
#endif

namespace ApeFree.Cake2D.Skia
{
    /// <summary>
    /// 基于 SkiaSharp 的 2D 渲染适配器，实现 <see cref="IRenderCanvas"/> 接口。
    /// 原生全平台支持，严格管理 Skia C++ 非托管资源生命周期，杜绝孤儿句柄泄漏。
    /// </summary>
    public class SkiaRenderCanvas : IRenderCanvas
    {
        private readonly SKCanvas _canvas;
        private readonly Stack<float> _scaleStack = new Stack<float>();
        private float _currentScale = 1.0f;

#if CAKE2D_WINFORMS
        // 弱引用缓存表：仅在 Windows 环境下向下兼容 GDI+ Image 到 SKImage 的无损转换
        private static readonly ConditionalWeakTable<Image, SKImage> _gdiImageCache = new ConditionalWeakTable<Image, SKImage>();
#endif

        // Typeface 资源池缓存，容量上限 512
        private static readonly ConcurrentDictionary<string, SKTypeface> TypefaceCache = new ConcurrentDictionary<string, SKTypeface>();
        private const int MaxTypefaceCacheCapacity = 512;

        // 字符码点 -> 最佳匹配字体名称缓存
        private static readonly ConcurrentDictionary<int, string> RuneToFamilyCache = new ConcurrentDictionary<int, string>();

        public SKCanvas Canvas => _canvas;

        public SkiaRenderCanvas(SKCanvas canvas)
        {
            _canvas = canvas ?? throw new ArgumentNullException(nameof(canvas));
        }

        public void Clear(Color backgroundColor)
        {
            _canvas.Clear(ToSKColor(backgroundColor));
        }

        public void PushTransform(Matrix3x2 matrix)
        {
            _canvas.Save();
            _scaleStack.Push(_currentScale);

            float det = System.Math.Abs(matrix.M11 * matrix.M22 - matrix.M12 * matrix.M21);
            float scale = (float)System.Math.Sqrt(det > 1e-6f ? det : 1f);
            _currentScale *= scale;

            SKMatrix skMatrix = new SKMatrix
            {
                ScaleX = matrix.M11,
                SkewX = matrix.M21,
                TransX = matrix.M31,
                SkewY = matrix.M12,
                ScaleY = matrix.M22,
                TransY = matrix.M32,
                Persp0 = 0,
                Persp1 = 0,
                Persp2 = 1
            };
            _canvas.Concat(ref skMatrix);
        }

        public void PopTransform()
        {
            _canvas.Restore();
            _currentScale = _scaleStack.Count > 0 ? _scaleStack.Pop() : 1.0f;
        }

        public void DrawLine(PointF p1, PointF p2, Color color, float width, bool dashed = false)
        {
            using (SKPaint paint = CreateStrokePaint(color, width, dashed))
            {
                try
                {
                    _canvas.DrawLine(p1.X, p1.Y, p2.X, p2.Y, paint);
                }
                finally
                {
                    paint.PathEffect?.Dispose();
                }
            }
        }

        public void DrawPolyline(PointF[] points, Color color, float width)
        {
            if (points == null || points.Length < 2) return;
            using (SKPath path = CreatePath(points, false))
            using (SKPaint paint = CreateStrokePaint(color, width))
            {
                try
                {
                    _canvas.DrawPath(path, paint);
                }
                finally
                {
                    paint.PathEffect?.Dispose();
                }
            }
        }

        public void DrawPolygon(PointF[] points, Color strokeColor, float width)
        {
            if (points == null || points.Length < 2) return;
            using (SKPath path = CreatePath(points, true))
            using (SKPaint paint = CreateStrokePaint(strokeColor, width))
            {
                try
                {
                    _canvas.DrawPath(path, paint);
                }
                finally
                {
                    paint.PathEffect?.Dispose();
                }
            }
        }

        public void FillPolygon(PointF[] points, Color fillColor)
        {
            if (points == null || points.Length < 3) return;
            using (SKPath path = CreatePath(points, true))
            using (SKPaint paint = CreateFillPaint(fillColor))
            {
                _canvas.DrawPath(path, paint);
            }
        }

        public void FillAndStrokePolygon(PointF[] points, Color fillColor, Color strokeColor, float strokeWidth)
        {
            if (points == null || points.Length < 3) return;
            using (SKPath path = CreatePath(points, true))
            {
                using (SKPaint fillPaint = CreateFillPaint(fillColor))
                {
                    _canvas.DrawPath(path, fillPaint);
                }
                using (SKPaint strokePaint = CreateStrokePaint(strokeColor, strokeWidth))
                {
                    try
                    {
                        _canvas.DrawPath(path, strokePaint);
                    }
                    finally
                    {
                        strokePaint.PathEffect?.Dispose();
                    }
                }
            }
        }

        public void DrawRectangle(RectangleF rect, Color color, float width)
        {
            using (SKPaint paint = CreateStrokePaint(color, width))
            {
                try
                {
                    _canvas.DrawRect(ToSKRect(rect), paint);
                }
                finally
                {
                    paint.PathEffect?.Dispose();
                }
            }
        }

        public void FillRectangle(RectangleF rect, Color color)
        {
            using (SKPaint paint = CreateFillPaint(color))
            {
                _canvas.DrawRect(ToSKRect(rect), paint);
            }
        }

        public void FillAndStrokeRectangle(RectangleF rect, Color fillColor, Color strokeColor, float strokeWidth)
        {
            SKRect skRect = ToSKRect(rect);
            using (SKPaint fillPaint = CreateFillPaint(fillColor))
            {
                _canvas.DrawRect(skRect, fillPaint);
            }
            using (SKPaint strokePaint = CreateStrokePaint(strokeColor, strokeWidth))
            {
                try
                {
                    _canvas.DrawRect(skRect, strokePaint);
                }
                finally
                {
                    strokePaint.PathEffect?.Dispose();
                }
            }
        }

        public void DrawCircle(PointF center, float radius, Color color, float width)
        {
            using (SKPaint paint = CreateStrokePaint(color, width))
            {
                try
                {
                    _canvas.DrawCircle(center.X, center.Y, radius, paint);
                }
                finally
                {
                    paint.PathEffect?.Dispose();
                }
            }
        }

        public void FillCircle(PointF center, float radius, Color color)
        {
            using (SKPaint paint = CreateFillPaint(color))
            {
                _canvas.DrawCircle(center.X, center.Y, radius, paint);
            }
        }

        public void DrawEllipse(PointF center, float radiusX, float radiusY, Color color, float width)
        {
            using (SKPaint paint = CreateStrokePaint(color, width))
            {
                try
                {
                    _canvas.DrawOval(center.X, center.Y, radiusX, radiusY, paint);
                }
                finally
                {
                    paint.PathEffect?.Dispose();
                }
            }
        }

        public void FillEllipse(PointF center, float radiusX, float radiusY, Color color)
        {
            using (SKPaint paint = CreateFillPaint(color))
            {
                _canvas.DrawOval(center.X, center.Y, radiusX, radiusY, paint);
            }
        }

        public void DrawText(string text, PointF position, Color color, string fontName, float fontSize, bool isBold, TextAlignment alignment)
        {
            if (string.IsNullOrEmpty(text)) return;

            SKFontStyleWeight weight = isBold ? SKFontStyleWeight.Bold : SKFontStyleWeight.Normal;
            SKTypeface primaryTypeface = GetOrCreateTypeface(fontName, weight);
            float actualFontSize = fontSize > 0 ? fontSize : 10f;

            // Tier 1: 纯 ASCII 快速通道 (0 字符检测开销)
            if (IsAsciiOnly(text))
            {
                DrawSingleRun(text, position, color, primaryTypeface, actualFontSize, alignment);
                return;
            }

            // Tier 2: 探测当前主字体或首个匹配字体能否全覆盖
            if (CanTypefaceCoverAll(primaryTypeface, text))
            {
                DrawSingleRun(text, position, color, primaryTypeface, actualFontSize, alignment);
                return;
            }

            // 获取首个缺失字符的最佳匹配字体
            int firstMissingRune = FindFirstMissingRune(primaryTypeface, text);
            SKTypeface fallbackTypeface = GetFallbackTypefaceForRune(firstMissingRune, weight);

            if (CanTypefaceCoverAll(fallbackTypeface, text))
            {
                DrawSingleRun(text, position, color, fallbackTypeface, actualFontSize, alignment);
                return;
            }

            // Tier 3: 极端多语系混排兜底 (Run-level 切分绘制)
            DrawMultiRunText(text, position, color, primaryTypeface, weight, actualFontSize, alignment);
        }

        private static bool IsAsciiOnly(string s)
        {
            for (int i = 0; i < s.Length; i++)
            {
                if (s[i] > 127) return false;
            }
            return true;
        }

        private static int GetSafeRune(string text, ref int index)
        {
            char c = text[index];
            if (char.IsHighSurrogate(c))
            {
                if (index + 1 < text.Length && char.IsLowSurrogate(text[index + 1]))
                {
                    int rune = char.ConvertToUtf32(text, index);
                    index++; // 跳过低代理项
                    return rune;
                }
                return c;
            }
            return c;
        }

        private static bool CanTypefaceCoverAll(SKTypeface typeface, string text)
        {
            if (typeface == null) return false;
            int i = 0;
            while (i < text.Length)
            {
                if (text[i] > 127)
                {
                    int rune = GetSafeRune(text, ref i);
                    if (!typeface.ContainsGlyph(rune)) return false;
                }
                i++;
            }
            return true;
        }

        private static int FindFirstMissingRune(SKTypeface typeface, string text)
        {
            int i = 0;
            while (i < text.Length)
            {
                if (text[i] > 127)
                {
                    int rune = GetSafeRune(text, ref i);
                    if (typeface == null || !typeface.ContainsGlyph(rune))
                    {
                        return rune;
                    }
                }
                i++;
            }
            return 0;
        }

        private static SKTypeface GetFallbackTypefaceForRune(int rune, SKFontStyleWeight weight)
        {
            if (!RuneToFamilyCache.TryGetValue(rune, out string familyName))
            {
                using (SKTypeface matched = SKFontManager.Default.MatchCharacter(rune))
                {
                    familyName = matched?.FamilyName;
                }

                if (RuneToFamilyCache.Count < MaxTypefaceCacheCapacity)
                {
                    RuneToFamilyCache.TryAdd(rune, familyName);
                }
            }

            return GetOrCreateTypeface(familyName, weight);
        }

        private static SKTypeface GetOrCreateTypeface(string familyName, SKFontStyleWeight weight)
        {
            string key = $"{familyName ?? "Default"}_{weight}";
            if (TypefaceCache.TryGetValue(key, out var tf)) return tf;

            SKTypeface newTf;
            if (string.IsNullOrEmpty(familyName))
            {
                newTf = SKTypeface.FromFamilyName(null, weight, SKFontStyleWidth.Normal, SKFontStyleSlant.Upright) 
                        ?? SKTypeface.Default;
            }
            else
            {
                newTf = SKTypeface.FromFamilyName(familyName, weight, SKFontStyleWidth.Normal, SKFontStyleSlant.Upright) 
                        ?? SKTypeface.FromFamilyName(null, weight, SKFontStyleWidth.Normal, SKFontStyleSlant.Upright) 
                        ?? SKTypeface.Default;
            }

            if (TypefaceCache.Count < MaxTypefaceCacheCapacity)
            {
                return TypefaceCache.GetOrAdd(key, newTf);
            }
            return newTf;
        }

        private void DrawSingleRun(string text, PointF position, Color color, SKTypeface typeface, float fontSize, TextAlignment alignment)
        {
            using (SKPaint paint = new SKPaint
            {
                Typeface = typeface,
                TextSize = fontSize,
                Color = ToSKColor(color),
                IsAntialias = true
            })
            {
                SKRect textBounds = new SKRect();
                paint.MeasureText(text, ref textBounds);

                float drawX = position.X;
                float drawY = position.Y;

                switch (alignment)
                {
                    case TextAlignment.TopLeft:
                        drawY = position.Y - textBounds.Top;
                        break;
                    case TextAlignment.TopCenter:
                        drawX = position.X - textBounds.MidX;
                        drawY = position.Y - textBounds.Top;
                        break;
                    case TextAlignment.TopRight:
                        drawX = position.X - textBounds.Right;
                        drawY = position.Y - textBounds.Top;
                        break;
                    case TextAlignment.MiddleLeft:
                        drawY = position.Y - textBounds.MidY;
                        break;
                    case TextAlignment.MiddleCenter:
                        drawX = position.X - textBounds.MidX;
                        drawY = position.Y - textBounds.MidY;
                        break;
                    case TextAlignment.MiddleRight:
                        drawX = position.X - textBounds.Right;
                        drawY = position.Y - textBounds.MidY;
                        break;
                    case TextAlignment.BottomLeft:
                        drawY = position.Y - textBounds.Bottom;
                        break;
                    case TextAlignment.BottomCenter:
                        drawX = position.X - textBounds.MidX;
                        drawY = position.Y - textBounds.Bottom;
                        break;
                    case TextAlignment.BottomRight:
                        drawX = position.X - textBounds.Right;
                        drawY = position.Y - textBounds.Bottom;
                        break;
                }

                _canvas.DrawText(text, drawX, drawY, paint);
            }
        }

        private readonly struct TextRun
        {
            public readonly string Content;
            public readonly SKTypeface Typeface;

            public TextRun(string content, SKTypeface typeface)
            {
                Content = content;
                Typeface = typeface;
            }
        }

        private void DrawMultiRunText(string text, PointF position, Color color, SKTypeface primary, SKFontStyleWeight weight, float fontSize, TextAlignment alignment)
        {
            var runs = new List<TextRun>();
            int runStartIndex = 0;
            SKTypeface currentTf = null;

            int i = 0;
            while (i < text.Length)
            {
                int charStartIndex = i;
                int rune = GetSafeRune(text, ref i);
                i++;

                SKTypeface resolved = (primary != null && primary.ContainsGlyph(rune))
                    ? primary
                    : GetFallbackTypefaceForRune(rune, weight);

                if (currentTf == null)
                {
                    currentTf = resolved;
                    runStartIndex = charStartIndex;
                }
                else if (currentTf != resolved)
                {
                    runs.Add(new TextRun(text.Substring(runStartIndex, charStartIndex - runStartIndex), currentTf));
                    currentTf = resolved;
                    runStartIndex = charStartIndex;
                }
            }

            if (runStartIndex < text.Length && currentTf != null)
            {
                runs.Add(new TextRun(text.Substring(runStartIndex, text.Length - runStartIndex), currentTf));
            }

            if (runs.Count == 0) return;

            float totalWidth = 0f;
            float maxAscent = 0f;
            float maxDescent = 0f;
            var runWidths = new float[runs.Count];

            using (SKPaint measurePaint = new SKPaint { TextSize = fontSize, IsAntialias = true })
            {
                for (int r = 0; r < runs.Count; r++)
                {
                    measurePaint.Typeface = runs[r].Typeface;
                    SKFontMetrics metrics = measurePaint.FontMetrics;
                    if (-metrics.Ascent > maxAscent) maxAscent = -metrics.Ascent;
                    if (metrics.Descent > maxDescent) maxDescent = metrics.Descent;

                    runWidths[r] = measurePaint.MeasureText(runs[r].Content);
                    totalWidth += runWidths[r];
                }
            }

            float totalHeight = maxAscent + maxDescent;
            float originX = position.X;
            float baselineY = position.Y + maxAscent;

            switch (alignment)
            {
                case TextAlignment.TopCenter:
                case TextAlignment.MiddleCenter:
                case TextAlignment.BottomCenter:
                    originX -= totalWidth * 0.5f;
                    break;
                case TextAlignment.TopRight:
                case TextAlignment.MiddleRight:
                case TextAlignment.BottomRight:
                    originX -= totalWidth;
                    break;
            }

            switch (alignment)
            {
                case TextAlignment.MiddleLeft:
                case TextAlignment.MiddleCenter:
                case TextAlignment.MiddleRight:
                    baselineY -= totalHeight * 0.5f;
                    break;
                case TextAlignment.BottomLeft:
                case TextAlignment.BottomCenter:
                case TextAlignment.BottomRight:
                    baselineY -= totalHeight;
                    break;
            }

            using (SKPaint drawPaint = new SKPaint { TextSize = fontSize, Color = ToSKColor(color), IsAntialias = true })
            {
                float currentX = originX;
                for (int r = 0; r < runs.Count; r++)
                {
                    drawPaint.Typeface = runs[r].Typeface;
                    _canvas.DrawText(runs[r].Content, currentX, baselineY, drawPaint);
                    currentX += runWidths[r];
                }
            }
        }

        public void DrawImage(object image, RectangleF destRect)
        {
            if (image == null) return;

            SKRect skDest = ToSKRect(destRect);

            if (image is SKImage skImage)
            {
                _canvas.DrawImage(skImage, skDest);
            }
            else if (image is SKBitmap skBitmap)
            {
                _canvas.DrawBitmap(skBitmap, skDest);
            }
#if CAKE2D_WINFORMS
            else if (image is Image gdiImage)
            {
                if (!_gdiImageCache.TryGetValue(gdiImage, out SKImage cachedImage))
                {
                    cachedImage = ConvertGdiImageToSKImage(gdiImage);
                    if (cachedImage != null)
                    {
                        _gdiImageCache.Add(gdiImage, cachedImage);
                    }
                }

                if (cachedImage != null)
                {
                    _canvas.DrawImage(cachedImage, skDest);
                }
            }
#endif
        }

#if CAKE2D_WINFORMS
        private static SKImage ConvertGdiImageToSKImage(Image gdiImage)
        {
            if (gdiImage is Bitmap bmp)
            {
                BitmapData data = bmp.LockBits(
                    new Rectangle(0, 0, bmp.Width, bmp.Height),
                    ImageLockMode.ReadOnly,
                    PixelFormat.Format32bppPArgb);
                try
                {
                    SKImageInfo info = new SKImageInfo(bmp.Width, bmp.Height, SKColorType.Bgra8888, SKAlphaType.Premul);
                    return SKImage.FromPixelCopy(info, data.Scan0, data.Stride);
                }
                finally
                {
                    bmp.UnlockBits(data);
                }
            }
            else
            {
                using (Bitmap tmp = new Bitmap(gdiImage.Width, gdiImage.Height, PixelFormat.Format32bppPArgb))
                {
                    using (Graphics g = Graphics.FromImage(tmp))
                    {
                        g.DrawImage(gdiImage, 0, 0, gdiImage.Width, gdiImage.Height);
                    }
                    BitmapData data = tmp.LockBits(
                        new Rectangle(0, 0, tmp.Width, tmp.Height),
                        ImageLockMode.ReadOnly,
                        PixelFormat.Format32bppPArgb);
                    try
                    {
                        SKImageInfo info = new SKImageInfo(tmp.Width, tmp.Height, SKColorType.Bgra8888, SKAlphaType.Premul);
                        return SKImage.FromPixelCopy(info, data.Scan0, data.Stride);
                    }
                    finally
                    {
                        tmp.UnlockBits(data);
                    }
                }
            }
        }
#endif

        private SKPaint CreateStrokePaint(Color color, float width, bool dashed = false)
        {
            float effectiveWidth = width / _currentScale;
            if (effectiveWidth < 0.0001f) effectiveWidth = 0.0001f;

            SKPaint paint = new SKPaint
            {
                Style = SKPaintStyle.Stroke,
                Color = ToSKColor(color),
                StrokeWidth = effectiveWidth,
                IsAntialias = true,
                StrokeCap = SKStrokeCap.Round,
                StrokeJoin = SKStrokeJoin.Round
            };

            if (dashed)
            {
                // 创建 DashEffect，并在外部使用完毕后销毁
                paint.PathEffect = SKPathEffect.CreateDash(new float[] { 6f / _currentScale, 6f / _currentScale }, 0f);
            }

            return paint;
        }

        private static SKPaint CreateFillPaint(Color color)
        {
            return new SKPaint
            {
                Style = SKPaintStyle.Fill,
                Color = ToSKColor(color),
                IsAntialias = true
            };
        }

        private static SKPath CreatePath(PointF[] points, bool close)
        {
            SKPath path = new SKPath();
            if (points == null || points.Length == 0) return path;

            path.MoveTo(points[0].X, points[0].Y);
            for (int i = 1; i < points.Length; i++)
            {
                path.LineTo(points[i].X, points[i].Y);
            }

            if (close)
            {
                path.Close();
            }

            return path;
        }

        public static SKColor ToSKColor(Color color) => new SKColor(color.R, color.G, color.B, color.A);
        public static SKPoint ToSKPoint(PointF pt) => new SKPoint(pt.X, pt.Y);
        public static SKRect ToSKRect(RectangleF rect) => new SKRect(rect.Left, rect.Top, rect.Right, rect.Bottom);
        public static SKPoint[] ToSKPoints(PointF[] points) => points.Select(ToSKPoint).ToArray();
    }
}
