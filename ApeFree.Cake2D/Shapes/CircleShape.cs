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
            set => radius = value;
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
            return (point.X - centerPoint.X) * (point.X - centerPoint.X) +
                   (point.Y - centerPoint.Y) * (point.Y - centerPoint.Y) <= radius * radius;
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
