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
        /// <summary>
        /// 全局默认字体名称。默认为 null，代表交由具体渲染引擎自适应匹配操作系统最佳字体。
        /// </summary>
        public static string DefaultFontName { get; set; } = null;

        private string _fontName;

        /// <summary>
        /// 字体名称。若未显式指定，则继承自 <see cref="DefaultFontName"/>。
        /// </summary>
        public string FontName
        {
            get => _fontName ?? DefaultFontName;
            set => _fontName = value;
        }

        public string Text { get; set; } = string.Empty;
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
            if (string.IsNullOrEmpty(Text)) return RectangleF.Empty;

            // 按单字符精确区分宽窄字符计算文本宽度，大幅降低排版与包围盒偏差
            float width = 0f;
            for (int i = 0; i < Text.Length; i++)
            {
                width += (Text[i] > 127 ? 1.0f : 0.6f) * FontSize;
            }
            float height = FontSize * 1.25f;

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
                // 翻转 Y 轴基底向量，使文字笔划向下正向绘制，同时保持基线朝向与锚点屏幕位置完全精准
                transform = new Matrix3x2(
                    transform.M11,
                    transform.M12,
                    -transform.M21,
                    -transform.M22,
                    transform.M31,
                    transform.M32
                );
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
