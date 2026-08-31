using System;
using ApeFree.Cake2D.Events;
using ApeFree.Cake2D.Rendering;
using ApeFree.Cake2D.Scene;

namespace ApeFree.Cake2D.Interaction
{
    /// <summary>
    /// 视口交互状态抽象基类。
    /// </summary>
    public abstract class ViewportInteractionState
    {
        public Scene2D Scene { get; private set; }
        public Viewport2D Viewport => Scene?.Viewport;

        public virtual void Initialize(Scene2D scene) => Scene = scene;

        public virtual void OnMouseDown(ElementMouseEventArgs e) { }
        public virtual void OnMouseMove(ElementMouseEventArgs e) { }
        public virtual void OnMouseUp(ElementMouseEventArgs e) { }
        public virtual void OnMouseWheel(ElementMouseEventArgs e) { }
        public virtual void OnPaint(IRenderCanvas canvas) { }

        public virtual void OnEnter() { }
        public virtual void OnLeave() { }
    }
}