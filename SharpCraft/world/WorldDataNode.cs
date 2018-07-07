using Newtonsoft.Json;

// ReSharper disable InconsistentNaming

namespace SharpCraft.world
{
    public class WorldDataNode
    {
        [JsonProperty] public readonly string seed;
        [JsonProperty] public readonly string levelName;

        public WorldDataNode()
        {
        }

        public WorldDataNode(World w)
        {
            levelName = w.LevelName;
            seed = w.Seed;
        }

        public World GetWorld(string saveName)
        {
            return new World(saveName, levelName, seed);
        }
    }
}