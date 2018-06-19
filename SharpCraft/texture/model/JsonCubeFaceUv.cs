using Newtonsoft.Json;

namespace SharpCraft.model
{
    internal class JsonCubeFaceUv
    {
        [JsonProperty] public int[] UV;
        [JsonProperty] public string Texture;
    }
}