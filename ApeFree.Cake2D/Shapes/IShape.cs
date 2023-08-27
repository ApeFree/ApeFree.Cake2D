using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ApeFree.Cake2D.Shapes
{
    /// <summary>图形接口</summary>
    public interface IShape
    {
        /// <summary>缩放</summary>
        /// <param name="scaling">缩放比例</param>
        void Scale(float scaling);

        /// <summary>平移</summary>
        /// <param name="distanceX">X轴平移距离</param>
        /// <param name="distanceY">Y轴平移距离</param>
        void Offset(float distanceX, float distanceY);

        /// <summary>旋转</summary>
        /// <param name="centralPoint">中心点</param>
        /// <param name="angle">旋转角度</param>
        void Rotate(PointF centralPoint, float angle);

        /// <summary>指定点是否在图形内部</summary>
        /// <param name="point"></param>
        bool Contains(PointF point);

        /// <summary>图形上所有的点</summary>
        PointF[] Points { get; }

        /// <summary>获取外接矩形</summary>
        RectangleShape GetBounds();
    }

    /// <summary>平面图形接口</summary>
    public interface IPlaneShape : IShape
    {
        /// <summary>计算周长</summary>
        double CalculatePerimeter();

        /// <summary>计算面积</summary>
        double CalculateArea();
    }

    /// <summary>多边形接口</summary>
    public interface IPolygon : IPlaneShape
    {
        /// <summary>多边形的重心</summary>
        PointF Centroid { get; }
    }

    /// <summary>矩形接口</summary>
    public interface IRectangle
    {
        /// <summary>矩形宽度</summary>
        float Width { get; set; }
        /// <summary>矩形高度</summary>
        float Height { get; set; }
        /// <summary>旋转角度</summary>
        float Angle { get; set; }
    }

    /// <summary>椭圆形接口</summary>
    public interface IEllipse : IRectangle
    {
        /// <summary>圆心</summary>
        PointF CenterPoint { get; }
    }

    /// <summary>正圆形接口</summary>
    public interface ICircle
    {
        /// <summary>圆心</summary>
        PointF CenterPoint { get; }

        /// <summary>半径</summary>
        float Radius { get; set; }
    }
}
