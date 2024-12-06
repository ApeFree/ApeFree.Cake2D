using ApeFree.Cake2D.Shapes;
using ApeFree.Cake2D;
using System;
using System.Collections.Generic;
using System.Drawing;

namespace ApeFree.Cake2D.Shapes
{
    /// <summary>
    /// 正圆图形
    /// </summary>
    public class CircleShape : ICircle, IPlaneShape
    {
        private float radius;
        private PointF centerPoint;
        private double? radiusSquare;

        public CircleShape(PointF centerPoint, float radius)
        {
            this.centerPoint = centerPoint;
            this.radius = radius;
        }

        /// <inheritdoc/>
        public PointF CenterPoint { get => centerPoint; set => centerPoint = value; }

        /// <inheritdoc/>
        public float Radius
        {
            get => radius;
            set { radius = value; radiusSquare = null; }
        }

        /// <summary>
        /// 半径的平方
        /// </summary>
        protected double RadiusSquare
        {
            get
            {
                radiusSquare ??= Math.Pow(radius, 2);
                return radiusSquare.Value;
            }
        }

        /// <inheritdoc/>
        public double CalculateArea()
        {
            return Math.PI * radius * radius;
        }

        /// <inheritdoc/>
        public double CalculatePerimeter()
        {
            return 2 * Math.PI * radius;
        }

        /// <inheritdoc/>
        public bool Contains(PointF point)
        {
            var diffX = Math.Abs(point.X - CenterPoint.X);
            if (diffX > Radius)
            {
                return false;
            }

            var diffY = Math.Abs(point.Y - CenterPoint.Y);
            if (diffY > Radius)
            {
                return false;
            }

            var distanceSquare = Math.Pow(diffX, 2) + Math.Pow(diffY, 2);
            return distanceSquare < RadiusSquare;
        }

        /// <inheritdoc/>
        public RectangleShape GetBounds()
        {
            return new RectangleShape((int)(centerPoint.X - radius), (int)(centerPoint.Y - radius),
                                 (int)(2 * radius), (int)(2 * radius));
        }

        /// <inheritdoc/>
        public void Offset(float distanceX, float distanceY)
        {
            centerPoint = new PointF(centerPoint.X + distanceX, centerPoint.Y + distanceY);
        }

        /// <inheritdoc/>
        public PointF Centroid => centerPoint;

        /// <inheritdoc/>
        public PointF[] Points => new PointF[] { centerPoint };

        /// <inheritdoc/>
        public void Rotate(PointF centralPoint, float angle)
        {
            centerPoint = Math2D.PointAround(centralPoint, centerPoint, angle);
        }

        /// <inheritdoc/>
        public void Scale(float scaling)
        {
            radius *= scaling;
        }
    }
}
