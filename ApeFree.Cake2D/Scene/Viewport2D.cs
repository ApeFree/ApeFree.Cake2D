using System;
using System.Drawing;
using ApeFree.Cake2D.Math;

namespace ApeFree.Cake2D.Scene
{
    /// <summary>
    /// 二维无限画布视口管理器。
    /// 统一管理世界坐标系与屏幕像素坐标系的相互映射转换、缩放和平移。
    /// </summary>
    public class Viewport2D
    {
        private float _viewWidth = 1000f;
        private PointF _viewCenter = PointF.Empty;
        private int _width = 800;
        private int _height = 600;

        /// <summary>视口宽度（像素）</summary>
        public int Width
        {
            get => _width;
            set => Resize(value, _height);
        }

        /// <summary>视口高度（像素）</summary>
        public int Height
        {
            get => _height;
            set => Resize(_width, value);
        }

        /// <summary>当前视野中心点（世界坐标系）</summary>
        public PointF ViewCenter
        {
            get => _viewCenter;
            set => SetView(value, _viewWidth);
        }

        /// <summary>当前视野宽度（世界坐标系单位）</summary>
        public float ViewWidth
        {
            get => _viewWidth;
            set => SetView(_viewCenter, value);
        }

        /// <summary>
        /// Y轴方向。
        /// true: 数学坐标系 (Y向上增长，例如 CAD/地图/数学分析)；
        /// false: 屏幕坐标系 (Y向下增长，例如 UI界面/文档/流程图)。
        /// </summary>
        public bool YAxisUpwards { get; set; } = false;

        /// <summary>绘制内边距</summary>
        public int PaddingLeft { get; set; } = 0;
        public int PaddingTop { get; set; } = 0;
        public int PaddingRight { get; set; } = 0;
        public int PaddingBottom { get; set; } = 0;

        /// <summary>有效绘制区域矩形（屏幕像素坐标）</summary>
        public RectangleF DrawRectangle => new RectangleF(
            PaddingLeft,
            PaddingTop,
            System.Math.Max(1, Width - PaddingLeft - PaddingRight),
            System.Math.Max(1, Height - PaddingTop - PaddingBottom)
        );

        /// <summary>比例尺：每个世界单位对应的屏幕像素数 (Pixels/Unit)</summary>
        public float PixelsPerUnit => DrawRectangle.Width > 0 ? DrawRectangle.Width / _viewWidth : 1f;

        /// <summary>比例尺：每个屏幕像素对应的世界单位数 (Units/Pixel)</summary>
        public float UnitPerPixel => DrawRectangle.Width > 0 ? _viewWidth / DrawRectangle.Width : 1f;

        /// <summary>当前视野高度（世界坐标系单位）</summary>
        public float ViewHeight => DrawRectangle.Height * UnitPerPixel;

        /// <summary>视口几何发生变化时触发的事件</summary>
        public event EventHandler ViewChanged;

        /// <summary>更新视口像素尺寸</summary>
        public void Resize(int width, int height)
        {
            int newW = System.Math.Max(1, width);
            int newH = System.Math.Max(1, height);
            if (_width != newW || _height != newH)
            {
                _width = newW;
                _height = newH;
                OnViewChanged();
            }
        }

        /// <summary>设置视口中心与视野宽度</summary>
        public void SetView(PointF center, float width)
        {
            float targetW = System.Math.Max(0.0001f, width);
            if (_viewCenter != center || System.Math.Abs(_viewWidth - targetW) > 1e-6f)
            {
                _viewCenter = center;
                _viewWidth = targetW;
                OnViewChanged();
            }
        }

        /// <summary>平移视口（按屏幕像素位移量）</summary>
        public void Pan(float deltaScreenX, float deltaScreenY)
        {
            float unitPerPixel = UnitPerPixel;
            float worldDx = -deltaScreenX * unitPerPixel;
            float worldDy = YAxisUpwards ? deltaScreenY * unitPerPixel : -deltaScreenY * unitPerPixel;

            SetView(new PointF(_viewCenter.X + worldDx, _viewCenter.Y + worldDy), _viewWidth);
        }

