using Newtonsoft.Json;
using System.Collections.Generic;

namespace SharpCraft.model
{
    internal class JsonBlockModel
    {
        [JsonProperty] public Dictionary<TextureType, string> Textures;
        [JsonProperty] public JsonCube[] Cubes;

        public string GetTextureFromType(TextureType type)
        {
            Textures.TryGetValue(type, out var textureName);

            return textureName;
        }
    }
}