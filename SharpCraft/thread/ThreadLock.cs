using System.Threading;

namespace SharpCraft
{
    class ThreadLock
    {
        private bool locked;

        public delegate void Method();

        private Method method;

        public ThreadLock(Method m)
        {
            method = m;
        }

        public void WaitFor()
        {
            locked = true;
            
            while (locked)
            {
                Thread.Sleep(1);
            }
        }

        public void ExecuteCode()
        {
            method();
            locked = false;
        }
    }
}