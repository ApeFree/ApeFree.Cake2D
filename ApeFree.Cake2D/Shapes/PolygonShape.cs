using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;

namespace ApeFree.Cake2D.Shapes
{
    /// <summary>多边形</summary>
    public class PolygonShape : IPolygon
    {
        protected List<PointF> _points = new List<PointF>(); // 存储多边形的各个顶点坐标

        public PolygonShape(IEnumerable<PointF> points)
        {
            _points.AddRange(points);
        }

        public virtual void Scale(float scaling)
        {
            // 缩放多边形的各个顶点坐标
            for (int i = 0; i < _points.Count; i++)
            {
                _points[i] = new Point((int)(_points[i].X * scaling), (int)(_points[i].Y * scaling));
            }
        }

        public virtual void Offset(float distanceX, float distanceY)
        {
            // 平移多边形的各个顶点坐标
            for (int i = 0; i < _points.Count; i++)
            {
                _points[i] = new PointF(_points[i].X + distanceX, _points[i].Y + distanceY);
            }
        }

        public virtual void Rotate(PointF centralPoint, float angle)
        {
            // 将多边形的各个顶点坐标绕中心点旋转指定角度
            for (int i = 0; i < _points.Count; i++)
            {
                float x = ((float)((_points[i].X - centralPoint.X) * Math.Cos(angle) - (_points[i].Y - centralPoint.Y) * Math.Sin(angle) + centralPoint.X));
                float y = ((float)((_points[i].X - centralPoint.X) * Math.Sin(angle) + (_points[i].Y - centralPoint.Y) * Math.Cos(angle) + centralPoint.Y));
                _points[i] = new PointF(x, y);
            }
        }

        public virtual bool Contains(PointF point)
        {
            // 判断指定点是否在多边形内部
            bool result = false;
            int j = _points.Count - 1;
            for (int i = 0; i < _points.Count; i++)
            {
                if ((_points[i].Y < point.Y && _points[j].Y >= point.Y || _points[j].Y < point.Y && _points[i].Y >= point.Y)
                    && (_points[i].X + (point.Y - _points[i].Y) / (_points[j].Y - _points[i].Y) * (_points[j].X - _points[i].X) < point.X))
                {
                    result = !result;
                }
                j = i;
            }
            return result;
        }

        public IEnumerable<PointF> Points
        {
            get { return _points; }
        }

        public virtual RectangleShape GetBounds()
        {
            // 获取多边形的外接矩形
            float left = _points.Min(p => p.X);
            float top = _points.Min(p => p.Y);
            float right = _points.Max(p => p.X);
            float bottom = _points.Max(p => p.Y);
            return new RectangleShape(left, top, right - left, bottom - top);
        }

        public virtual double CalculatePerimeter()
        {
            // 计算多边形的周长
            double perimeter = 0;
            for (int i = 0; i < _points.Count; i++)
            {
                int j = (i + 1) % _points.Count;
                perimeter += Math.Sqrt(Math.Pow(_points[j].X - _points[i].X, 2) + Math.Pow(_points[j].Y - _points[i].Y, 2));
            }
            return perimeter;
        }

        public virtual double CalculateArea()
        {
            // 计算多边形的面积
            double area = 0;
            for (int i = 0; i < _points.Count; i++)
            {
                int j = (i + 1) % _points.Count;
                area += _points[i].X * _points[j].Y - _points[j].X * _points[i].Y;
            }
            return Math.Abs(area / 2);
        }

        public virtual PointF Centroid
        {
            get
            {
                // 计算多边形的重心
                double cx = 0;
                double cy = 0;
                double area = 0;
                for (int i = 0; i < _points.Count; i++)
                {
                    int j = (i + 1) % _points.Count;
                    double temp = _points[i].X * _points[j].Y - _points[j].X * _points[i].Y;
                    area += temp;
                    cx += (double)(_points[i].X + _points[j].X) * temp;
                    cy += (double)(_points[i].Y + _points[j].Y) * temp;
                }
                area /= 2;
                cx /= 6 * area;
                cy /= 6 * area;
                return new Point((int)cx, (int)cy);
            }
        }
    }
}
