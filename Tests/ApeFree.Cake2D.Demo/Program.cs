using System;
using System.Windows.Forms;

namespace ApeFree.Cake2D.Demo
{
    internal static class Program
    {
        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new MainDemoForm());
        }
    }
}