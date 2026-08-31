using System;
using System.Collections.Generic;
using System.Drawing;
using ApeFree.Cake2D.Scene;

namespace ApeFree.Cake2D.Rendering
{
    /// <summary>
    /// 渲染上下文，提供世界坐标到屏幕投影的快速工具，并收集当前帧的所有绘制指令。
    /// </summary>
    public class RenderContext2D
    {
        private int _sequenceCounter = 0;

        public Scene2D Scene { get; }
        public Viewport2D Viewport => Scene.Viewport;

        /// <summary>当前帧收集的绘制项队列</summary>
        public List<RenderItem2D> RenderItems { get; } = new List<RenderItem2D>();

        public RenderContext2D(Scene2D scene)
        {
            Scene = scene ?? throw new ArgumentNullException(nameof(scene));
        }

        /// <summary>将单个世界坐标点投影到屏幕像素坐标</summary>
        public PointF Project(PointF worldPoint) => Viewport.WorldToScreen(worldPoint);

        /// <summary>批量将世界坐标点数组投影到屏幕像素坐标</summary>
        public PointF[] Project(PointF[] worldPoints)
        {
            if (worldPoints == null) return new PointF[0];
            PointF[] result = new PointF[worldPoints.Length];
            for (int i = 0; i < worldPoints.Length; i++)
            {
                result[i] = Viewport.WorldToScreen(worldPoints[i]);
            }
            return result;
        }

        /// <summary>将世界长度（如半径、物理尺寸）投影为屏幕像素长度</summary>
        public float ProjectLength(float worldLength) => System.Math.Abs(worldLength * Viewport.PixelsPerUnit);

        /// <summary>解析图元的有效线宽</summary>
        public float ResolveLineWidth(float elementLineWidth)
        {
            return elementLineWidth > 0f ? elementLineWidth : Scene.DefaultLineWidth;
        }

        /// <summary>提交一个延迟绘制动作到全局渲染队列</summary>
        public void Submit(int zIndex, Action<IRenderCanvas> drawAction)
        {
            if (drawAction == null) return;
            RenderItems.Add(new RenderItem2D
            {
                ZIndex = zIndex,
                Sequence = _sequenceCounter++,
                DrawAction = drawAction
            });
        }
    }
}