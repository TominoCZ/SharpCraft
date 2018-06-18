using System.Collections.Generic;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace SharpCraft.model
{
    class BlockJSONModel
    {
        [JsonProperty] public Dictionary<TextureType, string> Textures;
        [JsonProperty] public JsonCube[] Cubes;

        public string GetTextureFromType(TextureType type)
        {
            Textures.TryGetValue(type, out var textureName);

            return textureName;
        }
    }

    class JsonCube
    {
        [JsonProperty] public int[] From;
        [JsonProperty] public int[] To;
        [JsonProperty] public Dictionary<TextureType, JsonCubeFaceUv> Faces;
    }

    class JsonCubeFaceUv
    {
        [JsonProperty] public int[] UV;
        [JsonProperty] public string Texture;
    }
}
