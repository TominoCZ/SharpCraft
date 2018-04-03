using System;
using System.Diagnostics;
using System.Reflection;
using System.Threading;

namespace SharpCraft.util
{
	public class GameTimer
	{
		private long  lastFrame  = NanoTime();
		private long  lastUpdate = NanoTime();
		private int   maxFps;
		public  bool  infiniteFps = false;
		private int   ups;
		private long  nanosPerFrame;
		private long  nanosPerUpdate;
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
			long tim = NanoTime();
			_partialTicks = (float) ((tim - lastUpdate) / (double) nanosPerUpdate);
			if (_partialTicks >= 1 || _partialTicks < 0)
			{
				CheckUpdate();
				CalcPartialTicks();
			}
		}

		private void Render()
		{
			lock (this)
			{
				CalcPartialTicks();
				renderHook();
			}
		}

		private void Update()
		{
			updateHook();
		}

		public void CheckRender()
		{
			if (infiniteFps) Render();
			else
			{
				long time = NanoTime();
				if (time - lastFrame >= nanosPerFrame)
				{
					lastFrame = time;
					Render();
				}
			}
		}

		public void CheckUpdate()
		{
			double count;

			lock (this)
			{
				long time = NanoTime();
				count = (time - lastUpdate) / (double) nanosPerUpdate;
				if (count > ups * 2)
				{
					count = 1;
					Console.WriteLine("Game lagging really fucking badly man. Get yo shit together");
				}

				if (count < 1)
				{
					return;
				}

				lastUpdate = time;
			}

			if (count > 2) Console.WriteLine(count);
			while (count-- > 1)
			{
				Update();
			}
		}

		public static long NanoTime()
		{
			long nano = 10000L * Stopwatch.GetTimestamp();
			nano /= TimeSpan.TicksPerMillisecond;
			nano *= 100L;
			return nano;
		}

		public float GetPartialTicks()
		{
			return _partialTicks;
		}
	}
}