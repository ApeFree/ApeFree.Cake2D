using System;
using System.Drawing;
using System.Windows.Forms;
using ApeFree.Cake2D.Scene;

namespace ApeFree.Cake2D.Gdi.Controls
{
    /// <summary>
    /// 内置 Cake2D 渲染视口的 WinForms 窗体基类。
    /// </summary>
    public class Cake2DCanvasForm : Form
    {
        public Cake2DControl CakeControl { get; }
        public Scene2D Scene => CakeControl.Scene;

        public Cake2DCanvasForm()
        {
            CakeControl = new Cake2DControl
            {
                Dock = DockStyle.Fill
            };
            Controls.Add(CakeControl);
        }
    }
}