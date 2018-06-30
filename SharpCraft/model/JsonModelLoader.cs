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
using System.Net.Mime;
using OpenTK.Graphics.OpenGL;
using Bitmap = System.Drawing.Bitmap;

namespace SharpCraft.model
{
    public class JsonModelLoader
    {
        public static int TEXTURE_BLOCKS;
        public static int TEXTURE_ITEMS;

        private static readonly ConcurrentDictionary<string, ModelBlock> _blockModels = new ConcurrentDictionary<string, ModelBlock>();
        private static readonly ConcurrentDictionary<string, ModelItem> _itemModels = new ConcurrentDictionary<string, ModelItem>();
        private static readonly Dictionary<string, ModelCustom> _customModels = new Dictionary<string, ModelCustom>();

        private static JsonModelLoader _instance;

        private static Vector3 V2, V3, V4, NORMAL;

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
            string dir = $"{SharpCraft.Instance.GameFolderDir}\\SharpCraft_Data\\assets\\models\\block";

            if (!Directory.Exists(dir))
                return;

            var listOfBlocks = BlockRegistry.AllBlocks();

            List<string> nonDuplicateTextures = new List<string>();

            var blockModels = new ConcurrentDictionary<string, JsonModel>();

            foreach (var block in listOfBlocks)
            {
                string file = $"{dir}\\{block.UnlocalizedName}.json";

                if (!File.Exists(file))
                    continue;

                JsonModel bjm = FixBlockJson(file);

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
                JsonModel model = pair.Value;

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

                ModelBlock mb = new ModelBlock(tme, _blockShader, ModelManager.LoadBlockModelToVao(vertexes, normals, uvs));

                _blockModels.TryAdd(name, mb);
            }

            TEXTURE_BLOCKS = id;
        }

