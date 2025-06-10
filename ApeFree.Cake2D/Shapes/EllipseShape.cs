using System;
using System.Drawing;

namespace ApeFree.Cake2D.Shapes
{
    /// <summary>椭圆形</summary>
    public class EllipseShape : RectangleShape
    {
        public override ShapeType ShapeType => ShapeType.Ellipse;

        public EllipseShape(float x, float y, float width, float height) : base(x, y, width, height) { }

        public EllipseShape(PointF location, float width, float height) : base(location, width, height) { }

        public PointF Location { get => Points[0]; set => Points[0] = value; }

        /// <summary>
        /// 椭圆中心点
        /// </summary>
        public PointF Center => new PointF(
            Location.X + Width / 2,
            Location.Y + Height / 2);

        /// <summary>
        /// 计算椭圆面积（πab）
        /// </summary>
        public override double CalculateArea()
        {
            // 椭圆面积公式：π * 半长轴 * 半短轴
            return Math.PI * (Width / 2) * (Height / 2);
        }

        /// <summary>
        /// 计算椭圆周长（近似公式）
        /// </summary>
        public override double CalculatePerimeter()
        {
            // 使用Ramanujan近似公式计算周长
            double a = Math.Max(Width, Height) / 2; // 半长轴
            double b = Math.Min(Width, Height) / 2; // 半短轴
            double h = Math.Pow((a - b) / (a + b), 2);

            return Math.PI * (a + b) * (1 + (3 * h) / (10 + Math.Sqrt(4 - 3 * h)));
        }

        /// <summary>
        /// 判断点是否在椭圆内部（包含边界）
        /// </summary>
        public override bool Contains(PointF point)
        {
            // 转换为以椭圆中心为原点的坐标系
            float dx = point.X - Center.X;
            float dy = point.Y - Center.Y;

            // 椭圆标准方程：(x/a)^2 + (y/b)^2 <= 1
            float a = Width / 2;  // 半长轴
            float b = Height / 2; // 半短轴

            // 处理宽度或高度为0的退化情况
            if (a <= 0 || b <= 0)
                return false;

            // 计算归一化距离（避免浮点精度问题）
            float normalized = (dx * dx) / (a * a) + (dy * dy) / (b * b);
            return normalized <= 1.0f;
        }
    }
}
