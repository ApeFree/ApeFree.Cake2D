using ApeFree.Cake2D;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;

namespace ApeFree.Cake2D.Shapes
{
    /// <summary>
    /// 向量
    /// </summary>
    public class VectorSahpe : IShape
    {
        public VectorSahpe(PointF startPoint, float length, float angle)
        {
            StartPoint = startPoint;
            Length = length;
            Angle = angle;
        }

        public PointF StartPoint { get; set; }

        public float Length { get; set; }

        public float Angle { get; set; }

        public PointF EndPoint => Math2D.CalculatePointOnCircle(StartPoint, Length, Angle);

        public IEnumerable<PointF> Points => new PointF[] { StartPoint, EndPoint };

        /// <inheritdoc/>
        public bool Contains(PointF point)
        {
            return Points.Contains(point);
        }

        /// <inheritdoc/>
        public RectangleShape GetBounds()
        {
            return new RectangleShape(StartPoint, (EndPoint.X - StartPoint.X), EndPoint.Y - StartPoint.Y);
        }

        /// <inheritdoc/>
        public void Offset(float distanceX, float distanceY)
        {
            StartPoint = new PointF(StartPoint.X + distanceX, StartPoint.Y + distanceY);
        }

        /// <inheritdoc/>
        public void Rotate(PointF centralPoint, float angle)
        {
            Angle += angle;
        }

        /// <inheritdoc/>
        public void Scale(float scaling)
        {
            Length *= scaling;
        }
    }
}
