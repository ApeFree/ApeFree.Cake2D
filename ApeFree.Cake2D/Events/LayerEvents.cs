using ApeFree.Cake2D.Events;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ApeFree.Cake2D
{
    public partial class Layer<TStyle>
    {
        /// <summary>当鼠标在控件上移动时发生。</summary>
        [Description("当鼠标在控件上移动时发生。")]
        public event MouseEventHandler MouseMove;

        /// <summary>当鼠标在控件上释放时发生。</summary>
        [Description("当鼠标在控件上释放时发生。")]
        public event MouseEventHandler MouseUp;

        /// <summary>当鼠标在控件上按下时发生。</summary>
        [Description("当鼠标在控件上按下时发生。")]
        public event MouseEventHandler MouseDown;

        /// <summary>当鼠标滚轮滚动时发生。</summary>
        [Description("当鼠标滚轮滚动时发生。")]
        public event MouseEventHandler MouseWheel;

        /// <summary>当鼠标离开控件时发生。</summary>
        [Description("当鼠标离开控件时发生。")]
        public event EventHandler MouseLeave;

        /// <summary>当鼠标进入控件时发生。</summary>
        [Description("当鼠标进入控件时发生。")]
        public event EventHandler MouseEnter;

        /// <summary>当用户单击控件时发生。</summary>
        [Description("当用户单击控件时发生。")]
        public event EventHandler Click;

        /// <summary>当用户双击控件时发生。</summary>
        [Description("当用户双击控件时发生。")]
        public event EventHandler DoubleClick;

        /// <summary>当控件的 Visible 属性值更改时发生。</summary>
        [Description("当控件的 Visible 属性值更改时发生。")]
        public event EventHandler VisibleChanged;

        /// <summary>当控件的 Enabled 属性值更改时发生。</summary>
        [Description("当控件的 Enabled 属性值更改时发生。")]
        public event EventHandler EnabledChanged;


        /// <summary>触发 MouseMove 事件</summary>
        /// <param name="sender">事件源</param>
        /// <param name="e">事件参数</param>
        public void RaiseMouseMove(object sender, MouseEventArgs e)
        {
            MouseMove?.Invoke(sender, e);
        }

        /// <summary>触发 MouseUp 事件</summary>
        /// <param name="sender">事件源</param>
        /// <param name="e">事件参数</param>
        public void RaiseMouseUp(object sender, MouseEventArgs e)
        {
            MouseUp?.Invoke(sender, e);
        }

        /// <summary>触发 MouseDown 事件</summary>
        /// <param name="sender">事件源</param>
        /// <param name="e">事件参数</param>
        public void RaiseMouseDown(object sender, MouseEventArgs e)
        {
            MouseDown?.Invoke(sender, e);
        }

        /// <summary>触发 MouseWheel 事件</summary>
        /// <param name="sender">事件源</param>
        /// <param name="e">事件参数</param>
        public void RaiseMouseWheel(object sender, MouseEventArgs e)
        {
            MouseWheel?.Invoke(sender, e);
        }

        /// <summary>触发 MouseLeave 事件</summary>
        /// <param name="sender">事件源</param>
        /// <param name="e">事件参数</param>
        public void RaiseMouseLeave(object sender, EventArgs e)
        {
            MouseLeave?.Invoke(sender, e);
        }

        /// <summary>触发 MouseEnter 事件</summary>
        /// <param name="sender">事件源</param>
        /// <param name="e">事件参数</param>
        public void RaiseMouseEnter(object sender, EventArgs e)
        {
            MouseEnter?.Invoke(sender, e);
        }

        /// <summary>触发 Click 事件</summary>
        /// <param name="sender">事件源</param>
        /// <param name="e">事件参数</param>
        public void RaiseClick(object sender, EventArgs e)
        {
            Click?.Invoke(sender, e);
        }

        /// <summary>触发 DoubleClick 事件</summary>
        /// <param name="sender">事件源</param>
        /// <param name="e">事件参数</param>
        public void RaiseDoubleClick(object sender, EventArgs e)
        {
            DoubleClick?.Invoke(sender, e);
        }

        /// <summary>触发 VisibleChanged 事件</summary>
        /// <param name="sender">事件源</param>
        /// <param name="e">事件参数</param>
        public void RaiseVisibleChanged(object sender, EventArgs e)
        {
            VisibleChanged?.Invoke(sender, e);
        }

        /// <summary>触发 EnabledChanged 事件</summary>
        /// <param name="sender">事件源</param>
        /// <param name="e">事件参数</param>
        public void RaiseEnabledChanged(object sender, EventArgs e)
        {
            EnabledChanged?.Invoke(sender, e);
        }
    }

    public partial class Layer<TStyle, TShape>
    {

    }
}

namespace ApeFree.Cake2D.Events
{
    public delegate void MouseEventHandler(object sender, MouseEventArgs e);

    public enum MouseButtons
    {
        Left = 0x100000,
        None = 0x0,
        Right = 0x200000,
        Middle = 0x400000,
        XButton1 = 0x800000,
        XButton2 = 0x1000000
    }

    public class MouseEventArgs : EventArgs
    {
        private readonly MouseButtons button;

        private readonly int clicks;

        private readonly int x;

        private readonly int y;

        private readonly int delta;

        public MouseButtons Button => button;

        public int Clicks => clicks;

        public int X => x;

        public int Y => y;

        public int Delta => delta;

        public Point Location => new Point(x, y);

        public MouseEventArgs(MouseButtons button, int clicks, int x, int y, int delta)
        {
            this.button = button;
            this.clicks = clicks;
            this.x = x;
            this.y = y;
            this.delta = delta;
        }
    }

}
