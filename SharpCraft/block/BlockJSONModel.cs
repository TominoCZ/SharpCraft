using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Newtonsoft.Json;

namespace SharpCraft.block
{
    class BlockJSONModel
    {
        [JsonProperty]
        public Dictionary<TextureType, string> Textures = new Dictionary<TextureType, string>();

        public BlockJSONModel()
        {

        }

        public string GetTextureFromType(TextureType type)
        {
            Textures.TryGetValue(type, out var texturename);

            return texturename;
        }
    }
}
