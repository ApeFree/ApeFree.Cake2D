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

        // ─── 本地空间变换（带脏标记缓存） ──────────────────────────────────────

        private Vector2D _position = Vector2D.Zero;
        private float _rotation = 0f;
        private Vector2D _scale = Vector2D.One;
        private Vector2D _origin = Vector2D.Zero;

        private bool _isLocalMatrixDirty = true;
        private bool _isWorldMatrixDirty = true;
        private Matrix3x2 _cachedLocalMatrix = Matrix3x2.Identity;
        private Matrix3x2 _cachedWorldMatrix = Matrix3x2.Identity;

        /// <summary>本地坐标系位置（相对于父节点）</summary>
        public Vector2D Position
        {
            get => _position;
            set
            {
                if (_position == value) return;
                _position = value;
                InvalidateTransform();
            }
        }

        /// <summary>旋转角度（度数，顺时针）</summary>
        public float Rotation
        {
            get => _rotation;
            set
            {
                if (_rotation == value) return;
                _rotation = value;
                InvalidateTransform();
            }
        }

        /// <summary>缩放比例</summary>
        public Vector2D Scale
        {
            get => _scale;
            set
            {
                if (_scale == value) return;
                _scale = value;
                InvalidateTransform();
            }
        }

        /// <summary>局部变换的原点/旋转锚点（相对于自身左上角）</summary>
        public Vector2D Origin
        {
            get => _origin;
            set
            {
                if (_origin == value) return;
                _origin = value;
                InvalidateTransform();
            }
        }

        /// <summary>使当前图元及所有后代图元的变换矩阵缓存失效</summary>
        public void InvalidateTransform()
        {
            _isLocalMatrixDirty = true;
            InvalidateWorldTransform();
        }

        private void InvalidateWorldTransform()
        {
            _isWorldMatrixDirty = true;
            for (int i = 0; i < Children.Count; i++)
            {
                Children[i].InvalidateWorldTransform();
            }
        }

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
            child.InvalidateWorldTransform();
        }

        public void RemoveChild(Element2D child)
        {
            if (child == null) return;
            if (Children.Remove(child))
            {
                child.Parent = null;
                child.InvalidateWorldTransform();
            }
        }

        // ─── 矩阵变换 ────────────────────────────────────────────────────────

        /// <summary>计算本元素的本地变换矩阵（带脏标记缓存）</summary>
        public virtual Matrix3x2 GetLocalMatrix()
        {
            if (!_isLocalMatrixDirty) return _cachedLocalMatrix;

            Matrix3x2 m = Matrix3x2.CreateTranslation(-_origin.X, -_origin.Y);
            if (_scale != Vector2D.One)
                m *= Matrix3x2.CreateScale(_scale);
            if (System.Math.Abs(_rotation) > 1e-5f)
                m *= Matrix3x2.CreateRotation(Math2D.ToRadians(_rotation));
            m *= Matrix3x2.CreateTranslation(_origin.X + _position.X, _origin.Y + _position.Y);

            _cachedLocalMatrix = m;
            _isLocalMatrixDirty = false;
            return m;
        }

        /// <summary>计算本元素的世界变换矩阵（递归叠加所有祖先节点的变换，带脏标记缓存）</summary>
        public virtual Matrix3x2 GetWorldMatrix()
        {
            if (!_isWorldMatrixDirty) return _cachedWorldMatrix;

            Matrix3x2 local = GetLocalMatrix();
            Matrix3x2 world = Parent != null ? local * Parent.GetWorldMatrix() : local;

            _cachedWorldMatrix = world;
            _isWorldMatrixDirty = false;
            return world;
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

        /// <summary>获取包含图元自身及整棵子树在世界坐标系下的联合 AABB 包围矩形</summary>
        public virtual RectangleF GetSubtreeWorldBounds()
        {
            RectangleF selfBounds = GetWorldBounds();
            if (Children.Count == 0) return selfBounds;

            float minX = selfBounds.IsEmpty ? float.MaxValue : selfBounds.Left;
            float minY = selfBounds.IsEmpty ? float.MaxValue : selfBounds.Top;
            float maxX = selfBounds.IsEmpty ? float.MinValue : selfBounds.Right;
            float maxY = selfBounds.IsEmpty ? float.MinValue : selfBounds.Bottom;
            bool any = !selfBounds.IsEmpty;

            for (int i = 0; i < Children.Count; i++)
            {
                var child = Children[i];
                if (!child.IsVisible) continue;
                RectangleF cb = child.GetSubtreeWorldBounds();
                if (cb.IsEmpty) continue;

                any = true;
                if (cb.Left < minX) minX = cb.Left;
                if (cb.Right > maxX) maxX = cb.Right;
                if (cb.Top < minY) minY = cb.Top;
                if (cb.Bottom > maxY) maxY = cb.Bottom;
            }

            if (!any) return RectangleF.Empty;
            return new RectangleF(minX, minY, maxX - minX, maxY - minY);
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

            // 视口视锥剔除：如果启用且图元（及其子树包围盒）在当前视口范围外，直接跳过
            if (context != null && context.EnableCulling && !context.IsVisibleInViewport(this))
            {
                return;
            }

            OnRender(canvas, context);

            if (Children.Count > 0)
            {
                // 子节点按 ZIndex 升序依次渲染
                int count = Children.Count;
                if (count == 1)
                {
                    Children[0].Render(canvas, context);
                }
                else
                {
                    // 检查是否已是有序
                    bool isSorted = true;
                    for (int i = 0; i < count - 1; i++)
                    {
                        if (Children[i].ZIndex > Children[i + 1].ZIndex)
                        {
                            isSorted = false;
                            break;
                        }
                    }

                    if (isSorted)
                    {
                        for (int i = 0; i < count; i++)
                        {
                            Children[i].Render(canvas, context);
                        }
                    }
                    else
                    {
                        var sorted = new List<Element2D>(Children);
                        sorted.Sort((a, b) => a.ZIndex.CompareTo(b.ZIndex));
                        for (int i = 0; i < sorted.Count; i++)
                        {
                            sorted[i].Render(canvas, context);
                        }
                    }
                }
            }
        }

        /// <summary>子类实现此方法完成自身的具体几何绘制</summary>
        protected abstract void OnRender(IRenderCanvas canvas, RenderContext2D context);
    }
}
