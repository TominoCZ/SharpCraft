using Newtonsoft.Json;
using System.Collections.Generic;

#pragma warning disable CS0649

namespace SharpCraft.json
{
    internal class JsonModel
    {
        [JsonProperty] public string inherit;
        [JsonProperty] public Dictionary<string, string> textures;
        [JsonProperty] public JsonCube[] cubes;
    }
}