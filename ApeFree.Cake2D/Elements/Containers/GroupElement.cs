using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using ApeFree.Cake2D.Math;
using ApeFree.Cake2D.Rendering;

namespace ApeFree.Cake2D.Elements.Containers
{
    /// <summary>
    /// 图元容器/图层组（Group）。
    /// </summary>
    public class GroupElement : Element2D
    {
        public override RectangleF GetLocalBounds()
        {
            if (Children.Count == 0) return RectangleF.Empty;

            float minX = float.MaxValue;
            float minY = float.MaxValue;
            float maxX = float.MinValue;
            float maxY = float.MinValue;
            bool any = false;

            for (int i = 0; i < Children.Count; i++)
            {
                var child = Children[i];
                if (!child.IsVisible) continue;
                RectangleF cb = Math2D.GetTransformedBounds(child.GetLocalBounds(), child.GetLocalMatrix());
                if (cb.IsEmpty && cb.Width <= 0 && cb.Height <= 0) continue;

                any = true;
                if (cb.Left < minX) minX = cb.Left;
                if (cb.Right > maxX) maxX = cb.Right;
                if (cb.Top < minY) minY = cb.Top;
                if (cb.Bottom > maxY) maxY = cb.Bottom;
            }

            if (!any) return RectangleF.Empty;
            return new RectangleF(minX, minY, maxX - minX, maxY - minY);
        }

        protected override void OnRender(IRenderCanvas canvas, RenderContext2D context)
        {
            // 组自身不直接绘制，由基类遍历其 Children 进行绘制
        }
    }
}