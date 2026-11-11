using System.Collections.Generic;
using System.Drawing;
using ApeFree.Cake2D.Math;
using ApeFree.Cake2D.Rendering;

namespace ApeFree.Cake2D.Elements.Primitives
{
    /// <summary>
    /// 连续折线图元。
    /// 借助底层仿射变换矩阵，支持旋转、缩放与平移绘制。
    /// </summary>
    public class PolylineElement : Element2D
    {
        public List<PointF> Points { get; set; } = new List<PointF>();

        public PolylineElement() { }

        public PolylineElement(IEnumerable<PointF> points)
        {
            if (points != null) Points.AddRange(points);
        }

        public override RectangleF GetLocalBounds()
        {
            RectangleF bounds = Math2D.GetBounds(Points);
            float pad = System.Math.Max(LineWidth > 0 ? LineWidth : 2f, 5f);
            bounds.Inflate(pad, pad);
            return bounds;
        }

        public override bool HitTest(PointF worldPoint)
        {
            if (Points.Count < 2) return false;
            PointF localPt = WorldToLocal(worldPoint);
            float pad = System.Math.Max(LineWidth > 0 ? LineWidth / 2f : 1f, 5f);
            for (int i = 0; i < Points.Count - 1; i++)
            {
                if (Math2D.IsPointNearLine(localPt, Points[i], Points[i + 1], tolerance: pad))
                    return true;
            }
            return false;
        }

        protected override void OnRender(IRenderCanvas canvas, RenderContext2D context)
        {
            if (Points.Count < 2) return;

            Color stroke = StrokeColor ?? Color.Black;
            float lineW = context.ResolveLineWidth(LineWidth);

            Matrix3x2 transform = context.GetElementToScreenMatrix(this);
            canvas.PushTransform(transform);
            try
            {
                canvas.DrawPolyline(Points.ToArray(), stroke, lineW);
            }
            finally
            {
                canvas.PopTransform();
            }
        }
    }
}
