using System.Drawing;
using ApeFree.Cake2D.Math;
using ApeFree.Cake2D.Rendering;

namespace ApeFree.Cake2D.Elements.Primitives
{
    /// <summary>
    /// 正圆图元（以 Position 为圆心）。
    /// 借助底层仿射变换矩阵实现旋转与缩放下的精确几何呈现。
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
            Color? stroke = StrokeColor;
            Color? fill = FillColor;
            float lineW = context.ResolveLineWidth(LineWidth);

            Matrix3x2 transform = context.GetElementToScreenMatrix(this);
            canvas.PushTransform(transform);
            try
            {
                if (fill.HasValue)
                {
                    canvas.FillCircle(PointF.Empty, Radius, fill.Value);
                }
                if (stroke.HasValue)
                {
                    canvas.DrawCircle(PointF.Empty, Radius, stroke.Value, lineW);
                }
            }
            finally
            {
                canvas.PopTransform();
            }
        }
    }
}
