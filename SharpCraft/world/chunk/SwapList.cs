using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace SharpCraft.world.chunk
{
    internal class SwapList<T>
    {
        private List<T> _primary = new List<T>();
        private List<T> _backup = new List<T>();
        internal bool Building;
        private readonly Action<T> _action;
        private readonly Comparison<T> _comparison;

        internal SwapList(Action<T> action, Comparison<T> comparison)
        {
            _action = action;
            _comparison = comparison;
        }

        internal void Exec(bool parallel)
        {
            _primary.Sort(_comparison);
            if (parallel) Parallel.ForEach(_primary, _action);
            else _primary.ForEach(_action);
            _primary.Clear();

            List<T> ch = _primary;
            _primary = _backup;
            _backup = ch;

            if (_primary.Count > 0) Exec(parallel);

            Building = false;
        }

        internal void Add(T t) => (Building ? _backup : _primary).Add(t);

        public void Remove(T pos)
        {
            _primary.Remove(pos);
            _backup.Remove(pos);
        }

        public void Clear()
        {
            _primary.Clear();
            _backup.Clear();
        }
    }
}