        public void LoadItems()
        {
            string dir = $"{SharpCraft.Instance.GameFolderDir}\\SharpCraft_Data\\assets\\models\\item";

            var listOfItems = ItemRegistry.AllItems();

            List<string> nonDuplicateTextures = new List<string>();

            var itemModels = new ConcurrentDictionary<string, List<JsonModel>>();

            foreach (var item in listOfItems)
            {
                if (item is ItemBlock)
                    continue;

                string file = $"{dir}\\{item.UnlocalizedName}.json";

                var models = new List<JsonModel>();

                if (!File.Exists(file))
                {
                    var cube = new JsonCube();
                    var uv = new JsonCubeFaceUv { texture = "item" };

                    cube.faces = new Dictionary<TextureType, JsonCubeFaceUv>
                    {
                        { TextureType.west, uv }
                    };

                    cube.from = new[] { 8f, 0, 0 };
                    cube.to = new[] { 8f, 16, 16 };

                    var bjm = new JsonModel
                    {
                        cubes = new[]
                        {
                            cube
                        },
                        textures = new Dictionary<string, string> { { "item", "items/pick_stone" } }
                    };

                    models.Add(bjm);
                }
                else
                {
                    models.Add(FixBlockJson(file));

                    while (models.Last() is JsonModel jm && !string.IsNullOrEmpty(jm.inherit))
                    {
                        string inhertiedFile = $"{SharpCraft.Instance.GameFolderDir}\\SharpCraft_Data\\assets\\models\\{jm.inherit}.json";

                        if (File.Exists(inhertiedFile))
                            models.Add(FixBlockJson(inhertiedFile));
                    }

                    models.Reverse();
                }

                string itemName = Path.GetFileNameWithoutExtension(file);

                itemModels.TryAdd(itemName, models); //save what block is using what model

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
                    
                    foreach (var (key, value) in model.textures)
                    {
                        try
                        {
                            textures.Add(key, value);
                        }
                        catch
                        {
                            Console.WriteLine($"ERROR: Found duplicate texture names in inhertied models!");
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

                _itemModels.TryAdd(name, mi);
            }

            TEXTURE_ITEMS = id;
        }

        public static bool LoadModel(string path, Shader<ModelCustom> shader) //TODO 
        {
            string file = $"{SharpCraft.Instance.GameFolderDir}\\SharpCraft_Data\\assets\\models\\{path}.json";

            if (!File.Exists(file))
                return false;

            List<string> nonDuplicateTextures = new List<string>();

            List<JsonModel> models = new List<JsonModel> { FixBlockJson(file) };

            while (models.Last() is JsonModel jm && !string.IsNullOrEmpty(jm.inherit))
            {
                string inhertiedFile = $"{SharpCraft.Instance.GameFolderDir}\\SharpCraft_Data\\assets\\models\\{jm.inherit}.json";

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

            //float[] vertexes = new float[72 * model.cubes.Length];
            //float[] normals = new float[72 * model.cubes.Length];
            //float[] uvs = new float[48 * model.cubes.Length];

            foreach (var model in models)
            {
                foreach (var cube in model.cubes)
                {
                    CubeModelBuilder.AppendCubeModel(cube, model.textures, textureMapElements, ref vertexes,
                        ref normals, ref uvs);
                }
            }

            var customModel = new ModelCustom(id, ModelManager.LoadModel3ToVao(vertexes.ToArray(), normals.ToArray(), uvs.ToArray()), shader);

            _customModels.Add(path, customModel);

            return true;
            //return customModel;
        }
        /*
        //TODO - finish + create model from texture if model not found
        private int LoadItems()
        {
            string dirBlock = $"{SharpCraft.Instance.GameFolderDir}\\SharpCraft_Data\\assets\\models\\block";
            string dirItem = $"{SharpCraft.Instance.GameFolderDir}\\SharpCraft_Data\\assets\\models\\item";

            if (!Directory.Exists(dirBlock) || !Directory.Exists(dirItem))
                return 0;

            // string[] files = Directory.GetFiles(dir); //TODO - ONLY LOAD JSONS FOR REGISTERED BLOCKS!

            var listOfItems = ItemRegistry.AllItems();

            List<string> nonDuplicateTextures = new List<string>();

            var itemModels = new ConcurrentDictionary<string, JsonModel>();

            foreach (var iitem in listOfItems)
            {
                string file = "";

                bool isItemBlock = false;

                if (iitem is ItemBlock itemBlock)
                {
                    isItemBlock = true;
                    file = $"{dirBlock}\\{itemBlock.UnlocalizedName}.json";
                }
                else if (iitem is Item item)
                    file = $"{dirItem}\\{item.UnlocalizedName}.json";

                if (!File.Exists(file))
                    continue;

                JsonModel bjm = JsonConvert.DeserializeObject<JsonModel>(File.ReadAllText(file));

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
                JsonModel model = pair.Value;

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
        */
        private static JsonModel FixBlockJson(string file)
        {
            var json = File.ReadAllText(file);

            json = json.Replace("elements", "cubes").Replace("faceData", "faces").Replace("textureFacing", "texture").Replace("#", "");

            var parsed = JsonConvert.DeserializeObject<JsonModel>(json);

            //TODO - optional
            /*
            foreach (var cube in parsed.cubes)
            {
                int faceIndex = 0;

                foreach (var face in cube.faces.Values)
                {
                    face.uv[faceIndex] = Math.Clamp(face.uv[faceIndex], 0, 16);
                    face.uv[faceIndex + 1] = Math.Clamp(face.uv[faceIndex + 1], 0, 16);
                    face.uv[faceIndex + 2] = Math.Clamp(face.uv[faceIndex + 2], 0, 16);
                    face.uv[faceIndex + 3] = Math.Clamp(face.uv[faceIndex + 3], 0, 16);

                    faceIndex++;
                }
            }*/

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

                id = TextureManager.LoadTexture(map);
            }

            return id;
        }

        private static void WriteBitmap(Bitmap textureMap, string texPath, int textureSize, ref int countX, ref int countY)
        {
            var file = $"{SharpCraft.Instance.GameFolderDir}\\SharpCraft_Data\\assets\\textures\\{texPath}.png";

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

        public static ModelItem GetModelForItem(string itemName)
        {
            _itemModels.TryGetValue(itemName, out var model);

            return model;
        }

        public static ModelCustom GetCustomModel(string path)
        {
            _customModels.TryGetValue(path, out var model);

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

        public static void Reload()
        {
            var bkp = new Dictionary<string, ModelCustom>(_customModels);

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
            TextureManager.DestroyTexture(TEXTURE_BLOCKS);
            TextureManager.DestroyTexture(TEXTURE_ITEMS);

            foreach (var customModel in _customModels.Values)
            {
                TextureManager.DestroyTexture(customModel.TextureID);
                customModel.Destroy();
            }

            foreach (var pair in _blockModels.Values)
            {
                pair.Destroy();
            }

            foreach (var pair in _itemModels.Values)
            {
                pair.Destroy();
            }

            _customModels.Clear();
            _blockModels.Clear();
            _itemModels.Clear();
        }
    }
}