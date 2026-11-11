using System;
using System.Collections.Generic;
using ApeFree.Cake2D.Elements;
using ApeFree.Cake2D.Scene;

namespace ApeFree.Cake2D.Rendering
{
    /// <summary>
    /// 二维渲染引擎核心。
    /// 读取 Scene2D 场景状态，按图元层级执行视锥剔除与即时直通渲染，同时兼容旧图元的延迟提交机制。
    /// </summary>
    public class Engine2D
    {
        // 复用队列缓冲区，避免每帧产生堆对象垃圾分配
        private readonly List<Element2D> _renderQueue = new List<Element2D>();

        public void Render(Scene2D scene, IRenderCanvas canvas)
        {
            if (scene == null || canvas == null) return;

            // 1. 清空背景
            canvas.Clear(scene.BackgroundColor);

            // 2. 构建渲染上下文（计算并缓存视口与屏幕投影矩阵、视口包围盒）
            RenderContext2D context = new RenderContext2D(scene);

            // 3. 收集顶层根节点并按 ZIndex 从小到大（先画底层）排序
            _renderQueue.Clear();
            _renderQueue.AddRange(scene.Elements);
            _renderQueue.Sort((a, b) => a.ZIndex.CompareTo(b.ZIndex));

            // 4. 即时直通渲染遍历（结合视锥剔除与树状层级）
            for (int i = 0; i < _renderQueue.Count; i++)
            {
                _renderQueue[i].Render(canvas, context);
            }

            // 5. 向后兼容：如果有外部自定义图元调用了 context.Submit，处理遗留延迟项
            if (context.RenderItems.Count > 0)
            {
                context.RenderItems.Sort((a, b) =>
                    a.ZIndex != b.ZIndex
                        ? a.ZIndex.CompareTo(b.ZIndex)
                        : a.Sequence.CompareTo(b.Sequence));

                for (int i = 0; i < context.RenderItems.Count; i++)
                {
                    context.RenderItems[i].DrawAction(canvas);
                }
            }
        }
    }
}
