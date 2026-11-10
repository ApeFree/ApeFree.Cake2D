using System.Drawing;
using ApeFree.Cake2D.Math;
using ApeFree.Cake2D.Rendering;

namespace ApeFree.Cake2D.Elements.Billboards
{
    /// <summary>
    /// 文本图元。
    /// 借助底层仿射变换矩阵，支持任意角度旋转与缩放渲染。
    /// </summary>
    public class TextElement : Element2D
    {
        public string Text { get; set; } = string.Empty;
        public string FontName { get; set; } = "Arial";
        public float FontSize { get; set; } = 12f;
        public bool IsBold { get; set; } = false;
        public TextAlignment Alignment { get; set; } = TextAlignment.TopLeft;

        public TextElement() { }

        public TextElement(string text, float x = 0, float y = 0)
        {
            Text = text;
            Position = new Vector2D(x, y);
        }

        public override RectangleF GetLocalBounds()
        {
            // 估算文本包围盒（考虑宽字符/中文字符估算）
            float charWidthRatio = 0.7f;
            for (int i = 0; i < Text.Length; i++)
            {
                if (Text[i] > 127) { charWidthRatio = 1.0f; break; }
            }

            float width = Text.Length * FontSize * charWidthRatio;
            float height = FontSize * 1.2f;

            // 根据对齐方式调整包围盒原点偏移
            float offsetX = 0f;
            float offsetY = 0f;

            switch (Alignment)
            {
                case TextAlignment.TopLeft:
                case TextAlignment.MiddleLeft:
                case TextAlignment.BottomLeft:
                    offsetX = 0f;
                    break;
                case TextAlignment.TopCenter:
                case TextAlignment.MiddleCenter:
                case TextAlignment.BottomCenter:
                    offsetX = -width * 0.5f;
                    break;
                case TextAlignment.TopRight:
                case TextAlignment.MiddleRight:
                case TextAlignment.BottomRight:
                    offsetX = -width;
                    break;
            }

            switch (Alignment)
            {
                case TextAlignment.TopLeft:
                case TextAlignment.TopCenter:
                case TextAlignment.TopRight:
                    offsetY = 0f;
                    break;
                case TextAlignment.MiddleLeft:
                case TextAlignment.MiddleCenter:
                case TextAlignment.MiddleRight:
                    offsetY = -height * 0.5f;
                    break;
                case TextAlignment.BottomLeft:
                case TextAlignment.BottomCenter:
                case TextAlignment.BottomRight:
                    offsetY = -height;
                    break;
            }

            return new RectangleF(offsetX, offsetY, width, height);
        }

        protected override void OnRender(IRenderCanvas canvas, RenderContext2D context)
        {
            if (string.IsNullOrEmpty(Text)) return;

            Matrix3x2 transform = context.GetElementToScreenMatrix(this);

            // 当 Y 轴向上（CAD 模式，垂直镜像）时，需要反转 Y 轴，保持标牌文字屏幕正向阅读
            if (context.Viewport.YAxisUpwards)
            {
                transform = Matrix3x2.CreateScale(1f, -1f) * transform;
            }

            canvas.PushTransform(transform);
            try
            {
                Color color = StrokeColor ?? Color.Black;
                canvas.DrawText(Text, PointF.Empty, color, FontName, FontSize, IsBold, Alignment);
            }
            finally
            {
                canvas.PopTransform();
            }
        }
    }
}
