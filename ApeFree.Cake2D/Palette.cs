using ApeFree.Cake2D.Shapes;
using ApeFree.Cake2D.Shapes;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Linq;

namespace ApeFree.Cake2D
{
    /// <summary>
    /// 图形面板基类
    /// </summary>
    /// <typeparam name="TStyle">绘制风格类型</typeparam>
    public abstract class Palette<TStyle> : IDisposable
    {
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

        public Layer<TStyle, TShape> Draw<TShape>(TStyle style, TShape graphic) where TShape : IShape
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
