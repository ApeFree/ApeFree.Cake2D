using System.Drawing;
using ApeFree.Cake2D.Math;
using ApeFree.Cake2D.Rendering;

namespace ApeFree.Cake2D.Elements.Billboards
{
    /// <summary>
    /// 图像纹理图元。
    /// 借助底层仿射变换矩阵，支持任意角度旋转、缩放与平移贴图。
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
            if (Image == null || Width <= 0f || Height <= 0f) return;

            Matrix3x2 transform = context.GetElementToScreenMatrix(this);

            // 当 Y 轴向上（CAD 模式，垂直镜像）时，需要反转 Y 轴，保持标牌图片正向
            if (context.Viewport.YAxisUpwards)
            {
                // 先将图片按自身中心或左下/左上镜像翻转，由于原本是从 (0,0) 到 (Width, Height)，
                // 翻转 Y 轴后沿 Y 方向负向延伸，故补加位移使其仍占据 [0, Height] 区域
                transform = Matrix3x2.CreateScale(1f, -1f) * Matrix3x2.CreateTranslation(0f, Height) * transform;
            }

            canvas.PushTransform(transform);
            try
            {
                canvas.DrawImage(Image, new RectangleF(0, 0, Width, Height));
            }
            finally
            {
                canvas.PopTransform();
            }
        }
    }
}
