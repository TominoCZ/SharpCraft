using Newtonsoft.Json;
using System.Collections.Generic;

namespace SharpCraft.model
{
    internal class JsonCube
    {
        [JsonProperty] public float[] from = {0, 0, 0};
        [JsonProperty] public float[] to = {16, 16, 16};
        [JsonProperty] public Dictionary<TextureType, JsonCubeFaceUv> faces;
    }
}