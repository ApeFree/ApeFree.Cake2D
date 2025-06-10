using ApeFree.Cake2D;
using System.Collections.Generic;
using System.Drawing;

namespace ApeFree.Cake2D.Shapes
{
    public class LineShape : Shape
    {
        public override ShapeType ShapeType => ShapeType.Line;

        /// <summary>
        /// 构造线图形
        /// </summary>
        /// <param name="x1">起始点X坐标</param>
        /// <param name="y1">起始点Y坐标</param>
        /// <param name="x2">结束点X坐标</param>
        /// <param name="y2">结束点Y坐标</param>
        public LineShape(int x1, int y1, int x2, int y2) : this(new Point(x1, y1), new Point(x2, y2)) { }

        /// <summary>
        /// 构造线图形
        /// </summary>
        /// <param name="startPoint">起始点</param>
        /// <param name="endPoint">结束点</param>
        public LineShape(PointF startPoint, PointF endPoint) : base([startPoint, endPoint])
        {
            StartPoint = startPoint;
            EndPoint = endPoint;
        }

        /// <summary>
        /// 构造线图形
        /// </summary>
        /// <param name="startPoint">起始点</param>
        /// <param name="length">长度</param>
        /// <param name="angle">角度</param>
        public LineShape(Point startPoint, double length, float angle) : this(startPoint, Math2D.CalculatePointOnCircle(startPoint, (float)length, angle))
        {
            StartPoint = startPoint;
            EndPoint = Math2D.CalculatePointOnCircle(startPoint, (float)length, angle);
        }

        /// <summary>
        /// 线段的起始点
        /// </summary>
        public PointF StartPoint { get; set; }

        /// <summary>
        /// 线段的结束点
        /// </summary>
        public PointF EndPoint { get; set; }

        /// <summary>
        /// 线段的宽度
        /// </summary>
        public float Width { get; set; } = 1.0f;

        /// <summary>
        /// 线段的中心点
        /// </summary>
        public PointF CentrePoint => new PointF((StartPoint.X + EndPoint.X) / 2, (StartPoint.Y + EndPoint.Y) / 2);

        /// <summary>
        /// 线长
        /// </summary>
        public double Length => Math2D.CalculateLengthFromTwoPoints(StartPoint, EndPoint);

        /// <inheritdoc/>
        public override bool Contains(PointF point)
        {
            if (StartPoint == point || EndPoint == point)
            {
                return true;
            }

            return false;

            // 通过两点坐标和线宽计算出线段的矩形范围
            // 判断点是否在矩形范围内

        }
    }
}
