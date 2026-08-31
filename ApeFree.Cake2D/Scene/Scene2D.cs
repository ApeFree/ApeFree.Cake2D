using System;
using System.Collections.Generic;
using System.Drawing;
using ApeFree.Cake2D.Elements;
using ApeFree.Cake2D.Interaction;

namespace ApeFree.Cake2D.Scene
{
    /// <summary>
    /// 二维场景容器，持有视口、全局配置、场景图根图元集合与交互管理器。
    /// </summary>
    public class Scene2D
    {
        public Viewport2D Viewport { get; set; } = new Viewport2D();

        /// <summary>背景色</summary>
        public Color BackgroundColor { get; set; } = Color.White;

        /// <summary>默认线宽（像素）</summary>
        public float DefaultLineWidth { get; set; } = 1.5f;

        /// <summary>是否开启抗锯齿</summary>
        public bool AntiAlias { get; set; } = true;

        /// <summary>顶层根节点图元列表</summary>
        public List<Element2D> Elements { get; } = new List<Element2D>();

        /// <summary>当前视口交互状态机</summary>
        public ViewportInteractionState CurrentState
        {
            get => _currentState;
            set
            {
                _currentState?.OnLeave();
                _currentState = value;
                _currentState?.Initialize(this);
                _currentState?.OnEnter();
            }
        }
        private ViewportInteractionState _currentState;

        /// <summary>图元鼠标交互管理器</summary>
        public ElementInteractionManager InteractionManager { get; }

        /// <summary>请求视图重绘的通知委托/事件</summary>
        public event EventHandler InvalidateRequested;

        public Scene2D()
        {
            InteractionManager = new ElementInteractionManager(this);
            CurrentState = new DefaultPanZoomState();
            Viewport.ViewChanged += (s, e) => RequestInvalidate();
        }

        public void Add(Element2D element)
        {
            if (element != null && !Elements.Contains(element))
            {
                Elements.Add(element);
                RequestInvalidate();
            }
        }

        public void Remove(Element2D element)
        {
            if (element != null && Elements.Remove(element))
            {
                RequestInvalidate();
            }
        }

        public void Clear()
        {
            Elements.Clear();
            RequestInvalidate();
        }

        public void RequestInvalidate()
        {
            InvalidateRequested?.Invoke(this, EventArgs.Empty);
        }
    }
}