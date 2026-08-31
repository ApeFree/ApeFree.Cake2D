using System.Drawing;
using ApeFree.Cake2D.Math;
using ApeFree.Cake2D.Rendering;

namespace ApeFree.Cake2D.Elements.Primitives
{
    /// <summary>
    /// 矩形图元。
    /// </summary>
    public class RectangleElement : Element2D
    {
        public float Width { get; set; } = 100f;
        public float Height { get; set; } = 100f;

        public RectangleElement() { }

        public RectangleElement(float width, float height)
        {
            Width = width;
            Height = height;
        }

        public RectangleElement(float x, float y, float width, float height)
        {
            Position = new Vector2D(x, y);
            Width = width;
            Height = height;
        }

        public override RectangleF GetLocalBounds() => new RectangleF(0, 0, Width, Height);

        protected override void OnRender(IRenderCanvas canvas, RenderContext2D context)
        {
            Matrix3x2 worldMatrix = GetWorldMatrix();

            // 顺时针获取矩形的四个顶点
            PointF p0 = context.Project(worldMatrix.TransformPoint(new PointF(0, 0)));
            PointF p1 = context.Project(worldMatrix.TransformPoint(new PointF(Width, 0)));
            PointF p2 = context.Project(worldMatrix.TransformPoint(new PointF(Width, Height)));
            PointF p3 = context.Project(worldMatrix.TransformPoint(new PointF(0, Height)));

            PointF[] screenPoints = new[] { p0, p1, p2, p3 };

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