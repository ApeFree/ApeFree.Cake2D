using System.Collections.Generic;
using System.Drawing;
using ApeFree.Cake2D.Math;
using ApeFree.Cake2D.Rendering;

namespace ApeFree.Cake2D.Elements.Primitives
{
    /// <summary>
    /// 多边形图元。
    /// 借助底层仿射变换矩阵，支持旋转、缩放与平移绘制。
    /// </summary>
    public class PolygonElement : Element2D
    {
        public List<PointF> Points { get; set; } = new List<PointF>();

        public PolygonElement() { }

        public PolygonElement(IEnumerable<PointF> points)
        {
            if (points != null) Points.AddRange(points);
        }

        public float Area => Math2D.CalculatePolygonArea(Points);
        public float Perimeter => Math2D.CalculatePolygonPerimeter(Points);
        public PointF Centroid => Math2D.CalculatePolygonCentroid(Points);

        public override RectangleF GetLocalBounds() => Math2D.GetBounds(Points);

        public override bool HitTest(PointF worldPoint)
        {
            if (Points.Count < 3) return false;
            PointF localPt = WorldToLocal(worldPoint);
            return Math2D.IsPointInPolygon(Points, localPt);
        }

        protected override void OnRender(IRenderCanvas canvas, RenderContext2D context)
        {
            if (Points.Count < 3) return;

            Color? stroke = StrokeColor;
            Color? fill = FillColor;
            float lineW = context.ResolveLineWidth(LineWidth);

            Matrix3x2 transform = context.GetElementToScreenMatrix(this);
            canvas.PushTransform(transform);
            try
            {
                PointF[] pts = Points.ToArray();
                if (fill.HasValue && stroke.HasValue)
                {
                    canvas.FillAndStrokePolygon(pts, fill.Value, stroke.Value, lineW);
                }
                else if (fill.HasValue)
                {
                    canvas.FillPolygon(pts, fill.Value);
                }
                else if (stroke.HasValue)
                {
                    canvas.DrawPolygon(pts, stroke.Value, lineW);
                }
            }
            finally
            {
                canvas.PopTransform();
            }
        }
    }
}
