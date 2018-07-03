using Newtonsoft.Json;
using OpenTK;
using SharpCraft.block;
using SharpCraft.item;
using SharpCraft.render.shader;
using SharpCraft.texture;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using OpenTK.Graphics.OpenGL;
using SharpCraft.util;
using Bitmap = System.Drawing.Bitmap;
using Image = System.Drawing.Image;
#pragma warning disable 618

#pragma warning disable 612

namespace SharpCraft.model
{
    public class JsonModelLoader
    {
        public static int TextureBlocks;
        public static int TextureItems;

        private static readonly ConcurrentDictionary<string, ModelBlock> BlockModels = new ConcurrentDictionary<string, ModelBlock>();
        private static readonly ConcurrentDictionary<string, ModelItem> ItemModels = new ConcurrentDictionary<string, ModelItem>();
        private static readonly Dictionary<string, ModelCustom> CustomModels = new Dictionary<string, ModelCustom>();

        private static JsonModelLoader _instance;

        private static Vector3 _v2, _v3, _v4, _normal;

        private static Shader<ModelBlock> _blockShader;
        private static Shader<ModelItem> _itemShader;

        public JsonModelLoader(Shader<ModelBlock> blockShader, Shader<ModelItem> itemShader)
        {
            if (_instance != null)
                throw new Exception("There can only be one instance of the JsonModelLoader class!");

            _instance = this;

            _blockShader = blockShader;
            _itemShader = itemShader;
        }

        public void LoadBlocks()
        {
            string dir = $"{SharpCraft.Instance.GameFolderDir}/assets/sharpcraft/models/block";

            if (!Directory.Exists(dir))
                return;

            var listOfBlocks = BlockRegistry.AllBlocks();

            List<string> nonDuplicateTextures = new List<string>();

            var blockModels = new ConcurrentDictionary<string, JsonModel>();

            foreach (var block in listOfBlocks)
            {
                var unlocalizedLast = LangUtil.GetUnlocalizedNameLast(block.UnlocalizedName);

                string file = $"{dir}/{unlocalizedLast}.json";

                JsonModel bjm;

                if (!File.Exists(file))
                {
                    var cube = new JsonCube();
                    var uv = new JsonCubeFaceUv { texture = "item" };

                    cube.faces = new Dictionary<Facing, JsonCubeFaceUv>
                    {
                        { Facing.north, uv },
                        { Facing.south, uv },
                        { Facing.west, uv },
                        { Facing.east, uv },
                        { Facing.up, uv },
                        { Facing.down, uv }
                    };

                    cube.from = new[] { 0f, 0, 0 };
                    cube.to = new[] { 16f, 16, 16 };

                    bjm = new JsonModel
                    {
                        cubes = new[]
                        {
                            cube
                        },
                        textures = new Dictionary<string, string> { { "item", "" } }
                    };
                }
                else
                {
                    bjm = FixBlockJson(file);
                }

                blockModels.TryAdd(block.UnlocalizedName, bjm); //save what block is using what model

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
                JsonModel model = pair.Value;

                float[] vertexes = new float[108 * model.cubes.Length];
                float[] normals = new float[108 * model.cubes.Length];
                float[] uvs = new float[72 * model.cubes.Length];

                for (var index = 0; index < model.cubes.Length; index++)
                {
                    var cube = model.cubes[index];

                    CubeModelBuilder.AppendCubeModel(cube, model.textures, textureMapElements, ref vertexes,
                        ref normals, ref uvs, index);
                }

                if (!model.textures.TryGetValue("particle", out var particleTexture))
                    particleTexture = model.textures.Values.ToArray()[SharpCraft.Instance.Random.Next(0, model.textures.Count)];
                if (!model.textures.TryGetValue("item", out var slotTexture))
                {
                    if (model.cubes.Length > 0 && model.cubes[0].faces.TryGetValue(Facing.south, out var uv))
                        slotTexture = model.textures[uv.texture];
                }

                var particleTme = textureMapElements[particleTexture];
                var slotTme = textureMapElements[slotTexture];

                ModelBlock mb = new ModelBlock(slotTme, particleTme, _blockShader, ModelManager.LoadBlockModelToVao(vertexes, normals, uvs));

                BlockModels.TryAdd(name, mb);
            }

            TextureBlocks = id;
        }

