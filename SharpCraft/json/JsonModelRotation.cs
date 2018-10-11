using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using SharpCraft_Client.model;

namespace SharpCraft_Client.json
{
    public class JsonModelRotation
    {
        [JsonProperty] public float[] origin = { 8, 8, 8 };
        [JsonProperty, JsonConverter(typeof(StringEnumConverter))] public RotationAxis axis = RotationAxis.y;
        [JsonProperty] public float angle;
    }
}