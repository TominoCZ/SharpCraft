using System.Collections.Generic;
using Newtonsoft.Json;
using SharpCraft.texture;

namespace SharpCraft.json
{
    public class JsonCube
    {
        [JsonProperty] public float[] from = { 0, 0, 0 };
        [JsonProperty] public float[] to = { 16, 16, 16 };
        [JsonProperty] public JsonModelRotation rotation;
        [JsonProperty] public Dictionary<Facing, JsonCubeFaceUv> faces;
    }
}