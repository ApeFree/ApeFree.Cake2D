using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;

namespace ApeFree.Cake2D.Math
{
    /// <summary>
    /// 2D 图形几何与数学计算工具库。
    /// </summary>
    public static class Math2D
    {
        public const float DegToRad = (float)(System.Math.PI / 180.0);
        public const float RadToDeg = (float)(180.0 / System.Math.PI);

        public static float ToRadians(float degrees) => degrees * DegToRad;
        public static float ToDegrees(float radians) => radians * RadToDeg;

        /// <summary>计算圆上点的坐标（角度制，0度为X正半轴，顺时针方向）</summary>
        public static PointF CalculatePointOnCircle(PointF center, float radius, float angleDegrees)
        {
            float rad = ToRadians(angleDegrees);
            return new PointF(
                center.X + radius * (float)System.Math.Cos(rad),
                center.Y + radius * (float)System.Math.Sin(rad)
            );
        }

        /// <summary>两点之间的欧几里得距离</summary>
        public static float CalculateLengthFromTwoPoints(PointF p1, PointF p2)
        {
            float dx = p2.X - p1.X;
            float dy = p2.Y - p1.Y;
            return (float)System.Math.Sqrt(dx * dx + dy * dy);
        }

        /// <summary>计算从 p1 指向 p2 向量的角度（0~360度）</summary>
        public static float CalculateAngleFromTwoPoints(PointF p1, PointF p2)
        {
            float angle = (float)(System.Math.Atan2(p2.Y - p1.Y, p2.X - p1.X) * RadToDeg);
            return angle >= 0 ? angle : angle + 360f;
        }

        /// <summary>点位绕中心点旋转指定角度</summary>
        public static PointF PointAround(PointF center, PointF point, float rotationAngleDegrees)
        {
            float rad = ToRadians(rotationAngleDegrees);
            float cos = (float)System.Math.Cos(rad);
            float sin = (float)System.Math.Sin(rad);

            float dx = point.X - center.X;
            float dy = point.Y - center.Y;

            return new PointF(
                center.X + dx * cos - dy * sin,
                center.Y + dx * sin + dy * cos
            );
        }

        /// <summary>判断点是否在多边形内部（射线法）</summary>
        public static bool IsPointInPolygon(PointF[] polygon, PointF point)
        {
            if (polygon == null || polygon.Length < 3) return false;

            bool inside = false;
            int n = polygon.Length;
            for (int i = 0, j = n - 1; i < n; j = i++)
            {
                if (((polygon[i].Y > point.Y) != (polygon[j].Y > point.Y)) &&
                    (point.X < (polygon[j].X - polygon[i].X) * (point.Y - polygon[i].Y) / (polygon[j].Y - polygon[i].Y) + polygon[i].X))
                {
                    inside = !inside;
                }
            }
            return inside;
        }

        /// <summary>计算多边形面积</summary>
        public static float CalculatePolygonArea(PointF[] polygon)
        {
            if (polygon == null || polygon.Length < 3) return 0f;
            float area = 0f;
            int n = polygon.Length;
            for (int i = 0; i < n; i++)
            {
                int j = (i + 1) % n;
                area += polygon[i].X * polygon[j].Y - polygon[j].X * polygon[i].Y;
            }
            return System.Math.Abs(area) * 0.5f;
        }

        /// <summary>计算多边形周长</summary>
        public static float CalculatePolygonPerimeter(PointF[] polygon)
        {
            if (polygon == null || polygon.Length < 2) return 0f;
            float perimeter = 0f;
            int n = polygon.Length;
            for (int i = 0; i < n; i++)
            {
                int j = (i + 1) % n;
                perimeter += CalculateLengthFromTwoPoints(polygon[i], polygon[j]);
            }
            return perimeter;
        }

        /// <summary>计算多边形重心</summary>
        public static PointF CalculatePolygonCentroid(PointF[] polygon)
        {
            if (polygon == null || polygon.Length == 0) return PointF.Empty;
            if (polygon.Length == 1) return polygon[0];
            if (polygon.Length == 2) return new PointF((polygon[0].X + polygon[1].X) * 0.5f, (polygon[0].Y + polygon[1].Y) * 0.5f);

            float cx = 0f;
            float cy = 0f;
            float signedArea = 0f;

            int n = polygon.Length;
            for (int i = 0; i < n; i++)
            {
                int j = (i + 1) % n;
                float a = polygon[i].X * polygon[j].Y - polygon[j].X * polygon[i].Y;
                signedArea += a;
                cx += (polygon[i].X + polygon[j].X) * a;
                cy += (polygon[i].Y + polygon[j].Y) * a;
            }

            signedArea *= 0.5f;
            if (System.Math.Abs(signedArea) < 1e-6f)
            {
                // 退化情况：直接取算术平均
                return new PointF(polygon.Average(p => p.X), polygon.Average(p => p.Y));
            }

            return new PointF(cx / (6f * signedArea), cy / (6f * signedArea));
        }

        /// <summary>计算一组点集的正外接矩形 (AABB)</summary>
        public static RectangleF GetBounds(IEnumerable<PointF> points)
        {
            if (points == null) return RectangleF.Empty;

            float minX = float.MaxValue, minY = float.MaxValue;
            float maxX = float.MinValue, maxY = float.MinValue;
            bool any = false;

            foreach (var p in points)
            {
                any = true;
                if (p.X < minX) minX = p.X;
                if (p.X > maxX) maxX = p.X;
                if (p.Y < minY) minY = p.Y;
                if (p.Y > maxY) maxY = p.Y;
            }

            if (!any) return RectangleF.Empty;
            return new RectangleF(minX, minY, maxX - minX, maxY - minY);
        }

        /// <summary>获取矩形经过矩阵变换后的正外接矩形 (AABB)</summary>
        public static RectangleF GetTransformedBounds(RectangleF rect, Matrix3x2 matrix)
        {
            if (rect.IsEmpty || matrix.IsIdentity) return rect;

            PointF[] corners = new PointF[]
            {
                matrix.TransformPoint(new PointF(rect.Left, rect.Top)),
                matrix.TransformPoint(new PointF(rect.Right, rect.Top)),
                matrix.TransformPoint(new PointF(rect.Right, rect.Bottom)),
                matrix.TransformPoint(new PointF(rect.Left, rect.Bottom))
            };

            return GetBounds(corners);
        }

        /// <summary>计算点到线段的垂直/最短距离</summary>
        public static float DistanceToLineSegment(PointF pt, PointF p1, PointF p2)
        {
            float dx = p2.X - p1.X;
            float dy = p2.Y - p1.Y;
            float lengthSq = dx * dx + dy * dy;

            if (lengthSq < 1e-6f)
            {
                return CalculateLengthFromTwoPoints(pt, p1);
            }

            // 投影比例 t
            float t = ((pt.X - p1.X) * dx + (pt.Y - p1.Y) * dy) / lengthSq;
            t = System.Math.Max(0f, System.Math.Min(1f, t));

            PointF projection = new PointF(p1.X + t * dx, p1.Y + t * dy);
            return CalculateLengthFromTwoPoints(pt, projection);
        }

        /// <summary>判断点是否在线段附近（带容差范围）</summary>
        public static bool IsPointNearLine(PointF pt, PointF p1, PointF p2, float tolerance = 3f)
        {
            return DistanceToLineSegment(pt, p1, p2) <= tolerance;
        }
    }
}