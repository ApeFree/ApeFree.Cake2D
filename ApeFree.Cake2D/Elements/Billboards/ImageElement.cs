using System.Drawing;
using ApeFree.Cake2D.Math;
using ApeFree.Cake2D.Rendering;

namespace ApeFree.Cake2D.Elements.Billboards
{
    /// <summary>
    /// 图像纹理图元。
    /// </summary>
    public class ImageElement : Element2D
    {
        public object Image { get; set; }
        public float Width { get; set; } = 100f;
        public float Height { get; set; } = 100f;

        public ImageElement() { }

        public ImageElement(object image, float width, float height)
        {
            Image = image;
            Width = width;
            Height = height;
        }

        public override RectangleF GetLocalBounds() => new RectangleF(0, 0, Width, Height);

        protected override void OnRender(IRenderCanvas canvas, RenderContext2D context)
        {
            if (Image == null) return;

            Matrix3x2 worldMatrix = GetWorldMatrix();
            PointF p0 = context.Project(worldMatrix.TransformPoint(new PointF(0, 0)));
            PointF p2 = context.Project(worldMatrix.TransformPoint(new PointF(Width, Height)));

            float minX = System.Math.Min(p0.X, p2.X);
            float minY = System.Math.Min(p0.Y, p2.Y);
            float screenW = System.Math.Abs(p2.X - p0.X);
            float screenH = System.Math.Abs(p2.Y - p0.Y);

            RectangleF destRect = new RectangleF(minX, minY, screenW, screenH);
            object img = Image;

            context.Submit(ZIndex, c =>
            {
                c.DrawImage(img, destRect);
            });
        }
    }
}