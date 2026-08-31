using System;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;
using ApeFree.Cake2D.Events;
using ApeFree.Cake2D.Rendering;
using ApeFree.Cake2D.Scene;
using SkiaSharp;
using SkiaSharp.Views.Desktop;

namespace ApeFree.Cake2D.Skia.Controls
{
    /// <summary>
    /// 基于 SkiaSharp 的 Cake2D 二维视口控件（Windows Forms SKControl）。
    /// </summary>
    public class Cake2DSkiaControl : SKControl
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

        public Cake2DSkiaControl()
        {
            DoubleBuffered = true;
            Scene = new Scene2D();
        }

        private void OnSceneInvalidateRequested(object sender, EventArgs e)
        {
            if (IsHandleCreated && !IsDisposed)
            {
                Invalidate();
            }
        }

        protected override void OnPaintSurface(SKPaintSurfaceEventArgs e)
        {
            base.OnPaintSurface(e);

            if (_scene == null)
            {
                e.Surface.Canvas.Clear(SkiaRenderCanvas.ToSKColor(BackColor));
                return;
            }

            SkiaRenderCanvas canvas = new SkiaRenderCanvas(e.Surface.Canvas);
            _engine.Render(_scene, canvas);

            // 绘制当前状态机前景
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