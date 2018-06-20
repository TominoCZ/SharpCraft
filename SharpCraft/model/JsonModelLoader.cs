using Newtonsoft.Json;
using OpenTK;
using SharpCraft.block;
using SharpCraft.item;
using SharpCraft.model;
using SharpCraft.render.shader;
using SharpCraft.texture;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using Newtonsoft.Json.Converters;
using Bitmap = System.Drawing.Bitmap;

namespace SharpCraft
{
    public class JsonModelLoader
    {
        public static int TEXTURE_BLOCKS;

        private static readonly Dictionary<string, ModelBlock> _blockModels = new Dictionary<string, ModelBlock>();
        //private static readonly Dictionary<string, ModelItem> _itemModels = new Dictionary<string, ModelItem>(); TODO

        private static JsonModelLoader _instance;

        private static Vector3 V2, V3, V4, NORMAL;

        public JsonModelLoader(Shader<ModelBlock> blockShader)
        {
            if (_instance != null)
                throw new Exception("There can only be one instance of the JsonModelLoader class!");

            _instance = this;

            TEXTURE_BLOCKS = LoadBlocks(blockShader);
        }

        private int LoadBlocks(Shader<ModelBlock> blockShader)
        {
            string dir = $"{SharpCraft.Instance.GameFolderDir}\\SharpCraft_Data\\assets\\models\\block";

            if (!Directory.Exists(dir))
                return 0;

            var listOfBlocks = BlockRegistry.AllBlocks();

            List<string> nonDuplicateTextures = new List<string>();

            var blockModels = new ConcurrentDictionary<string, JsonBlockModel>();

            foreach (var block in listOfBlocks)
            {
                string file = $"{dir}\\{block.UnlocalizedName}.json";

                if (!File.Exists(file))
                    continue;

                JsonBlockModel bjm = FixBlockJson(file);

                string blockName = Path.GetFileNameWithoutExtension(file);

                blockModels.TryAdd(blockName, bjm); //save what block is using what model

                foreach (var pair in bjm.textures) //iterating over the textureMap in the Json model
                {
                    if (!nonDuplicateTextures.Contains(pair.Value))
                    {
                        nonDuplicateTextures.Add(pair.Value); //add the current texture name to a list of all textureMap if isn't already there
                    }
                }
            }

            var textureMapElements = new Dictionary<string, TextureMapElement>(); //each texture name has it's UV values TODO - maybe make a TextureMap class where this could be used

            var id = Stitch(nonDuplicateTextures.ToArray(), 16, textureMapElements); // stitch all textureMap, return the texture ID of the registered texture in VRAM

            //TODO - if json doesn't contain cube model, assume it's a full cube
            foreach (var pair in blockModels) //one model per registered block
            {
                string name = pair.Key;
                JsonBlockModel model = pair.Value;

                float[] vertexes = new float[72 * model.cubes.Length];
                float[] normals = new float[72 * model.cubes.Length];
                float[] uvs = new float[48 * model.cubes.Length];

                for (var index = 0; index < model.cubes.Length; index++)
                {
                    var cube = model.cubes[index];

                    CubeModelBuilder.AppendCubeModel(cube, model.textures, textureMapElements, ref vertexes,
                        ref normals, ref uvs, index);
                }

                string particleTextureName;

                if (!model.textures.TryGetValue("particle", out particleTextureName))
                    particleTextureName = model.textures.Values.ToArray()[SharpCraft.Instance.Random.Next(0, model.textures.Count)];

                var tme = textureMapElements[particleTextureName];

                ModelBlock mb = new ModelBlock(tme, blockShader, ModelManager.LoadBlockModelToVao(vertexes, normals, uvs));

                _blockModels.Add(name, mb);
            }

            return id;
        }

