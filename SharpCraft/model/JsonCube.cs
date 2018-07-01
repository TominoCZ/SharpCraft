using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using System.Collections.Generic;

namespace SharpCraft.model
{
    public class JsonCube
    {
        [JsonProperty] public float[] from = { 0, 0, 0 };
        [JsonProperty] public float[] to = { 16, 16, 16 };
        [JsonProperty] public JsonModelRotation rotation;
        [JsonProperty] public Dictionary<Facing, JsonCubeFaceUv> faces;
    }
}