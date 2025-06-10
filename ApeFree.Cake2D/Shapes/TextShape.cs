using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ApeFree.Cake2D.Shapes
{
    public class TextShape : Shape
    {
        public override ShapeType ShapeType => ShapeType.Text;

        public TextShape(PointF location, float width, float height, string text) : base([location])
        {
            Location = location;
            Width = width;
            Height = height;
            Text = text;
        }

        /// <summary>
        /// 文本的左上角坐标
        /// </summary>
        public PointF Location { get => Points[0]; set => Points[0] = value; }

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


        public override bool Contains(PointF point)
        {
            var rect = new RectangleF(Location.X, Location.Y, Width, Height);
            return rect.Contains(point);
        }
    }

    public class ImageShape : Shape
    {
        public override ShapeType ShapeType => ShapeType.Image;

        public ImageShape(PointF location, float width, float height, object image) : base([location])
        {
            Location = location;
            Width = width;
            Height = height;
            Image = image;
        }

        /// <summary>
        /// 文本的左上角坐标
        /// </summary>
        public PointF Location { get => Points[0]; set => Points[0] = value; }

        /// <summary>
        /// 文本区域宽度
        /// </summary>
        public float Width { get; set; }

        /// <summary>
        /// 文本区域高度
        /// </summary>
        public float Height { get; set; }

        /// <summary>
        /// 图像数据
        /// </summary>
        public object Image { get; set; }

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


        public override bool Contains(PointF point)
        {
            var rect = new RectangleF(Location.X, Location.Y, Width, Height);
            return rect.Contains(point);
        }
    }
}
