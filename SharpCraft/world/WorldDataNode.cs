using System;

namespace SharpCraft.world
{
    [Serializable]
    internal class WorldDataNode
    {
        public int Seed { get; }
        public string LevelName { get; }

        public WorldDataNode(World w)
        {
            LevelName = w.LevelName;
            Seed = w.Seed;
        }

        public World GetWorld(string saveName)
        {
            return new World(saveName, LevelName, Seed);
        }
    }
}