        public void LoadItems()
        {
            string dir = $"{SharpCraft.Instance.GameFolderDir}/assets/sharpcraft/models/item";

            var listOfItems = ItemRegistry.AllItems();

            List<string> nonDuplicateTextures = new List<string>();

            var itemModels = new ConcurrentDictionary<string, List<JsonModel>>();

            foreach (var item in listOfItems)
            {
                if (item is ItemBlock)
                    continue;

                string unlocalizedLast = LangUtil.GetUnlocalizedNameLast(item.UnlocalizedName);

                string file = $"{dir}/{unlocalizedLast}.json";

                var models = new List<JsonModel>();

                if (!File.Exists(file))
                {
                    var cube = new JsonCube();
                    var uv = new JsonCubeFaceUv { texture = "item" };

                    cube.faces = new Dictionary<Facing, JsonCubeFaceUv>
                    {
                        { Facing.west, uv }
                    };

                    cube.from = new[] { 8f, 0, 0 };
                    cube.to = new[] { 8f, 16, 16 };

                    var bjm = new JsonModel
                    {
                        cubes = new[]
                        {
                            cube
                        },
                        textures = new Dictionary<string, string> { { "item", unlocalizedLast } }
                    };

                    models.Add(bjm);
                }
                else
                {
                    models.Add(FixBlockJson(file));

                    while (models.Last() is JsonModel jm && !string.IsNullOrEmpty(jm.inherit))
                    {
                        string inhertiedFile = $"{SharpCraft.Instance.GameFolderDir}/assets/sharpcraft/models/{jm.inherit}.json";

                        if (File.Exists(inhertiedFile))
                            models.Add(FixBlockJson(inhertiedFile));
                    }

                    models.Reverse();
                }
                
                itemModels.TryAdd(item.UnlocalizedName, models); //save what block is using what model

                foreach (var jsonModel in models)
                {
                    foreach (var pair in jsonModel.textures) //iterating over the textureMap in the Json model
                    {
                        if (!nonDuplicateTextures.Contains(pair.Value))
                        {
                            nonDuplicateTextures
                                .Add(pair
                                    .Value); //add the current texture name to a list of all textureMap if isn't already there
                        }
                    }
                }
            }

            var textureMapElements = new Dictionary<string, TextureMapElement>(); //each texture name has it's UV values TODO - maybe make a TextureMap class where this could be used

            var id = Stitch(nonDuplicateTextures.ToArray(), 16, textureMapElements); // stitch all textureMap, return the texture ID of the registered texture in VRAM

            //TODO - if json doesn't contain cube model, assume it's a full cube
            foreach (var pair in itemModels) //one model per registered block
            {
                string name = pair.Key;

                TextureMapElement tme = null;

                List<float> vertexes = new List<float>();
                List<float> normals = new List<float>();
                List<float> uvs = new List<float>();

                List<JsonCube> cubes = new List<JsonCube>();

                Dictionary<string, string> textures = new Dictionary<string, string>();

                foreach (var model in pair.Value)
                {
                    foreach (var cube in model.cubes)
                    {
                        cubes.Add(cube);
                    }

                    foreach (var pairtex in model.textures)
                    {
                        try
                        {
                            textures.Add(pairtex.Key, pairtex.Value);
                        }
                        catch
                        {
                            Console.WriteLine("ERROR: Found duplicate texture names in inhertied models!");
                        }
                    }

                    if (model.textures.TryGetValue("item", out var texName))
                        textureMapElements.TryGetValue(texName, out tme);
                }

                foreach (var cube in cubes)
                {
                    CubeModelBuilder.AppendCubeModel(cube, textures, textureMapElements, ref vertexes,
                        ref normals, ref uvs);
                }

                ModelItem mi = new ModelItem(tme, _itemShader, ModelManager.LoadItemModelToVao(vertexes.ToArray(), normals.ToArray(), uvs.ToArray()));

                ItemModels.TryAdd(name, mi);
            }

            TextureItems = id;
        }

        public static bool LoadModel(string path, Shader<ModelCustom> shader)
        {
            string file = $"{SharpCraft.Instance.GameFolderDir}/assets/sharpcraft/models/{path}.json";

            if (!File.Exists(file))
                return false;

            List<string> nonDuplicateTextures = new List<string>();

            List<JsonModel> models = new List<JsonModel> { FixBlockJson(file) };

            while (models.Last() is JsonModel jm && !string.IsNullOrEmpty(jm.inherit))
            {
                string inhertiedFile = $"{SharpCraft.Instance.GameFolderDir}/assets/sharpcraft/models/{jm.inherit}.json";

                if (File.Exists(inhertiedFile))
                    models.Add(FixBlockJson(inhertiedFile));
            }

            models.Reverse();

            foreach (var model in models)
            {
                foreach (var pair in model.textures) //iterating over the textureMap in the Json model
                {
                    if (!nonDuplicateTextures.Contains(pair.Value))
                    {
                        nonDuplicateTextures
                            .Add(pair
                                .Value); //add the current texture name to a list of all textureMap if isn't already there
                    }
                }
            }

            var textureMapElements = new Dictionary<string, TextureMapElement>(); //each texture name has it's UV values TODO - maybe make a TextureMap class where this could be used

            var id = Stitch(nonDuplicateTextures.ToArray(), 16,
                    textureMapElements); //TODO - make the texture size variable

            List<float> vertexes = new List<float>();
            List<float> normals = new List<float>();
            List<float> uvs = new List<float>();

            foreach (var model in models)
            {
                foreach (var cube in model.cubes)
                {
                    CubeModelBuilder.AppendCubeModel(cube, model.textures, textureMapElements, ref vertexes,
                        ref normals, ref uvs);
                }
            }

            var customModel = new ModelCustom(id, ModelManager.LoadModel3ToVao(vertexes.ToArray(), normals.ToArray(), uvs.ToArray()), shader);

            CustomModels.Add(path, customModel);

            return true;
        }
        
