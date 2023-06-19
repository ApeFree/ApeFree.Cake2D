using System;
using System.Drawing;

namespace ApeFree.Cake2D.Shapes
{
    public class RectangleShape : BasePolygonShape, IRectangle
    {
        private float _width;
        private float _height;
        private float _angle;

        public RectangleShape(PointF location, float width, float height) : base(new[] { location, new PointF(location.X + width, location.Y), new PointF(location.X + width, location.Y + height), new PointF(location.X, location.Y + height) })
        {
            _width = width;
            _height = height;
        }

        public RectangleShape(float left, float top, float width, float height) : this(new PointF(left, top), width, height) { }

        public float Width
        {
            get { return _width; }
            set
            {
                _width = value;
                UpdatePoints();
            }
        }

        public float Height
        {
            get { return _height; }
            set
            {
                _height = value;
                UpdatePoints();
            }
        }

        public float Angle
        {
            get { return _angle; }
            set
            {
                _angle = value;
                UpdatePoints();
            }
        }

        private void UpdatePoints()
        {
            // 更新矩形的各个顶点坐标
            PointF center = new PointF(Left + Width / 2, Top + Height / 2);
            double sin = Math.Sin(Angle);
            double cos = Math.Cos(Angle);
            PointF[] points = new PointF[4];
            points[0] = new PointF(center.X - (int)(0.5 * Width * cos + 0.5 * Height * sin), center.Y - (int)(0.5 * Height * cos - 0.5 * Width * sin));
            points[1] = new PointF(center.X + (int)(0.5 * Width * cos - 0.5 * Height * sin), center.Y - (int)(0.5 * Height * cos + 0.5 * Width * sin));
            points[2] = new PointF(center.X + (int)(0.5 * Width * cos + 0.5 * Height * sin), center.Y + (int)(0.5 * Height * cos - 0.5 * Width * sin));
            points[3] = new PointF(center.X - (int)(0.5 * Width * cos - 0.5 * Height * sin), center.Y + (int)(0.5 * Height * cos + 0.5 * Width * sin));
            base._points.Clear();
            base._points.AddRange(points);
        }

        public float Left
        {
            get { return base.GetBounds().Left; }
            set
            {
                float delta = value - Left;
                Offset(delta, 0);
            }
        }

        public float Top
        {
            get { return base.GetBounds().Top; }
            set
            {
                float delta = value - Top;
                Offset(0, delta);
            }
        }
    }
}
