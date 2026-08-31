using System.Drawing;
using ApeFree.Cake2D.Events;

namespace ApeFree.Cake2D.Interaction
{
    /// <summary>
    /// 默认平移与滚轮缩放状态。
    /// </summary>
    public class DefaultPanZoomState : ViewportInteractionState
    {
        private bool _isPanning = false;
        private PointF _lastMousePt;

        public float ZoomFactor { get; set; } = 1.15f;
        public bool EnablePan { get; set; } = true;
        public bool EnableZoom { get; set; } = true;

        public override void OnMouseDown(ElementMouseEventArgs e)
        {
            if (!EnablePan) return;

            if (e.Button == CakeMouseButton.Left || e.Button == CakeMouseButton.Middle)
            {
                _isPanning = true;
                _lastMousePt = e.ScreenLocation;
            }
        }

        public override void OnMouseMove(ElementMouseEventArgs e)
        {
            if (_isPanning && EnablePan)
            {
                float dx = e.ScreenLocation.X - _lastMousePt.X;
                float dy = e.ScreenLocation.Y - _lastMousePt.Y;

                if (System.Math.Abs(dx) > 0.01f || System.Math.Abs(dy) > 0.01f)
                {
                    Viewport.Pan(dx, dy);
                    _lastMousePt = e.ScreenLocation;
                }
            }
        }

        public override void OnMouseUp(ElementMouseEventArgs e)
        {
            _isPanning = false;
        }

        public override void OnMouseWheel(ElementMouseEventArgs e)
        {
            if (!EnableZoom) return;

            float factor = e.Delta > 0 ? ZoomFactor : 1f / ZoomFactor;
            Viewport.ZoomAroundScreenPoint(e.ScreenLocation, factor);
        }

        public override void OnLeave()
        {
            _isPanning = false;
        }
    }
}