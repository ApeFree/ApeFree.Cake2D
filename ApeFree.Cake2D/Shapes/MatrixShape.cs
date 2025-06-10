using System;
using System.Drawing;
using System.Linq;

namespace ApeFree.Cake2D.Shapes
{
    /// <summary>
    /// 点阵图形
    /// </summary>
    public class MatrixShape : Shape
    {
        public MatrixShape(PointF[] points) : base(points)
        {
        }

        /// <summary>
        /// 点图形的半径
        /// </summary>
        public float Radius { get; set; }

        /// <summary>
        /// 点图像类型
        /// </summary>
        public PointType PointType { get; set; } = PointType.Circle;

        public override ShapeType ShapeType => ShapeType.Matrix;

        /// <inheritdoc/>
        public override bool Contains(PointF point)
        {
            Func<PointF, bool> func;
            switch (PointType)
            {
                case PointType.Circle:
                    func = new Func<PointF, bool>(p => (point.X - p.X) * (point.X - p.X) + (point.Y - p.Y) * (point.Y - p.Y) <= Radius * Radius);
                    break;
                case PointType.Square:
                    func = new Func<PointF, bool>(p => new RectangleF(p, new SizeF(Radius, Radius)).Contains(point));
                    break;
                default:
                    return false;
            }

            foreach (var p in Points)
            {
                if (func.Invoke(p))
                {
                    return true;
                }
            }
            return false;
        }
    }
}