        //TODO - finish + create model from texture if model not found
        private int LoadItems(Shader<ModelBlock> blockShader)
        {
            string dirBlock = $"{SharpCraft.Instance.GameFolderDir}\\SharpCraft_Data\\assets\\models\\block";
            string dirItem = $"{SharpCraft.Instance.GameFolderDir}\\SharpCraft_Data\\assets\\models\\item";

            if (!Directory.Exists(dirBlock) || !Directory.Exists(dirItem))
                return 0;

            // string[] files = Directory.GetFiles(dir); //TODO - ONLY LOAD JSONS FOR REGISTERED BLOCKS!

            var listOfItems = ItemRegistry.AllItems();

            List<string> nonDuplicateTextures = new List<string>();

            var itemModels = new ConcurrentDictionary<string, JsonBlockModel>();

            foreach (var iitem in listOfItems)
            {
                string file = "";

                bool isItemBlock = false;

                if (iitem is ItemBlock itemBlock)
                {
                    isItemBlock = true;
                    file = $"{dirBlock}\\{itemBlock.GetUnlocalizedName()}.json";
                }
                else if (iitem is Item item)
                    file = $"{dirItem}\\{item.GetUnlocalizedName()}.json";

                if (!File.Exists(file))
                    continue;

                JsonBlockModel bjm = JsonConvert.DeserializeObject<JsonBlockModel>(File.ReadAllText(file));

                string itemName = Path.GetFileNameWithoutExtension(file);

                itemModels.TryAdd(itemName, bjm); //save what block is using what model

                if (isItemBlock)
                    continue;

                foreach (var pair in bjm.textures) //iterating over the textureMap in the Json model
                {
                    if (!nonDuplicateTextures.Contains(pair.Value))
                    {
                        nonDuplicateTextures.Add(pair.Value); //add the current texture name to a list of all textureMap if isn't already there
                    }
                }
            }

            var textureMapElements = new Dictionary<string, TextureMapElement>(); //each texture name has it's UV values TODO - maybe make a TextureMap class where this could be used

            var id = Stitch(nonDuplicateTextures.ToArray(), 16, textureMapElements); // stitch all textureMap, return the texture ID of the registered texture in VRAM

            foreach (var pair in itemModels) //one model per registered block
            {
                string name = pair.Key;
                JsonBlockModel model = pair.Value;

                float[] vertexes = new float[72 * model.cubes.Length];
                float[] normals = new float[72 * model.cubes.Length];
                float[] uvs = new float[48 * model.cubes.Length];

                for (var index = 0; index < model.cubes.Length; index++)
                {
                    var cube = model.cubes[index];

                    CubeModelBuilder.AppendCubeModel(cube, model.textures, textureMapElements, ref vertexes, ref normals, ref uvs, index);
                }

                //ModelBlock mb = new ModelBlock(blockShader, ModelManager.LoadBlockModelToVao(vertexes, normals, uvs));

                //_itemModels.Add(name, mb);
            }

            return id;
        }

        private JsonBlockModel FixBlockJson(string file)
        {
            var json = File.ReadAllText(file);

            json = json.Replace("elements", "cubes").Replace("faceData", "faces").Replace("textureFacing", "texture").Replace("#", "");

            var parsed = JsonConvert.DeserializeObject<JsonBlockModel>(json);

            foreach (var variable in parsed.textures.Keys.ToArray())
            {
                var value = parsed.textures[variable];
                parsed.textures[variable] = value.Split('/').Last();
            }

            json = JsonConvert.SerializeObject(parsed, Formatting.Indented);

            File.WriteAllText(file, json);

            return parsed;
        }

        private int Stitch(string[] textures, int textureSize, Dictionary<string, TextureMapElement> sprites)
        {
            Bitmap map = new Bitmap(256, 256);

            int id;

            using (map)
            {
                int countX = 0;
                int countY = 0;

                foreach (var texName in textures)
                {
                    Vector2 start = new Vector2((float)countX / map.Width, (float)countY / map.Height);
                    Vector2 end = start + new Vector2((float)textureSize / map.Width, (float)textureSize / map.Height);

                    TextureMapElement mapElement = new TextureMapElement(start, end);

                    WriteBitmap(map, texName, textureSize, ref countX, ref countY);

                    sprites.Add(texName, mapElement);
                }

                map.Save("debug.png");

                id = TextureManager.LoadTexture(map);
            }

            return id;
        }

        private void WriteBitmap(Bitmap textureMap, string texName, int textureSize, ref int countX, ref int countY)
        {
            var file = $"{SharpCraft.Instance.GameFolderDir}\\SharpCraft_Data\\assets\\Textures\\blocks\\{texName}.png";

            Bitmap tex = (File.Exists(file)
                             ? new Bitmap(Bitmap.FromFile(file), textureSize, textureSize)
                             : null) ?? new Bitmap(TextureManager.TEXTURE_MISSING, textureSize, textureSize);

            using (tex)
            {
                using (Graphics g = Graphics.FromImage(textureMap))
                {
                    g.DrawImage(tex, countX, countY);

                    countX += textureSize;

                    if (countX + textureSize > textureMap.Width)
                    {
                        countX = 0;
                        countY += textureSize;
                    }
                }
            }
        }

        /// <summary>
        /// Returns null if block is not registered
        /// </summary>
        /// <param name="blockName"></param>
        /// <returns></returns>
        public static ModelBlock GetModelForBlock(string blockName)
        {
            _blockModels.TryGetValue(blockName, out var model);

            return model;
        }

        public static float[] CalculateNormals(float[] vertices)
        {
            float[] normals = new float[vertices.Length];

            for (int i = 0; i < vertices.Length; i += 12)
            {
                V2.X = vertices[i + 3];
                V2.Y = vertices[i + 4];
                V2.Z = vertices[i + 5];

                V3.X = vertices[i + 6];
                V3.Y = vertices[i + 7];
                V3.Z = vertices[i + 8];

                V4.X = vertices[i + 9];
                V4.Y = vertices[i + 10];
                V4.Z = vertices[i + 11];

                NORMAL = Vector3.Cross(V4 - V2, V2 - V3);

                for (int j = 0; j < 4; j++)
                {
                    for (int k = 0; k < 3; k++)
                    {
                        normals[i + j * 3 + k] = NORMAL[k];
                    }
                }
            }

            return normals;
        }
    }
}