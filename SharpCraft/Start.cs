using System;
using System.Threading;

namespace SharpCraft
{
    internal class Start
    {
        [STAThread]
        private static void Main(string[] args)
        {
            ThreadPool.SetMaxThreads(1000, 1000);

            using (SharpCraft game = new SharpCraft())
            {
                game.Icon = Resource.Icon;
                game.TargetUpdateFrequency = 20;
                //game.TargetRenderFrequency = 60;
                game.Run();
            }
        }
    }
}