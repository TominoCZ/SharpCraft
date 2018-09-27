using Newtonsoft.Json;

// ReSharper disable InconsistentNaming

namespace SharpCraft.json
{
    public class JsonBlockStates
    {
        [JsonProperty] public readonly JsonBlockState[] states;
    }

    public class JsonBlockState
    {
        [JsonProperty] public readonly string model;
    }
}