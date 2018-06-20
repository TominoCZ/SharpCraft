using Newtonsoft.Json;
using System.Collections.Generic;

namespace SharpCraft.model
{
    internal class JsonBlockModel
    {
        [JsonProperty] public Dictionary<string, string> textures;
        [JsonProperty] public JsonCube[] cubes;
    }
}