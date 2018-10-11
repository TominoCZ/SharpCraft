using Newtonsoft.Json;

// ReSharper disable InconsistentNaming

namespace SharpCraft_Client.json
{
    public class JsonBlockStates
    {
        [JsonProperty] public readonly JsonBlockState[] states;
    }
}