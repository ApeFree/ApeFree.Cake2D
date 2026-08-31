using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using ApeFree.Cake2D.Math;
using ApeFree.Cake2D.Rendering;

namespace ApeFree.Cake2D.Elements.Primitives
{
    /// <summary>
    /// 连续折线图元。
    /// </summary>
    public class PolylineElement : Element2D
    {
        public List<PointF> Points { get; set; } = new List<PointF>();

        public PolylineElement() { }

        public PolylineElement(IEnumerable<PointF> points)
        {
            if (points != null) Points.AddRange(points);
        }

        public override RectangleF GetLocalBounds() => Math2D.GetBounds(Points);

        public override bool HitTest(PointF worldPoint)
        {
            if (Points.Count < 2) return false;
            PointF localPt = WorldToLocal(worldPoint);
            for (int i = 0; i < Points.Count - 1; i++)
            {
                if (Math2D.IsPointNearLine(localPt, Points[i], Points[i + 1], tolerance: 5f))
                    return true;
            }
            return false;
        }

        protected override void OnRender(IRenderCanvas canvas, RenderContext2D context)
        {
            if (Points.Count < 2) return;

            Matrix3x2 worldMatrix = GetWorldMatrix();
            PointF[] screenPoints = Points.Select(p => context.Project(worldMatrix.TransformPoint(p))).ToArray();

            Color stroke = StrokeColor ?? Color.Black;
            float lineW = context.ResolveLineWidth(LineWidth);

            context.Submit(ZIndex, c =>
            {
                c.DrawPolyline(screenPoints, stroke, lineW);
            });
        }
    }
}