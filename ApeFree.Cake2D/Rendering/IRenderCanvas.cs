using System.Drawing;

namespace ApeFree.Cake2D.Rendering
{
    /// <summary>
    /// 平台无关的 2D 绘图基元接口。
    /// 所有坐标均为屏幕像素坐标（原点位于视口左上角）。
    /// </summary>
    public interface IRenderCanvas
    {
        /// <summary>以指定颜色清空画布</summary>
        void Clear(Color backgroundColor);

        /// <summary>绘制线段</summary>
        void DrawLine(PointF p1, PointF p2, Color color, float width, bool dashed = false);

        /// <summary>绘制连续折线</summary>
        void DrawPolyline(PointF[] points, Color color, float width);

        /// <summary>绘制多边形轮廓</summary>
        void DrawPolygon(PointF[] points, Color strokeColor, float width);

        /// <summary>填充多边形内部</summary>
        void FillPolygon(PointF[] points, Color fillColor);

        /// <summary>填充并描边多边形</summary>
        void FillAndStrokePolygon(PointF[] points, Color fillColor, Color strokeColor, float strokeWidth);

        /// <summary>绘制矩形轮廓</summary>
        void DrawRectangle(RectangleF rect, Color color, float width);

        /// <summary>填充矩形内部</summary>
        void FillRectangle(RectangleF rect, Color color);

        /// <summary>填充并描边矩形</summary>
        void FillAndStrokeRectangle(RectangleF rect, Color fillColor, Color strokeColor, float strokeWidth);

        /// <summary>绘制正圆形轮廓</summary>
        void DrawCircle(PointF center, float radius, Color color, float width);

        /// <summary>填充正圆形内部</summary>
        void FillCircle(PointF center, float radius, Color color);

        /// <summary>绘制椭圆轮廓</summary>
        void DrawEllipse(PointF center, float radiusX, float radiusY, Color color, float width);

        /// <summary>填充椭圆内部</summary>
        void FillEllipse(PointF center, float radiusX, float radiusY, Color color);

        /// <summary>绘制文本字符串</summary>
        void DrawText(string text, PointF position, Color color, string fontName, float fontSize, bool isBold, TextAlignment alignment);

        /// <summary>绘制图像纹理</summary>
        void DrawImage(object image, RectangleF destRect);
    }
}