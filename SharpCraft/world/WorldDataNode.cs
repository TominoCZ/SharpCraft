using System;

namespace SharpCraft.world
{
    [Serializable]
    internal class WorldDataNode
    {
        public int seed { get; }
        public string levelName { get; }

        public WorldDataNode(World w)
        {
            levelName = w.LevelName;
            seed = w.Seed;
        }
    }
}