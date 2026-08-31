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
            return Math2D.GetBounds(Children.Select(c => c.GetWorldBounds()).SelectMany(r => new[]
            {
                new PointF(r.Left, r.Top),
                new PointF(r.Right, r.Bottom)
            }));
        }

        protected override void OnRender(IRenderCanvas canvas, RenderContext2D context)
        {
            // 组自身不直接绘制，由基类遍历其 Children 进行绘制
        }
    }
}