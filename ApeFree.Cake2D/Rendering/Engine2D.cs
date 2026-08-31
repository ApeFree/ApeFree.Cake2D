using System;
using ApeFree.Cake2D.Elements;
using ApeFree.Cake2D.Scene;

namespace ApeFree.Cake2D.Rendering
{
    /// <summary>
    /// 二维渲染引擎核心。
    /// 读取 Scene2D 场景状态，收集图元绘制指令并执行画家算法排序输出。
    /// </summary>
    public class Engine2D
    {
        public void Render(Scene2D scene, IRenderCanvas canvas)
        {
            if (scene == null || canvas == null) return;

            // 1. 清空背景
            canvas.Clear(scene.BackgroundColor);

            // 2. 构建渲染上下文
            RenderContext2D context = new RenderContext2D(scene);

            // 3. 遍历场景图所有图元收集延迟绘制项
            foreach (Element2D element in scene.Elements)
            {
                element.Render(canvas, context);
            }

            // 4. 画家算法排序：ZIndex 从小到大（先画底层），相同时按 Sequence
            context.RenderItems.Sort((a, b) =>
                a.ZIndex != b.ZIndex
                    ? a.ZIndex.CompareTo(b.ZIndex)
                    : a.Sequence.CompareTo(b.Sequence));

            // 5. 依次执行实际的 Canvas 绘制指令
            foreach (RenderItem2D item in context.RenderItems)
            {
                item.DrawAction(canvas);
            }
        }
    }
}