using System;
using System.Collections.Generic;

namespace SharpCraft
{
    [Serializable]
    internal class WorldDataNode
    {
        public int seed { get; }
        public string levelName { get; }
        public WorldDataNode(World w)
        {
            levelName = w.levelName;
            seed = w.seed;
        }
    }
}