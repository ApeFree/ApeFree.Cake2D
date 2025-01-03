﻿using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;

namespace ApeFree.Cake2D.Shapes
{
    /// <summary>多边形基类</summary>
    public class PolygonShape : IPolygon
    {
        public PolygonShape(PointF[] points)
        {
            Points = points;
        }

        /// <inheritdoc/>
        public virtual void Scale(float scaling)
        {
            // 缩放多边形的各个顶点坐标
            for (int i = 0; i < Points.Length; i++)
            {
                Points[i] = new PointF(Points[i].X * scaling, Points[i].Y * scaling);
            }
        }

        /// <inheritdoc/>
        public virtual void Offset(float distanceX, float distanceY)
        {
            // 平移多边形的各个顶点坐标
            for (int i = 0; i < Points.Length; i++)
            {
                Points[i] = Points[i].Add(distanceX, distanceY);
            }
        }

        /// <inheritdoc/>
        public virtual void Rotate(PointF centralPoint, float angle)
        {
            // 将多边形的各个顶点坐标绕中心点旋转指定角度
            for (int i = 0; i < Points.Length; i++)
            {
                Points[i] = Math2D.PointAround(centralPoint, Points[i], angle);
            }
        }

        /// <inheritdoc/>
        public virtual bool Contains(PointF point)
        {
            var _points = Points.ToArray();
            // 判断指定点是否在多边形内部
            bool result = false;
            int j = _points.Length - 1;
            for (int i = 0; i < _points.Length; i++)
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

        /// <inheritdoc/>
        public PointF[] Points { get; set; }

        /// <inheritdoc/>
        public virtual RectangleShape GetBounds()
        {
            // 获取多边形的外接矩形
            float left = Points.Min(p => p.X);
            float top = Points.Min(p => p.Y);
            float right = Points.Max(p => p.X);
            float bottom = Points.Max(p => p.Y);
            return new RectangleShape(left, top, right - left, bottom - top);
        }

        /// <inheritdoc/>
        public virtual double CalculatePerimeter()
        {
            var _points = Points.ToArray();
            // 计算多边形的周长
            double perimeter = 0;
            for (int i = 0; i < _points.Length; i++)
            {
                int j = (i + 1) % _points.Length;
                perimeter += Math.Sqrt(Math.Pow(_points[j].X - _points[i].X, 2) + Math.Pow(_points[j].Y - _points[i].Y, 2));
            }
            return perimeter;
        }

        /// <inheritdoc/>
        public virtual double CalculateArea()
        {
            var _points = Points.ToArray();
            // 计算多边形的面积
            double area = 0;
            for (int i = 0; i < _points.Length; i++)
            {
                int j = (i + 1) % _points.Length;
                area += _points[i].X * _points[j].Y - _points[j].X * _points[i].Y;
            }
            return Math.Abs(area / 2);
        }

        /// <inheritdoc/>
        public virtual PointF Centroid
        {
            get
            {
                var _points = Points;

                // 计算多边形的重心
                double cx = 0;
                double cy = 0;
                double area = 0;
                for (int i = 0; i < _points.Length; i++)
                {
                    int j = (i + 1) % _points.Length;
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
