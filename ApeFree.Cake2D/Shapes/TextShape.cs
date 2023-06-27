using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ApeFree.Cake2D.Shapes
{
    public class TextShape : IShape
    {
        public TextShape(PointF location, float width, float height, string text)
        {
            Location = location;
            Width = width;
            Height = height;
            Text = text;
        }
        public PointF Location { get; set; }

        public float Width { get; set; }

        public float Height { get; set; }

        public string Text { get; set; }

        public void Scale(float scaling)
        {

        }

        public void Offset(float distanceX, float distanceY)
        {
            Location = Location.Add(distanceX, distanceY);
        }

        public void Rotate(PointF centralPoint, float angle)
        {
        }

        public bool Contains(PointF point)
        {
            if (point.X >= Location.X && point.X <= Location.X + Width && point.Y >= Height && point.Y <= Location.Y + Height)
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        public RectangleShape GetBounds()
        {
            return new RectangleShape(Location, Width, Height);
        }

        public float Left
        {
            get { return Location.X; }
            set
            {
                float delta = value - Left;
                Offset(delta, 0);
            }
        }

        public float Top
        {
            get { return Location.Y; }
            set
            {
                float delta = value - Top;
                Offset(0, delta);
            }
        }

        public IEnumerable<PointF> Points => new[] { Location };

    }
}
