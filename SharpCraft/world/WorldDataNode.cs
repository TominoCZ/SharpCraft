using Newtonsoft.Json;

// ReSharper disable InconsistentNaming

namespace SharpCraft_Client.world
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

        public WorldClient GetWorld(string saveName)
        { //TODO what about servers?

            return new WorldClient(saveName, levelName, seed);
        }
    }
}