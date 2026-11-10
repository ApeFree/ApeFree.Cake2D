using System.Drawing;
using ApeFree.Cake2D.Math;
using ApeFree.Cake2D.Rendering;

namespace ApeFree.Cake2D.Elements.Primitives
{
    /// <summary>
    /// 线段图元。
    /// 借助底层仿射变换矩阵，支持旋转、缩放与平移绘制。
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
            RectangleF bounds = Math2D.GetBounds(new[] { Start.ToPointF(), End.ToPointF() });
            // 考虑线宽膨胀，避免细长斜线被粗筛误杀
            float pad = System.Math.Max(LineWidth > 0 ? LineWidth : 2f, 5f);
            bounds.Inflate(pad, pad);
            return bounds;
        }

        public override bool HitTest(PointF worldPoint)
        {
            PointF localPt = WorldToLocal(worldPoint);
            float pad = System.Math.Max(LineWidth > 0 ? LineWidth / 2f : 1f, 5f);
            return Math2D.IsPointNearLine(localPt, Start.ToPointF(), End.ToPointF(), tolerance: pad);
        }

        protected override void OnRender(IRenderCanvas canvas, RenderContext2D context)
        {
            Color stroke = StrokeColor ?? Color.Black;
            float lineW = context.ResolveLineWidth(LineWidth);
            bool dashed = IsDashed;

            Matrix3x2 transform = context.GetElementToScreenMatrix(this);
            canvas.PushTransform(transform);
            try
            {
                canvas.DrawLine(Start.ToPointF(), End.ToPointF(), stroke, lineW, dashed);
            }
            finally
            {
                canvas.PopTransform();
            }
        }
    }
}
