using System;
using System.Runtime.InteropServices;
using OpenTK;

namespace SharpCraft
{
    class Program
    {
        static void Main(string[] args)
        {
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
