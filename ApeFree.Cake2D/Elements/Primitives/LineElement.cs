using System.Drawing;
using ApeFree.Cake2D.Math;
using ApeFree.Cake2D.Rendering;

namespace ApeFree.Cake2D.Elements.Primitives
{
    /// <summary>
    /// 线段图元。
    /// </summary>
    public class LineElement : Element2D
    {
        public Vector2D Start { get; set; } = Vector2D.Zero;
        public Vector2D End { get; set; } = new Vector2D(100, 0);

        public LineElement() { }

        public LineElement(Vector2D start, Vector2D end)
        {
            Start = start;
            End = end;
        }

        public LineElement(float x1, float y1, float x2, float y2)
        {
            Start = new Vector2D(x1, y1);
            End = new Vector2D(x2, y2);
        }

        public override RectangleF GetLocalBounds()
        {
            return Math2D.GetBounds(new[] { Start.ToPointF(), End.ToPointF() });
        }

        public override bool HitTest(PointF worldPoint)
        {
            PointF localPt = WorldToLocal(worldPoint);
            return Math2D.IsPointNearLine(localPt, Start.ToPointF(), End.ToPointF(), tolerance: 5f);
        }

        protected override void OnRender(IRenderCanvas canvas, RenderContext2D context)
        {
            Matrix3x2 worldMatrix = GetWorldMatrix();
            PointF p1 = context.Project(worldMatrix.TransformPoint(Start.ToPointF()));
            PointF p2 = context.Project(worldMatrix.TransformPoint(End.ToPointF()));

            Color stroke = StrokeColor ?? Color.Black;
            float lineW = context.ResolveLineWidth(LineWidth);
            bool dashed = IsDashed;

            context.Submit(ZIndex, c =>
            {
                c.DrawLine(p1, p2, stroke, lineW, dashed);
            });
        }
    }
}