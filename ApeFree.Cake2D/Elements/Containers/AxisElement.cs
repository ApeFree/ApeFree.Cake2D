using System.Drawing;
using ApeFree.Cake2D.Rendering;
using ApeFree.Cake2D.Scene;

namespace ApeFree.Cake2D.Elements.Containers
{
    /// <summary>
    /// 坐标轴图元（绘制 X 轴与 Y 轴以及原点标记）。
    /// </summary>
    public class AxisElement : Element2D
    {
        public Color XAxisColor { get; set; } = Color.Red;
        public Color YAxisColor { get; set; } = Color.Green;

        public AxisElement()
        {
            ZIndex = -900;
            IsInteractive = false;
            LineWidth = 1.5f;
        }

        public override RectangleF GetLocalBounds() => RectangleF.Empty;

        protected override void OnRender(IRenderCanvas canvas, RenderContext2D context)
        {
            Viewport2D vp = context.Viewport;
            RectangleF viewRect = vp.GetCurrentWorldViewRect();

            PointF originScreen = vp.WorldToScreen(PointF.Empty);
            float lineW = context.ResolveLineWidth(LineWidth);

            Color xCol = XAxisColor;
            Color yCol = YAxisColor;

            context.Submit(ZIndex, c =>
            {
                // X 轴
                PointF xLeft = vp.WorldToScreen(new PointF(viewRect.Left, 0));
                PointF xRight = vp.WorldToScreen(new PointF(viewRect.Right, 0));
                c.DrawLine(xLeft, xRight, xCol, lineW);

                // Y 轴
                PointF yTop = vp.WorldToScreen(new PointF(0, viewRect.Top));
                PointF yBottom = vp.WorldToScreen(new PointF(0, viewRect.Bottom));
                c.DrawLine(yTop, yBottom, yCol, lineW);

                // 原点圆点
                c.FillCircle(originScreen, 3f, Color.Black);
            });
        }
    }
}