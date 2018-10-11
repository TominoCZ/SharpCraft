using Newtonsoft.Json;
using System.Collections.Generic;
using SharpCraft_Client.texture;

namespace SharpCraft_Client.json
{
    public class JsonCube
    {
        [JsonProperty] public float[] from = { 0, 0, 0 };
        [JsonProperty] public float[] to = { 16, 16, 16 };
        [JsonProperty] public JsonModelRotation rotation;
        [JsonProperty] public Dictionary<Facing, JsonCubeFaceUv> faces;
    }
}