        private static JsonModel FixBlockJson(string file)
        {
            var json = File.ReadAllText(file);

            json = json.Replace("elements", "cubes").Replace("faceData", "faces").Replace("textureFacing", "texture").Replace("#", "");

            var parsed = JsonConvert.DeserializeObject<JsonModel>(json);
            
            json = JsonConvert.SerializeObject(parsed, Formatting.Indented);

            File.WriteAllText(file, json);

            return parsed;
        }

        private static int Stitch(string[] allTextures, int textureSize, Dictionary<string, TextureMapElement> sprites)
        {
            Bitmap map = new Bitmap(256, 256);

            int id;

            using (map)
            {
                int countX = 0;
                int countY = 0;

                foreach (var texName in allTextures)
                {
                    Vector2 start = new Vector2((float)countX / map.Width, (float)countY / map.Height);
                    Vector2 end = start + new Vector2((float)textureSize / map.Width, (float)textureSize / map.Height);

                    TextureMapElement mapElement = new TextureMapElement(start, end);

                    WriteBitmap(map, texName, textureSize, ref countX, ref countY);

                    sprites.Add(texName, mapElement);
                }

                map.Save("debug.png");

                var mipmaps = SettingsManager.GetBool("mipmap");

                id = mipmaps ? TextureManager.LoadTextureWithMipMap(map) : TextureManager.LoadTexture(map);
            }

            return id;
        }

        private static void WriteBitmap(Bitmap textureMap, string texPath, int textureSize, ref int countX, ref int countY)
        {
            var file = $"{SharpCraft.Instance.GameFolderDir}/assets/sharpcraft/textures/{texPath}.png";

            Bitmap tex = (File.Exists(file)
                             ? new Bitmap(Image.FromFile(file), textureSize, textureSize)
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
        /// <param name="unlocalizedName"></param>
        /// <returns></returns>
        [Obsolete("Use GetModelForBlock(Block)")]
        public static ModelBlock GetModelForBlock(string unlocalizedName)
        {
            BlockModels.TryGetValue(unlocalizedName, out var model);

            return model;
        }

        /// <summary>
        /// Returns null if block is not registered
        /// </summary>
        /// <param name="block"></param>
        /// <returns></returns>
        public static ModelBlock GetModelForBlock(Block block)
        {
            return GetModelForBlock(block.UnlocalizedName);
        }

        /// <summary>
        /// Returns null if item is not registered
        /// </summary>
        /// <param name="unlocalizedName"></param>
        /// <returns></returns>
        [Obsolete("Use GetModelForItem(Item)")]
        public static ModelItem GetModelForItem(string unlocalizedName)
        {
            ItemModels.TryGetValue(unlocalizedName, out var model);

            return model;
        }

        /// <summary>
        /// Returns null if item is not registered
        /// </summary>
        /// <param name="item"></param>
        /// <returns></returns>
        ///
        public static ModelItem GetModelForItem(Item item)
        {
            return GetModelForItem(item.UnlocalizedName);
        }

        public static ModelCustom GetCustomModel(string path)
        {
            CustomModels.TryGetValue(path, out var model);

            return model;
        }

        public static float[] CalculateNormals(float[] vertices)
        {
            float[] normals = new float[vertices.Length];

            for (int i = 0; i < vertices.Length; i += 12)
            {
                _v2.X = vertices[i + 3];
                _v2.Y = vertices[i + 4];
                _v2.Z = vertices[i + 5];

                _v3.X = vertices[i + 6];
                _v3.Y = vertices[i + 7];
                _v3.Z = vertices[i + 8];

                _v4.X = vertices[i + 9];
                _v4.Y = vertices[i + 10];
                _v4.Z = vertices[i + 11];

                _normal = Vector3.Cross(_v4 - _v2, _v2 - _v3);

                for (int j = 0; j < 4; j++)
                {
                    for (int k = 0; k < 3; k++)
                    {
                        normals[i + j * 3 + k] = _normal[k];
                    }
                }
            }

            return normals;
        }

        public static void Reload()
        {
            var bkp = new Dictionary<string, ModelCustom>(CustomModels);

            Destroy();

            _instance?.LoadBlocks();
            _instance?.LoadItems();

            foreach (var pair in bkp)
            {
                LoadModel(pair.Key, new Shader<ModelCustom>(pair.Value.Shader.ShaderName));
            }
        }

        public static void Destroy()
        {
            TextureManager.DestroyTexture(TextureBlocks);
            TextureManager.DestroyTexture(TextureItems);

            foreach (var customModel in CustomModels.Values)
            {
                TextureManager.DestroyTexture(customModel.TextureID);
                customModel.Destroy();
            }

            foreach (var pair in BlockModels.Values)
            {
                pair.Destroy();
            }

            foreach (var pair in ItemModels.Values)
            {
                pair.Destroy();
            }

            CustomModels.Clear();
            BlockModels.Clear();
            ItemModels.Clear();
        }
    }
}