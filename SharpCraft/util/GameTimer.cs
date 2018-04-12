using System;
using System.Diagnostics;

namespace SharpCraft.util
{
    public class GameTimer
    {
        private long lastFrame = NanoTime();
        private long lastUpdate = NanoTime();

        private long lastUpdateTime;

        private int maxFps;
        public bool infiniteFps = false;
        private int ups;
        private long nanosPerFrame;
        private long nanosPerUpdate;
        private float _partialTicks;

        public Action renderHook = () => { };
        public Action updateHook = () => { };

        public GameTimer(int fps, int ups)
        {
            maxFps = fps;
            this.ups = ups;
            nanosPerFrame = 1_000_000_000L / fps;
            nanosPerUpdate = 1_000_000_000L / ups;
        }

        private void CalcPartialTicks()
        {
            long time = NanoTime();
            var timeDelta = time - lastUpdate + lastUpdateTime;
            _partialTicks = timeDelta / (float)nanosPerUpdate;

            if (timeDelta >= nanosPerUpdate)
            {
                if (!CheckUpdate())
                    return;

                time = NanoTime();
                timeDelta = time - lastUpdate + lastUpdateTime;
                _partialTicks = timeDelta / (float)nanosPerUpdate;
            }
        }

        private void Render()
        {
            CalcPartialTicks();
            renderHook();
        }

        private void Update()
        {
            updateHook();
        }

        public void CheckRender()
        {
            if (infiniteFps)
            {
                Render();
                return;
            }

            long time = NanoTime();
            if (time - lastFrame < nanosPerFrame) return;

            lastFrame = time;
            Render();
        }

        public bool CheckUpdate()
        {
            long time = NanoTime();
            double count = (time - lastUpdate + lastUpdateTime) / (double)nanosPerUpdate;
            if (count > ups * 2)
            {
                count = 1;
                Console.WriteLine("Game lagging really fucking badly man. Get yo shit together");
            }

            if (count < 1)
            {
                return false;
            }

            lastUpdate = time;

            if (count > 2) Console.WriteLine($"Warning: game is lagging behind, updating {(long)count} times ({count})");
            while (count-- > 1)
            {
                time = NanoTime();
                Update();
                lastUpdateTime = NanoTime() - time;
            }

            return true;
        }

        public static long NanoTime()
        {
            return (long)(Stopwatch.GetTimestamp() / (Stopwatch.Frequency / 1000000000.0));
        }

        public float GetPartialTicks()
        {
            return _partialTicks;
        }
    }
}