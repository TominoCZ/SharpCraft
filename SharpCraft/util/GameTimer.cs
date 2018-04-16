using System;
using System.Diagnostics;

namespace SharpCraft.util
{
    public class GameTimer
    {
        private long lastFrame = GetNanoTime();
        private long lastUpdate = GetNanoTime();

        private long lastUpdateTime;

        public bool InfiniteFps = false;

        private int maxFps;
        private int ups;
        private long nanosPerFrame;
        private long nanosPerUpdate;
        private float partialTicks;
        private float lastPartialTicks;

        public Action UpdateHook = () => { };

        public GameTimer(int fps, int ups)
        {
            maxFps = fps;
            this.ups = ups;
            nanosPerFrame = 1_000_000_000L / fps;
            nanosPerUpdate = 1_000_000_000L / ups;
        }

        public void CalculatePartialTicks()
        {
            partialTicks = (GetNanoTime() - lastUpdate + lastUpdateTime) / (float)nanosPerUpdate;
        }

        public bool CanRender()
        {
            if (InfiniteFps)
                return true;

            long time = GetNanoTime();
            if (time - lastFrame < nanosPerFrame)
                return false;

            lastFrame = time;

            return true;
        }

        public bool TryUpdate()
        {
            long time = GetNanoTime();

            if (SharpCraft.Instance.IsPaused)
            {
                lastUpdate = time;

                return false;
            }

            var count = (time - lastUpdate + lastUpdateTime) / (double)nanosPerUpdate;
            if (count > ups * 2)
            {
                count = 1;
                Console.WriteLine("Game lagging really fucking badly man. Get yo shit together");
            }

            if (count < 1)
            {
                return false;
            }

            if (count > 2) Console.WriteLine($"Warning: game is lagging behind, skipping {(long)count - 1} ticks");

            while (count-- >= 1)
            {
                lastUpdate = GetNanoTime();
                UpdateHook();
                lastUpdateTime = GetNanoTime() - lastUpdate;
            }

            return true;
        }

        public static long GetNanoTime()
        {
            return (long)(Stopwatch.GetTimestamp() / (Stopwatch.Frequency / 1000000000D));
        }

        public float GetPartialTicks()
        {
            return SharpCraft.Instance.IsPaused ? lastPartialTicks : (lastPartialTicks = partialTicks);
        }
    }
}