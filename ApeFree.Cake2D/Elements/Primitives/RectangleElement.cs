using System.Drawing;
using ApeFree.Cake2D.Math;
using ApeFree.Cake2D.Rendering;

namespace ApeFree.Cake2D.Elements.Primitives
{
    /// <summary>
    /// 矩形图元。
    /// 借助底层仿射变换矩阵，支持旋转、缩放与平移绘制，免除顶点数组高频堆分配。
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
            if (Width <= 0f || Height <= 0f) return;

            Color? stroke = StrokeColor;
            Color? fill = FillColor;
            float lineW = context.ResolveLineWidth(LineWidth);

            Matrix3x2 transform = context.GetElementToScreenMatrix(this);
            canvas.PushTransform(transform);
            try
            {
                RectangleF localRect = new RectangleF(0, 0, Width, Height);
                if (fill.HasValue && stroke.HasValue)
                {
                    canvas.FillAndStrokeRectangle(localRect, fill.Value, stroke.Value, lineW);
                }
                else if (fill.HasValue)
                {
                    canvas.FillRectangle(localRect, fill.Value);
                }
                else if (stroke.HasValue)
                {
                    canvas.DrawRectangle(localRect, stroke.Value, lineW);
                }
            }
            finally
            {
                canvas.PopTransform();
            }
        }
    }
}
