using System.Drawing;
using ApeFree.Cake2D.Math;
using ApeFree.Cake2D.Rendering;

namespace ApeFree.Cake2D.Elements.Primitives
{
    /// <summary>
    /// 正圆图元（以 Position 为圆心）。
    /// </summary>
    public class CircleElement : Element2D
    {
        public float Radius { get; set; } = 50f;

        public CircleElement() { }

        public CircleElement(float radius)
        {
            Radius = radius;
        }

        public CircleElement(float centerX, float centerY, float radius)
        {
            Position = new Vector2D(centerX, centerY);
            Radius = radius;
        }

        public override RectangleF GetLocalBounds() => new RectangleF(-Radius, -Radius, Radius * 2, Radius * 2);

        public override bool HitTest(PointF worldPoint)
        {
            PointF localPt = WorldToLocal(worldPoint);
            return (localPt.X * localPt.X + localPt.Y * localPt.Y) <= (Radius * Radius);
        }

        protected override void OnRender(IRenderCanvas canvas, RenderContext2D context)
        {
            Matrix3x2 worldMatrix = GetWorldMatrix();
            PointF worldCenter = worldMatrix.TransformPoint(new PointF(0, 0));
            PointF screenCenter = context.Project(worldCenter);

            // 计算世界半径在屏幕上的像素长度（考虑缩放）
            float screenRadius = context.ProjectLength(Radius * System.Math.Max(Scale.X, Scale.Y));

            Color? stroke = StrokeColor;
            Color? fill = FillColor;
            float lineW = context.ResolveLineWidth(LineWidth);

            context.Submit(ZIndex, c =>
            {
                if (fill.HasValue)
                {
                    c.FillCircle(screenCenter, screenRadius, fill.Value);
                }
                if (stroke.HasValue)
                {
                    c.DrawCircle(screenCenter, screenRadius, stroke.Value, lineW);
                }
            });
        }
    }
}