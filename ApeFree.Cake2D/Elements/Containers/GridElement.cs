using System.Drawing;
using ApeFree.Cake2D.Rendering;
using ApeFree.Cake2D.Scene;

namespace ApeFree.Cake2D.Elements.Containers
{
    /// <summary>
    /// 无限背景网格图元。根据视口动态自适应绘制网格线。
    /// 内置极端视口防护机制，杜绝死循环挂起与海量线条绘制卡顿。
    /// </summary>
    public class GridElement : Element2D
    {
        private const int MaxLinesPerAxis = 200;

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
            if (vp.PixelsPerUnit <= 1e-6f) return;

            RectangleF viewRect = vp.GetCurrentWorldViewRect();
            if (viewRect.Width <= 0f || viewRect.Height <= 0f) return;

            float spacing = GridSpacing;
            if (spacing <= 0f) return;

            if (AutoScale)
            {
                float minPixelDist = 20f;
                int maxIterations = 30; // 严格防御死循环

                while (spacing * vp.PixelsPerUnit < minPixelDist && maxIterations-- > 0)
                {
                    spacing *= 2f;
                }

                maxIterations = 30;
                while (spacing * vp.PixelsPerUnit > minPixelDist * 4f && spacing > 1f && maxIterations-- > 0)
                {
                    spacing /= 2f;
                }
            }

            // 安全防线：根据视野跨度防止绘制过密线条卡死 UI
            float spanX = viewRect.Width;
            float spanY = viewRect.Height;
            while (spanX / spacing > MaxLinesPerAxis || spanY / spacing > MaxLinesPerAxis)
            {
                spacing *= 2f;
            }

            float startX = (float)(System.Math.Floor(viewRect.Left / spacing) * spacing);
            float endX = (float)(System.Math.Ceiling(viewRect.Right / spacing) * spacing);
            float startY = (float)(System.Math.Floor(viewRect.Top / spacing) * spacing);
            float endY = (float)(System.Math.Ceiling(viewRect.Bottom / spacing) * spacing);

            Color color = StrokeColor ?? Color.FromArgb(40, Color.Gray);
            float lineW = context.ResolveLineWidth(LineWidth);

            // 绘制纵向网格线
            for (float x = startX; x <= endX; x += spacing)
            {
                PointF top = vp.WorldToScreen(new PointF(x, viewRect.Top));
                PointF bottom = vp.WorldToScreen(new PointF(x, viewRect.Bottom));
                canvas.DrawLine(top, bottom, color, lineW);
            }

            // 绘制横向网格线
            for (float y = startY; y <= endY; y += spacing)
            {
                PointF left = vp.WorldToScreen(new PointF(viewRect.Left, y));
                PointF right = vp.WorldToScreen(new PointF(viewRect.Right, y));
                canvas.DrawLine(left, right, color, lineW);
            }
        }
    }
}
