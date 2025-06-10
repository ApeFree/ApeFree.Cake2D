using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;

namespace ApeFree.Cake2D.Shapes
{
    /// <summary>多边形基类</summary>
    public class PolygonShape : PlaneShape
    {
        public override ShapeType ShapeType => ShapeType.Polygon;

        public PolygonShape(PointF[] points) : base(points) { }

        /// <inheritdoc/>
        public override bool Contains(PointF point)
        {
            var _points = Points.ToArray();

            // 判断指定点是否在多边形内部
            bool result = false;
            int j = _points.Length - 1;
            for (int i = 0; i < _points.Length; i++)
            {
                if ((_points[i].Y < point.Y && _points[j].Y >= point.Y || _points[j].Y < point.Y && _points[i].Y >= point.Y) &&
                    (_points[i].X + (point.Y - _points[i].Y) / (_points[j].Y - _points[i].Y) * (_points[j].X - _points[i].X) < point.X))
                {
                    result = !result;
                }
                j = i;
            }
            return result;
        }

        /// <summary>
        /// 计算多边形周长
        /// </summary>
        /// <returns>多边形周长</returns>
        /// <exception cref="InvalidOperationException">当多边形顶点数小于3时抛出</exception>
        public override double CalculatePerimeter()
        {
            PointF[] points = Points;

            // 验证基础条件：至少3个顶点才能构成多边形
            if (points.Length < 3)
                throw new InvalidOperationException("多边形至少需要3个顶点");

            double perimeter = 0;

            // 遍历所有相邻顶点对（包含首尾闭合）
            for (int i = 0; i < points.Length; i++)
            {
                // 当前顶点
                PointF current = points[i];
                // 下一个顶点（当i是最后一个时取第一个顶点）
                PointF next = points[(i + 1) % points.Length];

                // 使用勾股定理计算两点间距离
                double dx = next.X - current.X;
                double dy = next.Y - current.Y;
                perimeter += Math.Sqrt(dx * dx + dy * dy);
            }

            return perimeter;
        }

        /// <summary>
        /// 使用鞋带定理计算多边形面积
        /// </summary>
        /// <returns>多边形面积（始终返回正值）</returns>
        /// <exception cref="InvalidOperationException">当多边形顶点数小于3时抛出</exception>
        public override double CalculateArea()
        {
            PointF[] points = Points;

            // 验证基础条件
            if (points.Length < 3)
                throw new InvalidOperationException("多边形至少需要3个顶点");

            double area = 0;

            // 应用鞋带定理公式：
            // area = ½ |Σ(x_i * y_{i+1} - x_{i+1} * y_i)|
            for (int i = 0; i < points.Length; i++)
            {
                // 当前顶点
                PointF current = points[i];
                // 下一个顶点（当i是最后一个时取第一个顶点）
                PointF next = points[(i + 1) % points.Length];

                // 累加行列式计算项
                area += (current.X * next.Y) - (next.X * current.Y);
            }

            // 取绝对值并除以2得到最终面积
            return Math.Abs(area) / 2.0;
        }
    }
}
