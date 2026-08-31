using System;

namespace ApeFree.Cake2D.Rendering
{
    /// <summary>
    /// 渲染单元项，包含深度/层级顺序与延迟绘制委托。
    /// </summary>
    public class RenderItem2D
    {
        /// <summary>层级顺序 (ZIndex)，越小越先绘制（处于底层），越大越后绘制（处于顶层）</summary>
        public int ZIndex { get; set; }

        /// <summary>添加序列号（用于 ZIndex 相同时保持相对顺序稳定）</summary>
        public int Sequence { get; set; }

        /// <summary>实际绘制执行动作</summary>
        public Action<IRenderCanvas> DrawAction { get; set; }
    }
}