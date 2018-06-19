using Newtonsoft.Json;
using System.Collections.Generic;

namespace SharpCraft.model
{
    internal class JsonCube
    {
        [JsonProperty] public int[] From;
        [JsonProperty] public int[] To;
        [JsonProperty] public Dictionary<TextureType, JsonCubeFaceUv> Faces;
    }
}