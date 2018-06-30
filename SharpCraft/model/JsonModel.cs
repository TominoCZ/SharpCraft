using Newtonsoft.Json;
using System.Collections.Generic;

namespace SharpCraft.model
{
    internal class JsonModel
    {
        [JsonProperty] public string inherit;
        [JsonProperty] public Dictionary<string, string> textures;
        [JsonProperty] public JsonCube[] cubes;
    }
}