        /// <summary>以屏幕上的某个点为固定锚点进行缩放</summary>
        public void ZoomAroundScreenPoint(PointF screenPoint, float factor)
        {
            if (factor <= 0f || System.Math.Abs(factor - 1f) < 1e-5f) return;

            PointF anchorWorld = ScreenToWorld(screenPoint);
            float newViewWidth = _viewWidth / factor;
            newViewWidth = System.Math.Max(0.0001f, newViewWidth);

            var rect = DrawRectangle;
            float screenCenterX = rect.Left + rect.Width * 0.5f;
            float screenCenterY = rect.Top + rect.Height * 0.5f;
            float dx = screenPoint.X - screenCenterX;
            float dy = screenPoint.Y - screenCenterY;

            float newUnitPerPixel = newViewWidth / rect.Width;
            float newCenterX = anchorWorld.X - dx * newUnitPerPixel;
            float newCenterY = YAxisUpwards ? anchorWorld.Y + dy * newUnitPerPixel : anchorWorld.Y - dy * newUnitPerPixel;

            SetView(new PointF(newCenterX, newCenterY), newViewWidth);
        }

        /// <summary>自动调整视野至包围指定世界区域 (Zoom to Fit)</summary>
        public void ZoomToFit(RectangleF worldBounds, float marginRatio = 0.1f)
        {
            if (worldBounds.IsEmpty) return;

            float boundW = worldBounds.Width;
            float boundH = worldBounds.Height;
            if (boundW <= 0f) boundW = 100f;
            if (boundH <= 0f) boundH = 100f;

            float centerX = worldBounds.Left + boundW * 0.5f;
            float centerY = worldBounds.Top + boundH * 0.5f;

            var rect = DrawRectangle;
            float aspect = rect.Width / rect.Height;
            float boundAspect = boundW / boundH;

            float targetViewWidth = boundAspect > aspect ? boundW : boundH * aspect;
            targetViewWidth *= (1f + marginRatio * 2f);

            SetView(new PointF(centerX, centerY), targetViewWidth);
        }

        /// <summary>屏幕像素坐标 -> 世界坐标</summary>
        public PointF ScreenToWorld(PointF screenPt)
        {
            var rect = DrawRectangle;
            if (rect.Width <= 0) return PointF.Empty;

            float screenCenterX = rect.Left + rect.Width * 0.5f;
            float screenCenterY = rect.Top + rect.Height * 0.5f;
            float unitPerPixel = _viewWidth / rect.Width;

            float worldX = _viewCenter.X + (screenPt.X - screenCenterX) * unitPerPixel;
            float worldY = YAxisUpwards
                ? _viewCenter.Y + (screenCenterY - screenPt.Y) * unitPerPixel
                : _viewCenter.Y + (screenPt.Y - screenCenterY) * unitPerPixel;

            return new PointF(worldX, worldY);
        }

        /// <summary>世界坐标 -> 屏幕像素坐标</summary>
        public PointF WorldToScreen(PointF worldPt)
        {
            var rect = DrawRectangle;
            if (rect.Width <= 0) return PointF.Empty;

            float screenCenterX = rect.Left + rect.Width * 0.5f;
            float screenCenterY = rect.Top + rect.Height * 0.5f;
            float pixelsPerUnit = rect.Width / _viewWidth;

            float screenX = screenCenterX + (worldPt.X - _viewCenter.X) * pixelsPerUnit;
            float screenY = YAxisUpwards
                ? screenCenterY - (worldPt.Y - _viewCenter.Y) * pixelsPerUnit
                : screenCenterY + (worldPt.Y - _viewCenter.Y) * pixelsPerUnit;

            return new PointF(screenX, screenY);
        }

        /// <summary>获取当前屏幕视口对应的世界坐标矩形边界 (AABB)</summary>
        public RectangleF GetCurrentWorldViewRect()
        {
            var rect = DrawRectangle;
            PointF p1 = ScreenToWorld(new PointF(rect.Left, rect.Top));
            PointF p2 = ScreenToWorld(new PointF(rect.Right, rect.Bottom));

            float minX = System.Math.Min(p1.X, p2.X);
            float maxX = System.Math.Max(p1.X, p2.X);
            float minY = System.Math.Min(p1.Y, p2.Y);
            float maxY = System.Math.Max(p1.Y, p2.Y);

            return new RectangleF(minX, minY, maxX - minX, maxY - minY);
        }

        protected virtual void OnViewChanged()
        {
            ViewChanged?.Invoke(this, EventArgs.Empty);
        }
    }
}