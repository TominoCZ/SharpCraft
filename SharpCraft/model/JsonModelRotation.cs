using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace SharpCraft.model
{
    public class JsonModelRotation
    {
        [JsonProperty] public float[] origin = { 8, 8, 8 };
        [JsonProperty, JsonConverter(typeof(StringEnumConverter))] public RotationAxis axis = RotationAxis.y;
        [JsonProperty] public float angle;
    }
}