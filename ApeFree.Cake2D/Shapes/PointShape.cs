using System.Drawing;

namespace ApeFree.Cake2D.Shapes
{
    /// <summary>
    /// 点图形
    /// </summary>
    public class PointShape : Shape
    {
        public override ShapeType ShapeType => ShapeType.Point;

        /// <summary>
        /// 点的坐标
        /// </summary>
        public PointF Location { get => Points[0]; set => Points[0] = value; }

        /// <summary>
        /// 点图形的半径
        /// </summary>
        public float Radius { get; set; }

        /// <summary>
        /// 点图像类型
        /// </summary>
        public PointType PointType { get; set; }

        public PointShape(PointF location) : base([location])
        {
            Location = location;
        }

        /// <inheritdoc/>
        public override bool Contains(PointF point)
        {
            // 根据点图像的类型区分判断方法
            switch (PointType)
            {
                case PointType.Circle:
                    {
                        return (point.X - Location.X) * (point.X - Location.X) +
                                (point.Y - Location.Y) * (point.Y - Location.Y) <= Radius * Radius;
                    }
                case PointType.Square:
                    {
                        return new RectangleF(Location, new SizeF(Radius, Radius)).Contains(point);
                    }
            }
            return false;
        }
    }

    /// <summary>
    /// 点图像类型
    /// </summary>
    public enum PointType
    {
        /// <summary>
        /// 圆形
        /// </summary>
        Circle,

        /// <summary>
        /// 方形
        /// </summary>
        Square,
        //Diamond,
        //Triangle
    }
}
