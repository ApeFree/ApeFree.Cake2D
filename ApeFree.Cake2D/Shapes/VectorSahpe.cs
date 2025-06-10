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
    public class VectorSahpe : Shape
    {
        public override ShapeType ShapeType => ShapeType.Vector;

        public VectorSahpe(PointF startPoint, float length, float angle) : base([startPoint])
        {
            StartPoint = startPoint;
            Angle = angle;
            Length = length;
        }

        /// <summary>
        /// 起始点
        /// </summary>
        public PointF StartPoint { get; set; }

        /// <summary>
        /// 线长
        /// </summary>
        public float Length { get; set; }

        /// <summary>
        /// 结束点
        /// </summary>
        public PointF EndPoint => Math2D.CalculatePointOnCircle(StartPoint, Length, Angle);

        /// <inheritdoc/>
        public override bool Contains(PointF point)
        {
            return Points.Contains(point);
        }
    }
}
