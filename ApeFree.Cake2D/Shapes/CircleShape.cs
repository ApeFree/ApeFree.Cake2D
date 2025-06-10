using System;
using System.Drawing;

namespace ApeFree.Cake2D.Shapes
{
    /// <summary>正圆形接口</summary>
    public class CircleShape : PlaneShape
    {
        public override ShapeType ShapeType => ShapeType.Circle;

        public CircleShape(PointF points, float radius) : base([points])
        {
            Radius = radius;
        }

        public CircleShape(float x, float y, float radius) : this(new PointF(x, y), radius) { }

        /// <summary>圆心</summary>
        public PointF Location { get => Points[0]; set => Points[0] = value; }

        /// <summary>半径</summary>
        public float Radius { get; set; }

        public override double CalculateArea()
        {
            return Math.PI * Radius * Radius;
        }

        public override double CalculatePerimeter()
        {
            return Math.PI * Radius * 2;
        }

        public override bool Contains(PointF point)
        {
            return (point.X - Location.X) * (point.X - Location.X) + (point.Y - Location.Y) * (point.Y - Location.Y) <= Radius * Radius;
        }
    }
}
