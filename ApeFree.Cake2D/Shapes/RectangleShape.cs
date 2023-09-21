using System;
using System.Drawing;

namespace ApeFree.Cake2D.Shapes
{
    public class RectangleShape : PolygonShape, IRectangle
    {
        private float _width;
        private float _height;
        private float _angle;

        public RectangleShape(PointF location, float width, float height) : base(new[] { location, new PointF(location.X + width, location.Y), new PointF(location.X, location.Y + height), new PointF(location.X + width, location.Y + height) })
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
                if (_width != value)
                {
                    _width = value;
                    UpdatePoints();
                }
            }
        }

        public float Height
        {
            get { return _height; }
            set
            {
                if (_height != value)
                {
                    _height = value;
                    UpdatePoints();
                }
            }
        }

        public float Angle
        {
            get { return _angle; }
            set
            {
                if (_angle != value)
                {
                    _angle = value;
                    UpdatePoints();
                }
            }
        }

        private void UpdatePoints()
        {
            // 更新矩形的各个顶点坐标
            Points[1] = Math2D.CalculatePointOnCircle(Points[0], Width, Angle);
            Points[2] = Math2D.CalculatePointOnCircle(Points[0], Height, Angle - 90);
            Points[3] = Math2D.CalculatePointOnCircle(Points[1], Height, Angle - 90);
        }

        public float Left
        {
            get { return Points[0].X; }
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
