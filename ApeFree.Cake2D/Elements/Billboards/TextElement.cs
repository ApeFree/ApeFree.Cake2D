using System.Drawing;
using ApeFree.Cake2D.Math;
using ApeFree.Cake2D.Rendering;

namespace ApeFree.Cake2D.Elements.Billboards
{
    /// <summary>
    /// 文本图元。
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
            // 估算文本包围盒
            float width = Text.Length * FontSize * 0.7f;
            float height = FontSize * 1.2f;
            return new RectangleF(0, 0, width, height);
        }

        protected override void OnRender(IRenderCanvas canvas, RenderContext2D context)
        {
            if (string.IsNullOrEmpty(Text)) return;

            Matrix3x2 worldMatrix = GetWorldMatrix();
            PointF screenPos = context.Project(worldMatrix.TransformPoint(new PointF(0, 0)));

            Color color = StrokeColor ?? Color.Black;
            string fontName = FontName;
            float fontSize = FontSize;
            bool isBold = IsBold;
            TextAlignment alignment = Alignment;
            string text = Text;

            context.Submit(ZIndex, c =>
            {
                c.DrawText(text, screenPos, color, fontName, fontSize, isBold, alignment);
            });
        }
    }
}