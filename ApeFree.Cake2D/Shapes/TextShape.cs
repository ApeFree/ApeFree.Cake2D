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
       
        /// <summary>
        /// 文本的左上角坐标
        /// </summary>
        public PointF Location { get; set; }

        /// <summary>
        /// 文本区域宽度
        /// </summary>
        public float Width { get; set; }

        /// <summary>
        /// 文本区域高度
        /// </summary>
        public float Height { get; set; }

        /// <summary>
        /// 文本内容
        /// </summary>
        public string Text { get; set; }

        /// <inheritdoc/>
        public void Scale(float scaling)
        {

        }

        /// <inheritdoc/>
        public void Offset(float distanceX, float distanceY)
        {
            Location = Location.Add(distanceX, distanceY);
        }

        /// <inheritdoc/>
        public void Rotate(PointF centralPoint, float angle)
        {
        }

        /// <inheritdoc/>
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

        /// <inheritdoc/>
        public RectangleShape GetBounds()
        {
            return new RectangleShape(Location, Width, Height);
        }

        /// <summary>
        /// 距离容器左边距离
        /// </summary>
        public float Left
        {
            get { return Location.X; }
            set
            {
                float delta = value - Left;
                Offset(delta, 0);
            }
        }

        /// <summary>
        /// 距离容器顶部距离
        /// </summary>
        public float Top
        {
            get { return Location.Y; }
            set
            {
                float delta = value - Top;
                Offset(0, delta);
            }
        }

        /// <inheritdoc/>
        public PointF[] Points => new[] { Location };

    }
}
