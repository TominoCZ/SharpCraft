using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

namespace SharpCraft
{
    static class ThreadPool
    {
        private static List<Worker> _workers;
        private static List<Worker.Method> _queue;

        private static List<Worker> _workersHigh;
        private static List<Worker.Method> _queueHigh;

        private static Thread _queueThread;

        static ThreadPool()
        {
            _workers = new List<Worker>();
            _queue = new List<Worker.Method>();

            _workersHigh = new List<Worker>();
            _queueHigh = new List<Worker.Method>();

            int threads = Math.Max(Environment.ProcessorCount * 2 / 3, 1);

            for (int i = 0; i < threads; i++)
            {
                _workers.Add(new Worker());
                _workersHigh.Add(new Worker());
            }

            _queueThread = new Thread(manageTaskQueue);
            _queueThread.IsBackground = true;
            _queueThread.Start();
        }

        public static void ScheduleTask(bool highPriority, Worker.Method f)
        {
            var worker = getAvailableWorker(highPriority);

            if (worker != null)
                worker.RunTask(f);
            else
            {
                lock (highPriority ? _queueHigh : _queue)
                {
                    if (highPriority)
                        _queueHigh.Add(f);
                    else
                        _queue.Add(f);
                }
            }
        }

        private static void manageTaskQueue()
        {
            while (true)
            {
                lock (_queueHigh)
                {
                    var workerHigh = getAvailableWorker(true);

                    if (workerHigh != null && _queueHigh.Count > 0)
                    {
                        var func = _queueHigh.First();

                        if (func != null)
                        {
                            workerHigh.RunTask(func);

                            _queueHigh.Remove(func);
                        }
                    }
                }

                lock (_queue)
                {
                    var worker = getAvailableWorker(false);

                    if (worker != null && _queue.Count > 0)
                    {
                        var func = _queue.First();

                        if (func != null)
                        {
                            worker.RunTask(func);

                            _queue.Remove(func);
                        }
                    }
                }

                Thread.Sleep(1);
            }
        }

        private static Worker getAvailableWorker(bool highPriority)
        {
            var workers = highPriority ? _workersHigh : _workers;

            for (int i = 0; i < workers.Count; i++)
            {
                var worker = workers[i];

                if (worker.Ready)
                    return worker;
            }

            return null;
        }
    }

    class Worker
    {
        private Thread _thread;
        private Method _task;

        public delegate void Method();

        public bool Ready { get; private set; }

        public Worker()
        {
            _thread = new Thread(run);
            _thread.IsBackground = true;
            _thread.Start();

            Ready = true;
        }

        private void run()
        {
            while (true)
            {
                if (_task != null)
                {
                    _task();

                    _task = null;
                    Ready = true;
                }

                Thread.Sleep(1);
            }
        }

        public void RunTask(Method workerFunc)
        {
            Ready = false;

            _task = workerFunc;
        }
    }
}