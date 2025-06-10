using ApeFree.Cake2D.Shapes;
using System;
using System.Collections.Generic;
using System.Drawing;

namespace ApeFree.Cake2D
{
    /// <summary>
    /// 图形面板基类
    /// </summary>
    /// <typeparam name="TStyle">绘制风格类型</typeparam>
    public abstract class Palette<TStyle> : IDisposable
    {
        private SizeF outputSize = new SizeF(100, 100);
        private SizeF canvasSize = new SizeF(100, 100);
        private float scale = 1f;

        /// <summary>
        /// 缩放比例
        /// </summary>
        public float Scale { get => scale; set => scale = value > 0 ? value : 0; }

        /// <summary>
        /// 原点偏移
        /// </summary>
        public PointF OriginOffset { get; set; } = new PointF(0, 0);

        /// <summary>
        /// 缩放中心
        /// </summary>
        public PointF ZoomCenter { get; set; } = new PointF(0, 0);

        /// <summary>
        /// 画布大小
        /// </summary>
        public SizeF CanvasSize
        {
            get => canvasSize;
            set
            {
                if (canvasSize != value)
                {
                    canvasSize = value;
                    OnCanvasSizeChanged();
                }
            }
        }

        /// <summary>
        /// 背景色
        /// </summary>
        public Color BackColor { get; set; } = Color.Transparent;

        /// <summary>
        /// 输出图像大小
        /// </summary>
        public SizeF OutputSize
        {
            get => outputSize;
            set
            {
                if (outputSize != value)
                {
                    outputSize = value;
                    OnOutputSizeChanged();
                }
            }
        }

        protected virtual void OnCanvasSizeChanged() { Scale = 1; }

        protected virtual void OnOutputSizeChanged() { Scale = 1; }

        /// <summary>图层</summary>
        public IList<Layer<TStyle>> Layers { get; private set; }

        /// <summary>构造画板</summary>
        protected internal Palette()
        {
            Layers = new List<Layer<TStyle>>();
        }

        /// <inheritdoc/>
        public virtual void Dispose()
        {
            Layers.Clear();
            Layers = null;
        }

        public PointF TransformAbsCoordsToRelCoords(PointF point)
        {
            // 计算缩放后的坐标
            var x = (point.X + OriginOffset.X) * Scale - ZoomCenter.X;
            var y = (point.Y + OriginOffset.Y) * Scale - ZoomCenter.Y;
            return new PointF(x, y);
        }

        public PointF TransformRelCoordsToAbsCoords(PointF point)
        {
            var x = (point.X + ZoomCenter.X) / Scale - OriginOffset.X;
            var y = (point.Y + ZoomCenter.Y) / Scale - OriginOffset.Y;
            return new PointF(x, y);
        }

        /// <summary>更新画布</summary>
        public virtual void UpdateCanvas()
        {
            foreach (var layer in Layers)
            {
                // 跳过绘制不可见的图层
                if (!layer.Visible)
                {
                    continue;
                }

                // 根据图形类型调用对应的绘制实现
                switch (layer.Shape)
                {
                    case LineShape shape:
                        DrawLineHandler(layer.Style, shape);
                        break;
                    case VectorSahpe shape:
                        DrawVectorHandler(layer.Style, shape);
                        break;
                    case EllipseShape shape:
                        DrawEllipseHandler(layer.Style, shape);
                        break;
                    case RectangleShape shape:
                        DrawRectangleHandler(layer.Style, shape);
                        break;
                    case CircleShape shape:
                        DrawCircleHandler(layer.Style, shape);
                        break;
                    case PolygonShape shape:
                        DrawPolygonHandler(layer.Style, shape);
                        break;
                    case TextShape shape:
                        DrawTextHandler(layer.Style, shape);
                        break;
                    case ImageShape shape:
                        DrawImageHandler(layer.Style, shape);
                        break;
                    case ComplexShape shape:
                        DrawComplexShapeHandler(layer.Style, shape);
                        break;
                    default:
                        break;
                }
            }
        }

