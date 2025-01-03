﻿using ApeFree.Cake2D.Shapes;

namespace ApeFree.Cake2D
{
    /// <summary>
    /// 图层
    /// </summary>
    public partial class Layer<TStyle>
    {
        /// <summary>
        /// 风格样式
        /// </summary>
        public TStyle Style { get; set; }

        /// <summary>
        /// 图形
        /// </summary>
        public IShape Shape { get; set; }

        /// <summary>
        /// 可见性，画板绘制时是否绘制当前图层
        /// </summary>
        public bool Visible { get; set; } = true;

        /// <summary>
        /// 可选性，图层是否可以被选中
        /// </summary>
        public bool Selectable { get; set; } = true;

        /// <summary>
        /// 可聚焦
        /// </summary>
        public bool Focusable { get; set; }

        /// <summary>
        /// 画板
        /// </summary>
        public Palette<TStyle> Parent { get; }

        /// <summary>
        /// 附带数据
        /// </summary>
        public object Tag { get; set; }

        /// <summary>
        /// 构造图层
        /// </summary>
        /// <param name="parent">所属画板</param>
        internal Layer(Palette<TStyle> parent)
        {
            Parent = parent;
        }

        /// <summary>
        /// 构造图层
        /// </summary>
        /// <param name="parent">所属画板</param>
        /// <param name="style">样式风格</param>
        /// <param name="shape">图形</param>
        internal Layer(Palette<TStyle> parent, TStyle style, IShape shape) : this(parent)
        {
            Style = style;
            Shape = shape;
        }
    }

    public partial class Layer<TStyle, TShape> : Layer<TStyle> where TShape : IShape
    {
        /// <summary>
        /// 图形
        /// </summary>
        public new TShape Shape { get => (TShape)base.Shape; set => base.Shape = value; }

        internal Layer(Palette<TStyle> parent) : base(parent)
        {
        }

        internal Layer(Palette<TStyle> parent, TStyle style, IShape shape) : base(parent, style, shape)
        {
        }
    }
}
