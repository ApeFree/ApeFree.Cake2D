using System;
using System.Drawing;

namespace ApeFree.Cake2D.Events
{
    [Flags]
    public enum CakeMouseButton
    {
        None = 0,
        Left = 1,
        Right = 2,
        Middle = 4
    }

    public class ElementMouseEventArgs : EventArgs
    {
        private bool? _handled;

        public PointF ScreenLocation { get; }
        public PointF WorldLocation { get; }
        public CakeMouseButton Button { get; }
        public int Clicks { get; }
        public int Delta { get; }

        /// <summary>
        /// 事件是否已被消费或中止冒泡。
        /// 默认未显式设置时为 false（允许沿父节点链正常冒泡）；
        /// 若设为 true，则中止向上冒泡；
        /// 若设为 false，则显式允许穿透到底层视口平移。
        /// </summary>
        public bool Handled
        {
            get => _handled ?? false;
            set => _handled = value;
        }

        /// <summary>获取业务代码是否显式指定了 Handled 属性（null 表示未显式改动）</summary>
        public bool? ExplicitHandled => _handled;

        public ElementMouseEventArgs(PointF screenLoc, PointF worldLoc, CakeMouseButton button, int clicks, int delta)
        {
            ScreenLocation = screenLoc;
            WorldLocation = worldLoc;
            Button = button;
            Clicks = clicks;
            Delta = delta;
            _handled = null;
        }
    }
}