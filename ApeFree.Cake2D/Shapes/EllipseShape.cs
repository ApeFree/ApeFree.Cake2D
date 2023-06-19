using System.Drawing;

namespace ApeFree.Cake2D.Shapes
{
    public class EllipseShape : RectangleShape, IEllipse
    {
        public EllipseShape(PointF centerPoint, float width, float height) : base(centerPoint.X - width / 2, centerPoint.Y - height / 2, width, height) { }

        public EllipseShape(float left, float top, float width, float height) : base(left, top, width, height) { }

        public PointF CenterPoint => Centroid;
    }
}
