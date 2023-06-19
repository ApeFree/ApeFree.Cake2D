using ApeFree.Cake2D;
using System.Collections.Generic;
using System.Drawing;

namespace ApeFree.Cake2D.Shapes
{
    public class LineShape : IShape
    {
        public PointF StartPoint { get; set; }

        public PointF EndPoint { get; set; }

        private PointF CentrePoint => new PointF((StartPoint.X + EndPoint.X) / 2, (StartPoint.Y + EndPoint.Y) / 2);

        /// <inheritdoc/>
        public IEnumerable<PointF> Points => new PointF[] { StartPoint, EndPoint };


        /// <summary>
        /// 线长
        /// </summary>
        public double Length => GdiMath.CalculateLengthFromTwoPoints(StartPoint, EndPoint);

        /// <inheritdoc/>
        public void Rotate(PointF centralPoint, float angle)
        {
            var cp = CentrePoint;
            StartPoint = GdiMath.PointAround(cp, StartPoint, angle);
            EndPoint = GdiMath.PointAround(cp, EndPoint, angle);
        }

        /// <inheritdoc/>
        public void Scale(float scaling)
        {
            // 计算中心点
            PointF center = CentrePoint;

            // 计算起点和中心点的距离
            float startDistance = (float)GdiMath.CalculateLengthFromTwoPoints(StartPoint, center);

            // 根据缩放比例计算新的距离
            float newStartDistance = startDistance * scaling;

            // 计算新的起点和终点
            float angle = (float)GdiMath.CalculateAngleFromTwoPoints(center, StartPoint);

            StartPoint = GdiMath.CalculatePointOnCircle(new PointF(CentrePoint.X, CentrePoint.Y), newStartDistance, angle);
            EndPoint = GdiMath.CalculatePointOnCircle(new PointF(CentrePoint.X, CentrePoint.Y), newStartDistance, 360 - angle);
        }

        /// <inheritdoc/>
        public void Offset(float distanceX, float distanceY)
        {
            StartPoint = new PointF(StartPoint.X + distanceX, StartPoint.Y + distanceY);
            EndPoint = new PointF(EndPoint.X + distanceX, EndPoint.Y + distanceY);
        }

        /// <inheritdoc/>
        public bool Contains(PointF p)
        {
            if (StartPoint == p || EndPoint == p)
            {
                return true;
            }

            // 判断点是否在直线上
            float a = EndPoint.Y - StartPoint.Y;
            float b = StartPoint.X - EndPoint.X;
            float c = EndPoint.X * StartPoint.Y - StartPoint.X * EndPoint.Y;
            return a * p.X + b * p.Y + c == 0;
        }

        /// <inheritdoc/>
        public RectangleShape GetBounds()
        {
            return new RectangleShape(StartPoint, (EndPoint.X - StartPoint.X), EndPoint.Y - StartPoint.Y);
        }

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
        public LineShape(PointF startPoint, PointF endPoint)
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
        public LineShape(Point startPoint, double length, float angle)
        {
            StartPoint = startPoint;
            EndPoint = GdiMath.CalculatePointOnCircle(startPoint, (float)length, angle);
        }
    }
}
