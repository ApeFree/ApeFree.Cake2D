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
    public class ComplexShape : Shape
    {
        /// <summary>
        /// 内部图形集合
        /// </summary>
        public LinkedList<Shape> Shapes { get; }

        public override ShapeType ShapeType => ShapeType.Complex;

        public ComplexShape(PointF location) : base([location])
        {
            Shapes = new LinkedList<Shape>();
        }

        /// <inheritdoc/>
        public override bool Contains(PointF point)
        {
            foreach (Shape g in Shapes)
            {
                if (g.Contains(point))
                {
                    return true;
                }
            }
            return false;
        }
    }
}
