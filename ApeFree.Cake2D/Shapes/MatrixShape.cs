using System;
using System.Drawing;
using System.Linq;

namespace ApeFree.Cake2D.Shapes
{
    /// <summary>
    /// 点阵图形
    /// </summary>
    public class MatrixShape : IShape
    {
        public MatrixShape(PointF[] points)
        {
            Points = points;
        }

        /// <summary>
        /// 点图形的半径
        /// </summary>
        public float Radius { get; set; }

        /// <summary>
        /// 点图像类型
        /// </summary>
        public PointShapeType ShapeType { get; set; }

        /// <inheritdoc/>
        public PointF[] Points { get; set; }

        /// <inheritdoc/>
        public bool Contains(PointF point)
        {
            Func<PointF, bool> func;
            switch (ShapeType)
            {
                case PointShapeType.Circle:
                    func = new Func<PointF, bool>(p => (point.X - p.X) * (point.X - p.X) + (point.Y - p.Y) * (point.Y - p.Y) <= Radius * Radius);
                    break;
                case PointShapeType.Square:
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

        /// <inheritdoc/>
        public RectangleShape GetBounds()
        {
            // 获取多边形的外接矩形
            float left = Points.Min(p => p.X);
            float top = Points.Min(p => p.Y);
            float right = Points.Max(p => p.X);
            float bottom = Points.Max(p => p.Y);
            return new RectangleShape(left, top, right - left, bottom - top);
        }

        /// <inheritdoc/>
        public void Offset(float distanceX, float distanceY)
        {
            // 平移多边形的各个顶点坐标
            for (int i = 0; i < Points.Length; i++)
            {
                Points[i] = Points[i].Add(distanceX, distanceY);
            }
        }

        /// <inheritdoc/>
        public void Rotate(PointF centralPoint, float angle)
        {
            // 将多边形的各个顶点坐标绕中心点旋转指定角度
            for (int i = 0; i < Points.Length; i++)
            {
                Points[i] = Math2D.PointAround(centralPoint, Points[i], angle);
            }
        }

        /// <inheritdoc/>
        public void Scale(float scaling)
        {
            // 缩放多边形的各个顶点坐标
            for (int i = 0; i < Points.Length; i++)
            {
                Points[i] = new Point((int)(Points[i].X * scaling), (int)(Points[i].Y * scaling));
            }
        }

        /// <summary>
        /// 重心点
        /// </summary>
        public virtual PointF Centroid
        {
            get
            {
                var _points = Points;

                // 计算多边形的重心
                double cx = 0;
                double cy = 0;
                double area = 0;
                for (int i = 0; i < _points.Length; i++)
                {
                    int j = (i + 1) % _points.Length;
                    double temp = _points[i].X * _points[j].Y - _points[j].X * _points[i].Y;
                    area += temp;
                    cx += (double)(_points[i].X + _points[j].X) * temp;
                    cy += (double)(_points[i].Y + _points[j].Y) * temp;
                }
                area /= 2;
                cx /= 6 * area;
                cy /= 6 * area;
                return new Point((int)cx, (int)cy);
            }
        }
    }
}
