using SharpCraft.block;
using System;
using System.Collections.Concurrent;

namespace SharpCraft.world
{
    [Serializable]
    public class WorldLut
    {
        private readonly ConcurrentDictionary<short, string> _forward;
        private readonly ConcurrentDictionary<string, short> _backward;

        public WorldLut()
        {
            _forward = new ConcurrentDictionary<short, string>();
            _backward = new ConcurrentDictionary<string, short>();
        }

        public void Put(string unlocalizedName)
        {
            short id = (short)_forward.Count;

            if (_forward.Keys.Contains(id))
                return;

            _forward.TryAdd(id, unlocalizedName);
            _backward.TryAdd(unlocalizedName, id);
        }

        public short Translate(string unlocalizedName)
        {
            return _backward[unlocalizedName];
        }

        public string Translate(short id)
        {
            if (_forward.TryGetValue(id, out var name))
                return name;

            return BlockRegistry.GetBlock<BlockAir>().UnlocalizedName;
        }
    }
}