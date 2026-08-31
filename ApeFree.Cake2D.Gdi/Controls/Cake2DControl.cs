using System;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Text;
using System.Windows.Forms;
using ApeFree.Cake2D.Events;
using ApeFree.Cake2D.Rendering;
using ApeFree.Cake2D.Scene;

namespace ApeFree.Cake2D.Gdi.Controls
{
    /// <summary>
    /// 基于 GDI+ 的 Cake2D 二维视口控件（Windows Forms UserControl）。
    /// </summary>
    public class Cake2DControl : UserControl
    {
        private Scene2D _scene;
        private readonly Engine2D _engine = new Engine2D();

        /// <summary>当前绑定的 2D 场景</summary>
        [Browsable(false)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public Scene2D Scene
        {
            get => _scene;
            set
            {
                if (_scene != null)
                {
                    _scene.InvalidateRequested -= OnSceneInvalidateRequested;
                }
                _scene = value;
                if (_scene != null)
                {
                    _scene.Viewport.Resize(Width, Height);
                    _scene.InvalidateRequested += OnSceneInvalidateRequested;
                }
                Invalidate();
            }
        }

        public Cake2DControl()
        {
            DoubleBuffered = true;
            SetStyle(
                ControlStyles.AllPaintingInWmPaint |
                ControlStyles.UserPaint |
                ControlStyles.OptimizedDoubleBuffer |
                ControlStyles.ResizeRedraw,
                true);

            Scene = new Scene2D();
        }

        private void OnSceneInvalidateRequested(object sender, EventArgs e)
        {
            if (IsHandleCreated && !IsDisposed)
            {
                Invalidate();
            }
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);

            if (_scene == null)
            {
                e.Graphics.Clear(BackColor);
                return;
            }

            if (_scene.AntiAlias)
            {
                e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
                e.Graphics.TextRenderingHint = TextRenderingHint.AntiAliasGridFit;
                e.Graphics.InterpolationMode = InterpolationMode.HighQualityBicubic;
            }

            GdiRenderCanvas canvas = new GdiRenderCanvas(e.Graphics);
            _engine.Render(_scene, canvas);

            // 绘制当前状态机前景（如选框）
            _scene.CurrentState?.OnPaint(canvas);
        }

        protected override void OnResize(EventArgs e)
        {
            base.OnResize(e);
            if (_scene != null)
            {
                _scene.Viewport.Resize(Width, Height);
                Invalidate();
            }
        }

        protected override void OnMouseDown(MouseEventArgs e)
        {
            base.OnMouseDown(e);
            if (_scene == null) return;

            ElementMouseEventArgs args = CreateMouseEventArgs(e);
            bool handled = _scene.InteractionManager.OnMouseDown(args);
            if (!handled)
            {
                _scene.CurrentState?.OnMouseDown(args);
            }
        }

        protected override void OnMouseMove(MouseEventArgs e)
        {
            base.OnMouseMove(e);
            if (_scene == null) return;

            ElementMouseEventArgs args = CreateMouseEventArgs(e);
            bool handled = _scene.InteractionManager.OnMouseMove(args);
            if (!handled)
            {
                _scene.CurrentState?.OnMouseMove(args);
            }
        }

        protected override void OnMouseUp(MouseEventArgs e)
        {
            base.OnMouseUp(e);
            if (_scene == null) return;

            ElementMouseEventArgs args = CreateMouseEventArgs(e);
            bool handled = _scene.InteractionManager.OnMouseUp(args);
            if (!handled)
            {
                _scene.CurrentState?.OnMouseUp(args);
            }
        }

        protected override void OnMouseWheel(MouseEventArgs e)
        {
            if (e is HandledMouseEventArgs handledE)
            {
                handledE.Handled = true;
            }
            base.OnMouseWheel(e);
            if (_scene == null) return;

            ElementMouseEventArgs args = CreateMouseEventArgs(e);
            _scene.CurrentState?.OnMouseWheel(args);
        }

        private ElementMouseEventArgs CreateMouseEventArgs(MouseEventArgs e)
        {
            PointF screenLoc = e.Location;
            PointF worldLoc = _scene.Viewport.ScreenToWorld(screenLoc);
            CakeMouseButton btn = CakeMouseButton.None;
            if ((e.Button & MouseButtons.Left) == MouseButtons.Left) btn |= CakeMouseButton.Left;
            if ((e.Button & MouseButtons.Right) == MouseButtons.Right) btn |= CakeMouseButton.Right;
            if ((e.Button & MouseButtons.Middle) == MouseButtons.Middle) btn |= CakeMouseButton.Middle;

            return new ElementMouseEventArgs(screenLoc, worldLoc, btn, e.Clicks, e.Delta);
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing && _scene != null)
            {
                _scene.InvalidateRequested -= OnSceneInvalidateRequested;
            }
            base.Dispose(disposing);
        }
    }
}