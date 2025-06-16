using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ApeFree.Cake2D.Shapes
{
    /// <summary>图形基类</summary>
    public abstract class Shape
    {
        private readonly object lockerGetDisplayPoints = new object();

        /// <summary>是否需要重新计算显示坐标点</summary>
        protected internal bool IsRecomputeDisplayPointsNeeded => Angle != angle || !Enumerable.SequenceEqual(pointsCache, Points);

        /// <summary>图形上所有点的真实坐标（旋转之前）</summary>
        public PointF[] Points { get; }
        private PointF[] pointsCache;

        /// <summary>图形上所有点的显示坐标</summary>
        public PointF[] DisplayPoints
        {
            get
            {
                lock (lockerGetDisplayPoints)
                {
                    if (IsRecomputeDisplayPointsNeeded)
                    {
                        RecomputeDisplayPoints();
                    }
                    return displayPoints.ToArray();
                }
            }
            protected set => displayPoints = value.ToArray();
        }
        private PointF[] displayPoints;

        /// <summary>图像的旋转角度</summary>
        public float Angle { get; set; }
        private float angle;

        /// <summary>图形类型</summary>
        public abstract ShapeType ShapeType { get; }

        protected Shape(PointF[] points)
        {
            Points = points;
            pointsCache = points.ToArray();
            // displayPoints = points.ToArray();
        }

        /// <summary>重新计算显示点的坐标</summary>
        private void RecomputeDisplayPoints()
        {
            OnDisplayPointsRecomputing();
            Points.CopyTo(pointsCache, 0);
            angle = Angle;
        }

        /// <summary>当显示点重新计算的时候</summary>
        protected virtual void OnDisplayPointsRecomputing()
        {
            if (Angle != 0)
            {
                var centerPoint = GetRotationAxisPoint();
                var updatePoints = new PointF[Points.Length];
                for (int i = 0; i < Points.Length; i++)
                {
                    updatePoints[i] = Math2D.PointAround(centerPoint, Points[i], Angle);
                }
                displayPoints = updatePoints;
            }
            else
            {
                displayPoints = Points.ToArray();
            }
        }

        /// <summary>平移</summary>
        /// <param name="distanceX">X轴平移距离</param>
        /// <param name="distanceY">Y轴平移距离</param>
        public virtual void Offset(float distanceX, float distanceY)
        {
            var updatePoints = new PointF[Points.Length];
            for (int i = 0; i < Points.Length; i++)
            {
                updatePoints[i] = Points[i].Add(distanceX, distanceY);
            }
            updatePoints.CopyTo(Points, 0);
        }

        /// <summary>获取外接矩形</summary>
        public RectangleF GetBounds()
        {
            var top = float.MaxValue;
            var left = float.MaxValue;
            var bottom = float.MinValue;
            var right = float.MinValue;
            for (int i = 0; i < DisplayPoints.Length; i++)
            {
                var point = DisplayPoints[i];
                if (point.X < left)
                {
                    left = point.X;
                }
                if (point.X > right)
                {
                    right = point.X;
                }
                if (point.Y < top)
                {
                    top = point.Y;
                }
                if (point.Y > bottom)
                {
                    bottom = point.Y;
                }
            }

            return new RectangleF(left, top, right - left, bottom - top);
        }

        /// <summary>获取图形的旋转轴点</summary>
        protected virtual PointF GetRotationAxisPoint()
        {
            var rect = GetBounds();
            return new PointF(rect.X + rect.Width / 2, rect.Y + rect.Height / 2);
        }

        /// <summary>指定点是否在图形内部</summary>
        /// <param name="point"></param>
        public abstract bool Contains(PointF point);
    }

    public enum ShapeType
    {
        Ellipse,
        Circle,
        Rectangle,
        Line,
        Polygon,
        Point,
        Text,
        Vector,
        Complex,
        Matrix,
        Image
    }


    /// <summary>平面图形接口</summary>
    public abstract class PlaneShape : Shape
    {
        protected PlaneShape(PointF[] points) : base(points) { }

        /// <summary>
        /// 计算周长
        /// </summary>
        public abstract double CalculatePerimeter();

        /// <summary>
        /// 计算面积
        /// </summary>
        public abstract double CalculateArea();
    }
}