        /// <summary>绘制多边形</summary>
        public Layer<TStyle, PolygonShape> DrawPolygon(TStyle style, PolygonShape graphic) => Draw(style, graphic);

        /// <summary>绘制线</summary>
        public Layer<TStyle, LineShape> DrawLine(TStyle style, LineShape graphic) => Draw(style, graphic);

        /// <summary>绘制向量</summary>
        public Layer<TStyle, VectorSahpe> DrawVector(TStyle style, VectorSahpe graphic) => Draw(style, graphic);

        /// <summary>绘制椭圆</summary>
        public Layer<TStyle, EllipseShape> DrawEllipse(TStyle style, EllipseShape graphic) => Draw(style, graphic);

        /// <summary>绘制矩形</summary>
        public Layer<TStyle, RectangleShape> DrawRectangle(TStyle style, RectangleShape graphic) => Draw(style, graphic);

        /// <summary>绘制圆形</summary>
        public Layer<TStyle, CircleShape> DrawCircle(TStyle style, CircleShape graphic) => Draw(style, graphic);

        /// <summary>绘制文本</summary>
        public Layer<TStyle, TextShape> DrawText(TStyle style, TextShape graphic) => Draw(style, graphic);

        /// <summary>绘制文本</summary>
        public Layer<TStyle, ImageShape> DrawImage(TStyle style, ImageShape graphic) => Draw(style, graphic);

        /// <summary>绘制</summary>
        public Layer<TStyle, TShape> Draw<TShape>(TStyle style, TShape graphic) where TShape : Shape
        {
            var layer = new Layer<TStyle, TShape>(this, style, graphic);
            Layers.Add(layer);
            return layer;
        }

        /// <summary>绘制椭圆的实现过程</summary>
        protected abstract void DrawEllipseHandler(TStyle style, EllipseShape graphic);
        /// <summary>绘制线的实现过程</summary>
        protected abstract void DrawLineHandler(TStyle style, LineShape graphic);
        /// <summary>绘制向量的实现过程</summary>
        protected abstract void DrawVectorHandler(TStyle style, VectorSahpe graphic);
        /// <summary>绘制矩形的实现过程</summary> 
        protected abstract void DrawRectangleHandler(TStyle style, RectangleShape graphic);
        /// <summary>绘制圆形的实现过程</summary> 
        protected abstract void DrawCircleHandler(TStyle style, CircleShape graphic);
        /// <summary>绘制多边形的实现过程</summary> 
        protected abstract void DrawPolygonHandler(TStyle style, PolygonShape shape);
        /// <summary>绘制文本的实现过程</summary> 
        protected abstract void DrawTextHandler(TStyle style, TextShape shape);
        /// <summary>绘制图像的实现过程</summary> 
        protected abstract void DrawImageHandler(TStyle style, ImageShape shape);
        /// <summary>绘制复合图形的实现过程</summary> 
        protected abstract void DrawComplexShapeHandler(TStyle style, ComplexShape shape);


        /// <summary>
        /// 通过投射点查找顶部图层
        /// </summary>
        /// <param name="point">投射点</param>
        /// <returns></returns>
        public Layer<TStyle> SelectTopLayerByCastingPoint(PointF point)
        {
            for (int i = Layers.Count - 1; i >= 0; i--)
            {
                var layer = Layers[i];

                if (layer.Visible && layer.Selectable && layer.Shape.Contains(point))
                {
                    return layer;
                }
            }
            return null;
        }
    }

    /// <summary>
    /// 图形面板基类
    /// </summary>
    /// <typeparam name="TCanvas">画布类型</typeparam>
    /// <typeparam name="TStyle">绘制风格类型</typeparam>
    public abstract class Palette<TCanvas, TStyle> : Palette<TStyle>
    {
        /// <summary>
        /// 画布
        /// </summary>
        public TCanvas Canvas { get; set; }

        /// <summary>
        /// 构造画板
        /// </summary>
        protected Palette() { }

        /// <summary>
        /// 构造画板
        /// </summary>
        /// <param name="canvas">画布对象</param>
        protected Palette(TCanvas canvas) : this()
        {
            Canvas = canvas;
        }
    }
}
