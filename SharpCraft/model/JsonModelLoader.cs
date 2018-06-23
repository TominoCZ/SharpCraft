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
using OpenTK.Graphics.OpenGL;
using Bitmap = System.Drawing.Bitmap;

namespace SharpCraft.model
{
    public class JsonModelLoader
    {
        public static int TEXTURE_BLOCKS;

        private static readonly ConcurrentDictionary<string, ModelBlock> _blockModels = new ConcurrentDictionary<string, ModelBlock>();
        private static readonly Dictionary<string, ModelCustom> _customModels = new Dictionary<string, ModelCustom>();

        private static JsonModelLoader _instance;

        private static Vector3 V2, V3, V4, NORMAL;

        private static Shader<ModelBlock> _blockShader;

        public JsonModelLoader(Shader<ModelBlock> blockShader)
        {
            if (_instance != null)
                throw new Exception("There can only be one instance of the JsonModelLoader class!");

            _instance = this;

            _blockShader = blockShader;

            TEXTURE_BLOCKS = LoadBlocks();
        }

        private static int LoadBlocks()
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

                ModelBlock mb = new ModelBlock(tme, _blockShader, ModelManager.LoadBlockModelToVao(vertexes, normals, uvs));

                _blockModels.TryAdd(name, mb);
            }

            return id;
        }

        public static void LoadModel(string path, Shader<ModelCustom> shader) //TODO 
        {
            string file = $"{SharpCraft.Instance.GameFolderDir}\\SharpCraft_Data\\assets\\models\\{path}.json";

            if (!File.Exists(file))
                return;

            List<string> nonDuplicateTextures = new List<string>();

            JsonBlockModel model = FixBlockJson(file);

            foreach (var pair in model.textures) //iterating over the textureMap in the Json model
            {
                if (!nonDuplicateTextures.Contains(pair.Value))
                {
                    nonDuplicateTextures.Add(pair.Value); //add the current texture name to a list of all textureMap if isn't already there
                }
            }

            var textureMapElements = new Dictionary<string, TextureMapElement>(); //each texture name has it's UV values TODO - maybe make a TextureMap class where this could be used

            var id = Stitch(nonDuplicateTextures.ToArray(), 16, textureMapElements); //TODO - make the texture size variable
            
            float[] vertexes = new float[72 * model.cubes.Length];
            float[] normals = new float[72 * model.cubes.Length];
            float[] uvs = new float[48 * model.cubes.Length];

            for (var index = 0; index < model.cubes.Length; index++)
            {
                var cube = model.cubes[index];

                CubeModelBuilder.AppendCubeModel(cube, model.textures, textureMapElements, ref vertexes,
                    ref normals, ref uvs, index);
            }

            var customModel = new ModelCustom(id, ModelManager.LoadModel3ToVao(vertexes, normals, uvs), shader);

            _customModels.Add(path, customModel);

            //return customModel;
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

        private static JsonBlockModel FixBlockJson(string file)
        {
            var json = File.ReadAllText(file);

            json = json.Replace("elements", "cubes").Replace("faceData", "faces").Replace("textureFacing", "texture").Replace("#", "");

            var parsed = JsonConvert.DeserializeObject<JsonBlockModel>(json);

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

            TEXTURE_BLOCKS = LoadBlocks();

            foreach (var pair in bkp)
            {
                LoadModel(pair.Key, new Shader<ModelCustom>(pair.Value.Shader.ShaderName));
            }
        }

        public static void Destroy()
        {
            TextureManager.DestroyTexture(TEXTURE_BLOCKS);

            foreach (var customModel in _customModels.Values)
            {
                TextureManager.DestroyTexture(customModel.TextureID);
                customModel.Destroy();
            }

            foreach (var pair in _blockModels.Values)
            {
                pair.Destroy();
            }

            _customModels.Clear();
            _blockModels.Clear();
        }
    }
}