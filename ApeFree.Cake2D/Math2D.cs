using ApeFree.Cake2D.Shapes;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ApeFree.Cake2D
{
    /// <summary>
    /// 绘图数学库
    /// </summary>
    public static class Math2D
    {
        /// <summary>
        /// 计算圆上点的坐标
        /// </summary>
        /// <param name="centrePoint">圆心</param>
        /// <param name="radius">半径</param>
        /// <param name="angle">角度</param>
        /// <returns></returns>
        public static PointF CalculatePointOnCircle(PointF centrePoint, float radius, float angle)
        {
            var x = centrePoint.X + radius * Math.Cos(angle * Math.PI / 180);
            var y = centrePoint.Y + radius * Math.Sin(angle * Math.PI / 180);
            return new PointF((float)x, (float)y);
        }

        /// <summary>
        /// 通过两点坐标计算距离
        /// </summary>
        /// <param name="p1">坐标1</param>
        /// <param name="p2">坐标2</param>
        /// <returns></returns>
        public static double CalculateLengthFromTwoPoints(PointF p1, PointF p2)
        {
            double length = Math.Sqrt(Math.Abs(p1.X - p2.X) * Math.Abs(p1.X - p2.X) + Math.Abs(p1.Y - p2.Y) * Math.Abs(p1.Y - p2.Y));
            return length;
        }

        /// <summary>
        /// 计算两点之间的角度
        /// </summary>
        /// <param name="p1"></param>
        /// <param name="p2"></param>
        /// <returns></returns>
        public static double CalculateAngleFromTwoPoints(PointF p1, PointF p2)
        {
            double angle = Math.Atan2(p2.Y - p1.Y, p2.X - p1.X) * 180 / Math.PI;
            return angle > 0 ? angle : 360 + angle;
        }

        /// <summary>
        /// 点位环绕
        /// </summary>
        /// <param name="centrePoint">中心点</param>
        /// <param name="satellitePoint">卫星点</param>
        /// <param name="rotationAngle">旋转半径</param>
        /// <returns></returns>
        public static PointF PointAround(PointF centrePoint, PointF satellitePoint, float rotationAngle)
        {
            var radius = CalculateLengthFromTwoPoints(centrePoint, satellitePoint);
            var angle = CalculateAngleFromTwoPoints(centrePoint, satellitePoint);
            var newPoint = CalculatePointOnCircle(centrePoint, (float)radius, (float)(angle + rotationAngle));
            return newPoint;
        }

        /// <summary>
        /// 线段伸缩
        /// 在固定点和活动点的直线上，计算出与固定点指定距离的新点位
        /// </summary>
        /// <param name="fixedPoint">固定点</param>
        /// <param name="activePoint">移动点</param>
        /// <param name="distance">距离</param>
        /// <returns></returns>
        public static PointF LineScale(PointF fixedPoint, PointF activePoint, float distance)
        {
            var angle = CalculateAngleFromTwoPoints(fixedPoint, activePoint);
            var newPoint = CalculatePointOnCircle(fixedPoint, distance, (float)angle);
            return newPoint;
        }

        /// <summary>
        /// 一个点是否位于一个由一组点构成的多边形内部
        /// </summary>
        /// <param name="polygon">按序可构成多边形的一组点</param>
        /// <param name="point">待测点</param>
        /// <returns></returns>
        public static bool IsPointInPolygon(PointF[] polygon, PointF point)
        {
            int count = 0;
            int n = polygon.Length;

            for (int i = 0, j = n - 1; i < n; j = i++)
            {
                if (((polygon[i].Y > point.Y) != (polygon[j].Y > point.Y)) &&
                    (point.X < (polygon[j].X - polygon[i].X) * (point.Y - polygon[i].Y) / (polygon[j].Y - polygon[i].Y) + polygon[i].X))
                {
                    count++;
                }
            }

            return count % 2 == 1;
        }

        /// <summary>
        /// 计算一组点的正外接矩形
        /// </summary>
        /// <param name="points"></param>
        /// <returns></returns>
        public static RectangleShape GetBounds(PointF[] points)
        {
            float minx = float.MaxValue, miny = float.MaxValue, maxx = float.MinValue, maxy = float.MinValue;

            foreach (var p in points)
            {
                if (p.X < minx)
                {
                    minx = p.X;
                }
                else if (p.X > maxx)
                {
                    maxx = p.X;
                }

                if (p.Y < miny)
                {
                    miny = p.Y;
                }
                else if (p.Y > maxy)
                {
                    maxy = p.Y;
                }
            }

            return new RectangleShape(new PointF(minx, miny), maxx - minx, maxy - miny);
        }
    }
}
