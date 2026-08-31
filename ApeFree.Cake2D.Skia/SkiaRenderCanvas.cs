using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using ApeFree.Cake2D.Rendering;
using SkiaSharp;

namespace ApeFree.Cake2D.Skia
{
    /// <summary>
    /// 基于 SkiaSharp 的 2D 渲染适配器，实现 <see cref="IRenderCanvas"/> 接口。
    /// </summary>
    public class SkiaRenderCanvas : IRenderCanvas
    {
        private readonly SKCanvas _canvas;

        public SKCanvas Canvas => _canvas;

        public SkiaRenderCanvas(SKCanvas canvas)
        {
            _canvas = canvas ?? throw new ArgumentNullException(nameof(canvas));
        }

        public void Clear(Color backgroundColor)
        {
            _canvas.Clear(ToSKColor(backgroundColor));
        }

        public void DrawLine(PointF p1, PointF p2, Color color, float width, bool dashed = false)
        {
            using (SKPaint paint = CreateStrokePaint(color, width, dashed))
            {
                _canvas.DrawLine(p1.X, p1.Y, p2.X, p2.Y, paint);
            }
        }

        public void DrawPolyline(PointF[] points, Color color, float width)
        {
            if (points == null || points.Length < 2) return;
            using (SKPath path = CreatePath(points, false))
            using (SKPaint paint = CreateStrokePaint(color, width))
            {
                _canvas.DrawPath(path, paint);
            }
        }

        public void DrawPolygon(PointF[] points, Color strokeColor, float width)
        {
            if (points == null || points.Length < 2) return;
            using (SKPath path = CreatePath(points, true))
            using (SKPaint paint = CreateStrokePaint(strokeColor, width))
            {
                _canvas.DrawPath(path, paint);
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
                    _canvas.DrawPath(path, strokePaint);
                }
            }
        }

        public void DrawRectangle(RectangleF rect, Color color, float width)
        {
            using (SKPaint paint = CreateStrokePaint(color, width))
            {
                _canvas.DrawRect(ToSKRect(rect), paint);
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
                _canvas.DrawRect(skRect, strokePaint);
            }
        }

        public void DrawCircle(PointF center, float radius, Color color, float width)
        {
            using (SKPaint paint = CreateStrokePaint(color, width))
            {
                _canvas.DrawCircle(center.X, center.Y, radius, paint);
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
                _canvas.DrawOval(center.X, center.Y, radiusX, radiusY, paint);
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
            using (SKTypeface tf = SKTypeface.FromFamilyName(string.IsNullOrEmpty(fontName) ? "Arial" : fontName, weight, SKFontStyleWidth.Normal, SKFontStyleSlant.Upright) ?? SKTypeface.Default)
            using (SKPaint paint = new SKPaint
            {
                Typeface = tf,
                TextSize = fontSize > 0 ? fontSize * 1.333f : 13.33f,
                Color = ToSKColor(color),
                IsAntialias = true
            })
            {
                SKRect textBounds = new SKRect();
                paint.MeasureText(text, ref textBounds);

                float drawX = position.X;
                float drawY = position.Y;

                // 水平对齐调整
                switch (alignment)
                {
                    case TextAlignment.TopLeft:
                    case TextAlignment.MiddleLeft:
                    case TextAlignment.BottomLeft:
                        drawX = position.X - textBounds.Left;
                        break;
                    case TextAlignment.TopCenter:
                    case TextAlignment.MiddleCenter:
                    case TextAlignment.BottomCenter:
                        drawX = position.X - textBounds.MidX;
                        break;
                    case TextAlignment.TopRight:
                    case TextAlignment.MiddleRight:
                    case TextAlignment.BottomRight:
                        drawX = position.X - textBounds.Right;
                        break;
                }

                // 垂直对齐调整（Skia 文本基线校准）
                switch (alignment)
                {
                    case TextAlignment.TopLeft:
                    case TextAlignment.TopCenter:
                    case TextAlignment.TopRight:
                        drawY = position.Y - textBounds.Top;
                        break;
                    case TextAlignment.MiddleLeft:
                    case TextAlignment.MiddleCenter:
                    case TextAlignment.MiddleRight:
                        drawY = position.Y - textBounds.MidY;
                        break;
                    case TextAlignment.BottomLeft:
                    case TextAlignment.BottomCenter:
                    case TextAlignment.BottomRight:
                        drawY = position.Y - textBounds.Bottom;
                        break;
                }

                _canvas.DrawText(text, drawX, drawY, paint);
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
            else if (image is Image gdiImage)
            {
                using (MemoryStream ms = new MemoryStream())
                {
                    gdiImage.Save(ms, ImageFormat.Png);
                    ms.Seek(0, SeekOrigin.Begin);
                    using (SKBitmap convertedBmp = SKBitmap.Decode(ms))
                    {
                        if (convertedBmp != null)
                        {
                            _canvas.DrawBitmap(convertedBmp, skDest);
                        }
                    }
                }
            }
        }

        private static SKPaint CreateStrokePaint(Color color, float width, bool dashed = false)
        {
            SKPaint paint = new SKPaint
            {
                Style = SKPaintStyle.Stroke,
                Color = ToSKColor(color),
                StrokeWidth = width < 0.5f ? 0.5f : width,
                IsAntialias = true,
                StrokeCap = SKStrokeCap.Round,
                StrokeJoin = SKStrokeJoin.Round
            };

            if (dashed)
            {
                paint.PathEffect = SKPathEffect.CreateDash(new float[] { 6f, 6f }, 0f);
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