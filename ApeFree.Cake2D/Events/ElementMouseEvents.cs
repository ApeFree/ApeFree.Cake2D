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
        public PointF ScreenLocation { get; }
        public PointF WorldLocation { get; }
        public CakeMouseButton Button { get; }
        public int Clicks { get; }
        public int Delta { get; }
        public bool Handled { get; set; }

        public ElementMouseEventArgs(PointF screenLoc, PointF worldLoc, CakeMouseButton button, int clicks, int delta)
        {
            ScreenLocation = screenLoc;
            WorldLocation = worldLoc;
            Button = button;
            Clicks = clicks;
            Delta = delta;
            Handled = false;
        }
    }
}