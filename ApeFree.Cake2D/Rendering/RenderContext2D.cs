using System;
using System.Collections.Generic;
using System.Drawing;
using ApeFree.Cake2D.Elements;
using ApeFree.Cake2D.Math;
using ApeFree.Cake2D.Scene;

namespace ApeFree.Cake2D.Rendering
{
    /// <summary>
    /// 渲染上下文，提供世界坐标到屏幕投影的快速工具，并支持视口视锥剔除和仿射变换矩阵合成。
    /// </summary>
    public class RenderContext2D
    {
        private int _sequenceCounter = 0;

        public Scene2D Scene { get; }
        public Viewport2D Viewport => Scene.Viewport;

        /// <summary>当前帧的世界到屏幕投影变换矩阵</summary>
        public Matrix3x2 WorldToScreenMatrix { get; }

        /// <summary>当前视口在世界坐标系下的包围矩形 (AABB)</summary>
        public RectangleF WorldVisibleBounds { get; }

        /// <summary>是否启用视口视锥剔除</summary>
        public bool EnableCulling => Scene.EnableFrustumCulling;

        /// <summary>当前帧收集的延迟绘制项队列（用于向后兼容第三方自定义图元的 Submit 调用）</summary>
        public List<RenderItem2D> RenderItems { get; } = new List<RenderItem2D>();

        public RenderContext2D(Scene2D scene)
        {
            Scene = scene ?? throw new ArgumentNullException(nameof(scene));
            WorldToScreenMatrix = Viewport.GetWorldToScreenMatrix();
            WorldVisibleBounds = Viewport.GetCurrentWorldViewRect();
        }

        /// <summary>获取图元本地坐标到屏幕像素坐标的复合仿射变换矩阵</summary>
        public Matrix3x2 GetElementToScreenMatrix(Element2D element)
        {
            if (element == null) return WorldToScreenMatrix;
            return element.GetWorldMatrix() * WorldToScreenMatrix;
        }

        /// <summary>测试指定图元是否在视口可见区域内（用于视锥剔除）</summary>
        public bool IsVisibleInViewport(Element2D element)
        {
            if (element == null) return false;
            // 若节点含有子节点，使用其包含子树的联合包围盒判定，防止误杀视口内的子节点
            RectangleF bounds = element.Children.Count > 0 ? element.GetSubtreeWorldBounds() : element.GetWorldBounds();
            // 如果包围盒为空（例如无限辅助线 AxisElement/GridElement），默认认为可见
            if (bounds.IsEmpty || bounds.Width <= 0f || bounds.Height <= 0f) return true;
            return WorldVisibleBounds.IntersectsWith(bounds);
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

        /// <summary>提交一个延迟绘制动作到全局渲染队列（向后兼容）</summary>
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
