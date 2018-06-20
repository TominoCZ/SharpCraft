using Newtonsoft.Json;

namespace SharpCraft.model
{
    internal class JsonCubeFaceUv
    {
        [JsonProperty] public float[] uv = {0, 0, 16, 16};
        [JsonProperty] public string texture;
    }
}