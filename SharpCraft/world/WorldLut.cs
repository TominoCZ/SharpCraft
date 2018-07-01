using SharpCraft.block;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;

namespace SharpCraft.world
{
    [Serializable]
    public class WorldLut
    {
        private ConcurrentDictionary<short, string> _forward;
        private ConcurrentDictionary<string, short> _backward;

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

        public void Load(Dictionary<short, string> lut)
        {
            _forward = new ConcurrentDictionary<short, string>(lut);
            _backward = new ConcurrentDictionary<string, short>(lut.ToDictionary(x => x.Value, x => x.Key));
        }

        public Dictionary<short, string> GetTable()
        {
            return _forward.ToDictionary(x => x.Key, x => x.Value);
        }
    }
}