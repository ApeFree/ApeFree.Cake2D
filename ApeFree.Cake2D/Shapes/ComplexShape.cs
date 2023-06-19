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

        public LinkedList<IShape> Shapes { get; }

        public IEnumerable<PointF> Points => Shapes.Select(g => g.Points).SelectMany(ps=>ps);

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        /// <param name="centralPoint"></param>
        /// <param name="angle"></param>
        public void Rotate(PointF centralPoint, float angle)
        {
            foreach (IShape g in Shapes)
            {
                g.Rotate(centralPoint, angle);
            }
        }

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        /// <param name="scaling"></param>
        public void Scale(float scaling)
        {
            foreach (IShape g in Shapes)
            {
                g.Scale(scaling);
            }
        }

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        /// <param name="distanceX"></param>
        /// <param name="distanceY"></param>
        public void Offset(float distanceX, float distanceY)
        {
            foreach (IShape g in Shapes)
            {
                g.Offset(distanceX, distanceY);
            }
        }

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

        public RectangleShape GetBounds()
        {
            throw new NotImplementedException();
        }
    }
}
