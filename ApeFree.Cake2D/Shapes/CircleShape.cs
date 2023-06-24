using ApeFree.Cake2D.Shapes;
using ApeFree.Cake2D;
using System;
using System.Collections.Generic;
using System.Drawing;

namespace ApeFree.Cake2D.Shapes
{
    public class CircleShape : ICircle, IPlaneShape
    {
        private float radius;
        private PointF centerPoint;

        public CircleShape(PointF centerPoint, float radius)
        {
            this.centerPoint = centerPoint;
            this.radius = radius;
        }

        public PointF CenterPoint { get => centerPoint; set => centerPoint = value; }

        public float Radius
        {
            get => radius;
            set => radius = value;
        }

        public double CalculateArea()
        {
            return Math.PI * radius * radius;
        }

        public double CalculatePerimeter()
        {
            return 2 * Math.PI * radius;
        }

        public bool Contains(PointF point)
        {
            return (point.X - centerPoint.X) * (point.X - centerPoint.X) +
                   (point.Y - centerPoint.Y) * (point.Y - centerPoint.Y) <= radius * radius;
        }

        public RectangleShape GetBounds()
        {
            return new RectangleShape((int)(centerPoint.X - radius), (int)(centerPoint.Y - radius),
                                 (int)(2 * radius), (int)(2 * radius));
        }

        public void Offset(float distanceX, float distanceY)
        {
            centerPoint = new PointF(centerPoint.X + distanceX, centerPoint.Y + distanceY);
        }

        public PointF Centroid => centerPoint;

        public IEnumerable<PointF> Points
        {
            get
            {
                yield return centerPoint;
            }
        }

        public void Rotate(PointF centralPoint, float angle)
        {
            centerPoint = Math2D.PointAround(centralPoint, centerPoint, angle);
        }

        public void Scale(float scaling)
        {
            radius *= scaling;
        }
    }
}
