using System.Drawing;

namespace ApeFree.Cake2D.Shapes
{
    /// <summary>
    /// 点图形
    /// </summary>
    public class PointShape : IShape
    {
        /// <summary>
        /// 点的坐标
        /// </summary>
        public PointF Location { get; set; }

        /// <summary>
        /// 点图形的半径
        /// </summary>
        public float Radius { get; set; }

        /// <summary>
        /// 点图像类型
        /// </summary>
        public PointShapeType ShapeType { get; set; }

        /// <inheritdoc/>
        public PointF[] Points => new PointF[] { Location };


        public PointShape(PointF location)
        {
            Location = location;
        }

        /// <inheritdoc/>
        public bool Contains(PointF point)
        {
            // 根据点图像的类型区分判断方法
            switch (ShapeType)
            {
                case PointShapeType.Circle:
                    {
                        return (point.X - Location.X) * (point.X - Location.X) +
                                (point.Y - Location.Y) * (point.Y - Location.Y) <= Radius * Radius;
                    }
                case PointShapeType.Square:
                    {
                        return new RectangleF(Location, new SizeF(Radius, Radius)).Contains(point);
                    }
            }
            return false;
        }

        /// <inheritdoc/>
        public RectangleShape GetBounds()
        {
            return new RectangleShape(Location, Radius, Radius);
        }

        /// <inheritdoc/>
        public void Offset(float distanceX, float distanceY)
        {
            Location = Location.Add(distanceX, distanceY);
        }

        /// <inheritdoc/>
        public void Rotate(PointF centralPoint, float angle)
        {
            Location = Math2D.PointAround(centralPoint, Location, angle);
        }

        /// <inheritdoc/>
        public void Scale(float scaling) { }
    }
}
