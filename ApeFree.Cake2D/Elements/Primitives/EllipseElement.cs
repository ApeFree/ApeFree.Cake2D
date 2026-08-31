using System.Drawing;
using ApeFree.Cake2D.Math;
using ApeFree.Cake2D.Rendering;

namespace ApeFree.Cake2D.Elements.Primitives
{
    /// <summary>
    /// 椭圆图元（以 Position 为中心）。
    /// </summary>
    public class EllipseElement : Element2D
    {
        public float RadiusX { get; set; } = 60f;
        public float RadiusY { get; set; } = 40f;

        public EllipseElement() { }

        public EllipseElement(float radiusX, float radiusY)
        {
            RadiusX = radiusX;
            RadiusY = radiusY;
        }

        public EllipseElement(float centerX, float centerY, float radiusX, float radiusY)
        {
            Position = new Vector2D(centerX, centerY);
            RadiusX = radiusX;
            RadiusY = radiusY;
        }

        public override RectangleF GetLocalBounds() => new RectangleF(-RadiusX, -RadiusY, RadiusX * 2, RadiusY * 2);

        public override bool HitTest(PointF worldPoint)
        {
            PointF localPt = WorldToLocal(worldPoint);
            if (RadiusX <= 0 || RadiusY <= 0) return false;
            float dx = localPt.X / RadiusX;
            float dy = localPt.Y / RadiusY;
            return (dx * dx + dy * dy) <= 1.0f;
        }

        protected override void OnRender(IRenderCanvas canvas, RenderContext2D context)
        {
            Matrix3x2 worldMatrix = GetWorldMatrix();
            PointF screenCenter = context.Project(worldMatrix.TransformPoint(new PointF(0, 0)));

            float screenRx = context.ProjectLength(RadiusX * Scale.X);
            float screenRy = context.ProjectLength(RadiusY * Scale.Y);

            Color? stroke = StrokeColor;
            Color? fill = FillColor;
            float lineW = context.ResolveLineWidth(LineWidth);

            context.Submit(ZIndex, c =>
            {
                if (fill.HasValue)
                {
                    c.FillEllipse(screenCenter, screenRx, screenRy, fill.Value);
                }
                if (stroke.HasValue)
                {
                    c.DrawEllipse(screenCenter, screenRx, screenRy, stroke.Value, lineW);
                }
            });
        }
    }
}