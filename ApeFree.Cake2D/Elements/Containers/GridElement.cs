using System.Drawing;
using ApeFree.Cake2D.Rendering;
using ApeFree.Cake2D.Scene;

namespace ApeFree.Cake2D.Elements.Containers
{
    /// <summary>
    /// 无限背景网格图元。根据视口动态自适应绘制网格线。
    /// </summary>
    public class GridElement : Element2D
    {
        /// <summary>网格间距（世界单位）</summary>
        public float GridSpacing { get; set; } = 50f;

        /// <summary>是否根据视口缩放自动调整网格密度</summary>
        public bool AutoScale { get; set; } = true;

        public GridElement()
        {
            ZIndex = -1000; // 默认置于最底层
            IsInteractive = false;
            StrokeColor = Color.FromArgb(40, Color.Gray);
            LineWidth = 1f;
        }

        public override RectangleF GetLocalBounds() => RectangleF.Empty;

        protected override void OnRender(IRenderCanvas canvas, RenderContext2D context)
        {
            Viewport2D vp = context.Viewport;
            RectangleF viewRect = vp.GetCurrentWorldViewRect();

            float spacing = GridSpacing;
            if (AutoScale && spacing > 0)
            {
                float minPixelDist = 20f;
                while (spacing * vp.PixelsPerUnit < minPixelDist)
                {
                    spacing *= 2f;
                }
                while (spacing * vp.PixelsPerUnit > minPixelDist * 4f && spacing > 1f)
                {
                    spacing /= 2f;
                }
            }

            if (spacing <= 0) return;

            float startX = (float)(System.Math.Floor(viewRect.Left / spacing) * spacing);
            float endX = (float)(System.Math.Ceiling(viewRect.Right / spacing) * spacing);
            float startY = (float)(System.Math.Floor(viewRect.Top / spacing) * spacing);
            float endY = (float)(System.Math.Ceiling(viewRect.Bottom / spacing) * spacing);

            Color color = StrokeColor ?? Color.FromArgb(40, Color.Gray);
            float lineW = context.ResolveLineWidth(LineWidth);

            context.Submit(ZIndex, c =>
            {
                // 绘制纵向网格线
                for (float x = startX; x <= endX; x += spacing)
                {
                    PointF top = vp.WorldToScreen(new PointF(x, viewRect.Top));
                    PointF bottom = vp.WorldToScreen(new PointF(x, viewRect.Bottom));
                    c.DrawLine(top, bottom, color, lineW);
                }

                // 绘制横向网格线
                for (float y = startY; y <= endY; y += spacing)
                {
                    PointF left = vp.WorldToScreen(new PointF(viewRect.Left, y));
                    PointF right = vp.WorldToScreen(new PointF(viewRect.Right, y));
                    c.DrawLine(left, right, color, lineW);
                }
            });
        }
    }
}