using System;
using System.Collections.Generic;
using System.Drawing;
using ApeFree.Cake2D.Events;
using ApeFree.Cake2D.Math;
using ApeFree.Cake2D.Rendering;

namespace ApeFree.Cake2D.Elements
{
    /// <summary>
    /// 所有二维场景图元的抽象基类（Scene Graph Node）。
    /// 提供层级树结构、仿射变换、包围盒、碰撞检测与鼠标交互事件分发。
    /// </summary>
    public abstract class Element2D
    {
        // ─── 标识与状态 ────────────────────────────────────────────────────────

        public string Name { get; set; } = string.Empty;
        public object Tag { get; set; }
        public bool IsVisible { get; set; } = true;
        public bool IsInteractive { get; set; } = true;
        public bool IsHovered { get; internal set; }
        public bool IsFocused { get; set; }
        public int ZIndex { get; set; } = 0;

        // ─── 本地空间变换 ────────────────────────────────────────────────────

        /// <summary>本地坐标系位置（相对于父节点）</summary>
        public Vector2D Position { get; set; } = Vector2D.Zero;

        /// <summary>旋转角度（度数，顺时针）</summary>
        public float Rotation { get; set; } = 0f;

        /// <summary>缩放比例</summary>
        public Vector2D Scale { get; set; } = Vector2D.One;

        /// <summary>局部变换的原点/旋转锚点（相对于自身左上角）</summary>
        public Vector2D Origin { get; set; } = Vector2D.Zero;

        // ─── 外观样式 ────────────────────────────────────────────────────────

        /// <summary>描边轮廓颜色（为 null 时不绘制轮廓）</summary>
        public Color? StrokeColor { get; set; } = Color.Black;

        /// <summary>填充颜色（为 null 时不填充内部）</summary>
        public Color? FillColor { get; set; }

        /// <summary>轮廓线宽（像素），&lt;= 0 时使用全局场景默认线宽</summary>
        public float LineWidth { get; set; } = 0f;

        /// <summary>是否使用虚线描边</summary>
        public bool IsDashed { get; set; } = false;

        // ─── 场景图树状层级 ──────────────────────────────────────────────────

        public Element2D Parent { get; private set; }
        public List<Element2D> Children { get; } = new List<Element2D>();

        public void AddChild(Element2D child)
        {
            if (child == null || child == this || Children.Contains(child)) return;
            child.Parent?.RemoveChild(child);
            child.Parent = this;
            Children.Add(child);
        }

        public void RemoveChild(Element2D child)
        {
            if (child == null) return;
            if (Children.Remove(child))
            {
                child.Parent = null;
            }
        }

        // ─── 矩阵变换 ────────────────────────────────────────────────────────

        /// <summary>计算本元素的本地变换矩阵</summary>
        public virtual Matrix3x2 GetLocalMatrix()
        {
            Matrix3x2 m = Matrix3x2.CreateTranslation(-Origin.X, -Origin.Y);
            if (Scale != Vector2D.One)
                m *= Matrix3x2.CreateScale(Scale);
            if (System.Math.Abs(Rotation) > 1e-5f)
                m *= Matrix3x2.CreateRotation(Math2D.ToRadians(Rotation));
            m *= Matrix3x2.CreateTranslation(Origin.X + Position.X, Origin.Y + Position.Y);
            return m;
        }

        /// <summary>计算本元素的世界变换矩阵（递归叠加所有祖先节点的变换）</summary>
        public virtual Matrix3x2 GetWorldMatrix()
        {
            Matrix3x2 local = GetLocalMatrix();
            return Parent != null ? local * Parent.GetWorldMatrix() : local;
        }

        /// <summary>本地坐标转世界坐标</summary>
        public PointF LocalToWorld(PointF localPt) => GetWorldMatrix().TransformPoint(localPt);

        /// <summary>世界坐标转本地坐标</summary>
        public PointF WorldToLocal(PointF worldPt)
        {
            if (Matrix3x2.Invert(GetWorldMatrix(), out Matrix3x2 inv))
            {
                return inv.TransformPoint(worldPt);
            }
            return worldPt;
        }

        // ─── 包围盒与命中测试 ────────────────────────────────────────────────

        /// <summary>获取图元在自身局部坐标系下的 AABB 包围矩形</summary>
        public abstract RectangleF GetLocalBounds();

        /// <summary>获取图元在世界坐标系下的 AABB 包围矩形</summary>
        public virtual RectangleF GetWorldBounds()
        {
            return Math2D.GetTransformedBounds(GetLocalBounds(), GetWorldMatrix());
        }

        /// <summary>测试给定的世界坐标点是否命中当前图元</summary>
        public virtual bool HitTest(PointF worldPoint)
        {
            PointF localPt = WorldToLocal(worldPoint);
            return GetLocalBounds().Contains(localPt);
        }

        // ─── 交互事件 ────────────────────────────────────────────────────────

        public event EventHandler<ElementMouseEventArgs> MouseEnter;
        public event EventHandler<ElementMouseEventArgs> MouseLeave;
        public event EventHandler<ElementMouseEventArgs> MouseDown;
        public event EventHandler<ElementMouseEventArgs> MouseMove;
        public event EventHandler<ElementMouseEventArgs> MouseUp;
        public event EventHandler<ElementMouseEventArgs> Click;
        public event EventHandler<ElementMouseEventArgs> DoubleClick;

        internal virtual void OnMouseEnter(ElementMouseEventArgs e) => MouseEnter?.Invoke(this, e);
        internal virtual void OnMouseLeave(ElementMouseEventArgs e) => MouseLeave?.Invoke(this, e);
        internal virtual void OnMouseDown(ElementMouseEventArgs e) => MouseDown?.Invoke(this, e);
        internal virtual void OnMouseMove(ElementMouseEventArgs e) => MouseMove?.Invoke(this, e);
        internal virtual void OnMouseUp(ElementMouseEventArgs e) => MouseUp?.Invoke(this, e);
        internal virtual void OnClick(ElementMouseEventArgs e) => Click?.Invoke(this, e);
        internal virtual void OnDoubleClick(ElementMouseEventArgs e) => DoubleClick?.Invoke(this, e);

        // ─── 渲染管线 ────────────────────────────────────────────────────────

        public void Render(IRenderCanvas canvas, RenderContext2D context)
        {
            if (!IsVisible) return;
            OnRender(canvas, context);
            foreach (Element2D child in Children)
            {
                child.Render(canvas, context);
            }
        }

        /// <summary>子类实现此方法完成自身的具体几何绘制</summary>
        protected abstract void OnRender(IRenderCanvas canvas, RenderContext2D context);
    }
}