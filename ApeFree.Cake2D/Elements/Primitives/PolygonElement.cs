using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using ApeFree.Cake2D.Math;
using ApeFree.Cake2D.Rendering;

namespace ApeFree.Cake2D.Elements.Primitives
{
    /// <summary>
    /// 多边形图元。
    /// </summary>
    public class PolygonElement : Element2D
    {
        public List<PointF> Points { get; set; } = new List<PointF>();

        public PolygonElement() { }

        public PolygonElement(IEnumerable<PointF> points)
        {
            if (points != null) Points.AddRange(points);
        }

        public float Area => Math2D.CalculatePolygonArea(Points.ToArray());
        public float Perimeter => Math2D.CalculatePolygonPerimeter(Points.ToArray());
        public PointF Centroid => Math2D.CalculatePolygonCentroid(Points.ToArray());

        public override RectangleF GetLocalBounds() => Math2D.GetBounds(Points);

        public override bool HitTest(PointF worldPoint)
        {
            if (Points.Count < 3) return false;
            PointF localPt = WorldToLocal(worldPoint);
            return Math2D.IsPointInPolygon(Points.ToArray(), localPt);
        }

        protected override void OnRender(IRenderCanvas canvas, RenderContext2D context)
        {
            if (Points.Count < 3) return;

            Matrix3x2 worldMatrix = GetWorldMatrix();
            PointF[] screenPoints = Points.Select(p => context.Project(worldMatrix.TransformPoint(p))).ToArray();

            Color? stroke = StrokeColor;
            Color? fill = FillColor;
            float lineW = context.ResolveLineWidth(LineWidth);

            context.Submit(ZIndex, c =>
            {
                if (fill.HasValue && stroke.HasValue)
                {
                    c.FillAndStrokePolygon(screenPoints, fill.Value, stroke.Value, lineW);
                }
                else if (fill.HasValue)
                {
                    c.FillPolygon(screenPoints, fill.Value);
                }
                else if (stroke.HasValue)
                {
                    c.DrawPolygon(screenPoints, stroke.Value, lineW);
                }
            });
        }
    }
}