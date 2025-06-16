using System.Drawing;
using System.Runtime.InteropServices;

namespace ApeFree.Cake2D.Shapes
{
    /// <summary>矩形接口</summary>
    public class RectangleShape : PlaneShape
    {
        public RectangleShape(float x, float y, float width, float height) : this(new PointF(x, y), width, height) { }

        public RectangleShape(PointF location, float width, float height) : base(GetPoints(location, width, height))
        {
            Width = width;
            Height = height;
        }

        private static PointF[] GetPoints(PointF location, float width, float height)
        {
            var x1 = location.X;
            var y1 = location.Y;
            var x2 = location.X + width;
            var y2 = location.Y + height;
            return
            [
                location,
                new PointF(x2, y1),
                new PointF(x1, y2),
                new PointF(x2, y2)
            ];
        }

        public override ShapeType ShapeType => ShapeType.Rectangle;

        public PointF Location { get => Points[0]; set => Points[0] = value; }

        /// <summary>矩形宽度</summary>
        public float Width
        {
            get => Points[1].X - Points[0].X;
            set
            {
                var points = Points;
                points[1].X = points[0].X + value;
                points[3].X = points[1].X;
            }
        }

        /// <summary>矩形高度</summary>
        public float Height
        {
            get => Points[2].Y - Points[0].Y;
            set
            {
                Points[2].Y = Points[0].Y + value;
                Points[3].Y = Points[0].Y + value;
            }
        }

        public float Left
        {
            get => Points[0].X;
            set
            {
                float delta = value - Left;
                Offset(delta, 0);
            }
        }

        public float Top
        {
            get => Points[0].Y;
            set
            {
                float delta = value - Top;
                Offset(0, delta);
            }
        }

        public override double CalculateArea()
        {
            return Width * Height;
        }

        public override double CalculatePerimeter()
        {
            return (Width + Height) * 2;
        }

        public override bool Contains(PointF point)
        {
            var rect = new RectangleF(Location.X, Location.Y, Width, Height);
            return rect.Contains(point);
        }
    }
}
