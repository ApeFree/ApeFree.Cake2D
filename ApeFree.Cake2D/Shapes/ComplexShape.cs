using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ApeFree.Cake2D.Shapes
{
    /// <summary>
    /// 复合图形
    /// </summary>
    public class ComplexShape : IShape
    {
        public ComplexShape()
        {
            Shapes = new LinkedList<IShape>();
        }

        /// <summary>
        /// 内部图形集合
        /// </summary>
        public LinkedList<IShape> Shapes { get; }

        /// <inheritdoc/>
        public PointF[] Points => Shapes.Select(g => g.Points).SelectMany(ps => ps).ToArray();

        /// <inheritdoc/>
        public void Rotate(PointF centralPoint, float angle)
        {
            foreach (IShape g in Shapes)
            {
                g.Rotate(centralPoint, angle);
            }
        }

        /// <inheritdoc/>
        public void Scale(float scaling)
        {
            foreach (IShape g in Shapes)
            {
                g.Scale(scaling);
            }
        }

        /// <inheritdoc/>
        public void Offset(float distanceX, float distanceY)
        {
            foreach (IShape g in Shapes)
            {
                g.Offset(distanceX, distanceY);
            }
        }

        /// <inheritdoc/>
        public bool Contains(PointF point)
        {
            foreach (IShape g in Shapes)
            {
                if (g.Contains(point))
                {
                    return true;
                }
            }
            return false;
        }

        /// <inheritdoc/>
        public RectangleShape GetBounds()
        {
            throw new NotImplementedException();
        }
    }
}
