using System;
using System.Runtime.InteropServices;
using OpenTK;

namespace SharpCraft
{
    class Program
    {
        [DllImport("kernel32.dll", SetLastError = true, ExactSpelling = true)]
        static extern bool FreeConsole();

        static void Main(string[] args)
        {
            FreeConsole();

            var wnd = new Window();

            wnd.Run(60);
        }

        class Window : GameWindow
        {
            public Window()
            {
                Title = "Kokot na kaši";
            }

            protected override void OnRenderFrame(FrameEventArgs e)
            {

            }
        }
    }
}
