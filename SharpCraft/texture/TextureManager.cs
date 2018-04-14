using OpenTK;
using OpenTK.Graphics.OpenGL;
using SharpCraft.block;
using SharpCraft.util;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using SharpCraft.gui;
using Bitmap = System.Drawing.Bitmap;
using Image = System.Drawing.Image;
using Rectangle = System.Drawing.Rectangle;

namespace SharpCraft.texture
{
    internal class TextureManager
    {
        private static Dictionary<EnumBlock, TextureBlockUV> _blockUVs = new Dictionary<EnumBlock, TextureBlockUV>();

        private static List<int> _allTextures = new List<int>();

        private static Bitmap TEXTURE_MISSING = CreateMissingTexture();

        public static Texture TEXTURE_DESTROY_PROGRESS;
        public static Texture TEXTURE_BLOCKS;
        public static Texture TEXTURE_GUI_WIDGETS;
        public static Texture TEXTURE_TEXT;

        public static void LoadTextures()
        {
            StitchTextures();

            TEXTURE_GUI_WIDGETS = LoadTexture("gui/widgets");
            TEXTURE_DESTROY_PROGRESS = LoadTexture("blocks/destroy_progress");
            TEXTURE_TEXT = LoadTexture("font/default");

            FontManager.LoadCharacters(TEXTURE_TEXT, "font/default");
        }

        private static void StitchTextures()
        {
            var bmp = CreateTextureMap("blocks");
            var id = LoadTexture(bmp);

            TEXTURE_BLOCKS = new Texture(id, bmp.Size);

            bmp.Dispose();
        }

        private static Bitmap CreateTextureMap(string folder)
        {
            Bitmap map = new Bitmap(256, 256);

            var blocks = Enum.GetValues(typeof(EnumBlock));

            var dir = $"SharpCraft_Data/assets/textures/{folder}";

            var files = new string[0];
            if (Directory.Exists(dir))
                files = Directory.GetFiles(dir, "*.png");

            for (int i = 0; i < files.Length; i++)
            {
                files[i] = Path.GetFileNameWithoutExtension(files[i]).ToLower();
            }

            int countX = 0;
            int countY = 0;

            var size = new Vector2(16f / map.Size.Width, 16f / map.Size.Height);

            using (map)
            {
                foreach (EnumBlock block in blocks)
                {
                    var texName = block.ToString().ToLower();
                    var uvs = new TextureBlockUV();

                    if (block == EnumBlock.MISSING)
                    {
                        var pos = new Vector2(countX * size.X, countY * size.Y);
                        var end = pos + size;

                        uvs.fill(pos, end);

                        DrawToBitmap(map, countX * 16, countY * 16, TEXTURE_MISSING);

                        countX++;

                        //check
                        if (countX * 16 >= map.Size.Width)
                        {
                            countX = 0;
                            countY++;
                        }
                    }
                    else if (ContainsContaining(files, texName) > 0)
                    {
                        //found

                        //found texture for all 6 sides
                        if (files.Contains(texName))
                        {
                            var pos = new Vector2(countX * size.X, countY * size.Y);
                            var end = pos + size;

                            uvs.fill(pos, end);

                            DrawToBitmap(map, countX * 16, countY * 16, $"{dir}/{texName}.png");

                            countX++;

                            //check
                            if (countX * 16 >= map.Size.Width)
                            {
                                countX = 0;
                                countY++;
                            }
                        }

                        //found texture for the 4 sides
                        if (files.Contains($"{texName}_side"))
                        {
                            var pos = new Vector2(countX * size.X, countY * size.Y);
                            var end = pos + size;

                            uvs.setUVForSide(FaceSides.North, pos, end);
                            uvs.setUVForSide(FaceSides.East, pos, end);
                            uvs.setUVForSide(FaceSides.South, pos, end);
                            uvs.setUVForSide(FaceSides.West, pos, end);

                            DrawToBitmap(map, countX * 16, countY * 16, $"{dir}/{texName}_side.png");

                            countX++;

                            //check
                            if (countX * 16 >= map.Size.Width)
                            {
                                countX = 0;
                                countY++;
                            }
                        }

                        foreach (var face in FaceSides.AllSides)
                        {
                            var faceName = face.ToString().ToLower();

                            if (files.Contains($"{texName}_{faceName}"))
                            {
                                var pos = new Vector2(countX * size.X, countY * size.Y);
                                var end = pos + size;

                                uvs.setUVForSide(face, pos, end);

                                DrawToBitmap(map, countX * 16, countY * 16, $"{dir}/{texName}_{faceName}.png");

                                countX++;

                                //check
                                if (countX * 16 >= map.Size.Width)
                                {
                                    countX = 0;
                                    countY++;
                                }
                            }
                        }
                    }

                    _blockUVs.Add(block, uvs);
                }

                var missingUVs = GetUVsFromBlock(EnumBlock.MISSING);

                foreach (var uv in _blockUVs)
                {
                    if (uv.Key != EnumBlock.AIR)
                        uv.Value.fillEmptySides(missingUVs.getUVForSide(FaceSides.Down));
                }

                map.Save("terrain_debug.png");

                var clone = (Bitmap)map.Clone();

                map.Dispose();

                return clone;
            }
        }

        private static Bitmap CreateMissingTexture()
        {
            var bmp = new Bitmap(16, 16);

            var pink = new SolidBrush(System.Drawing.Color.FromArgb(228, 0, 228));

            using (var g = Graphics.FromImage(bmp))
            {
                g.FillRectangle(pink, 0, 0, 8, 8);
                g.FillRectangle(pink, 8, 8, 8, 8);

                g.FillRectangle(Brushes.Black, 8, 0, 8, 8);
                g.FillRectangle(Brushes.Black, 0, 8, 8, 8);
            }

            return bmp;
        }

        public static int LoadTexture(Bitmap textureMap, bool smooth = false)
        {
            int texID = GL.GenTexture();
            _allTextures.Add(texID);

            GL.BindTexture(TextureTarget.Texture2D, texID);

            BitmapData data = textureMap.LockBits(new Rectangle(0, 0, textureMap.Width, textureMap.Height),
                ImageLockMode.ReadOnly, System.Drawing.Imaging.PixelFormat.Format32bppPArgb);

            GL.TexImage2D(TextureTarget.Texture2D, 0, PixelInternalFormat.Rgba, data.Width, data.Height, 0,
                OpenTK.Graphics.OpenGL.PixelFormat.Bgra, PixelType.UnsignedByte, data.Scan0);
            textureMap.UnlockBits(data);

            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)(smooth ? TextureMinFilter.Linear : TextureMinFilter.Nearest));
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)(smooth ? TextureMinFilter.Linear : TextureMinFilter.Nearest));
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapR, (int)TextureWrapMode.ClampToEdge);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapS, (int)TextureWrapMode.ClampToEdge);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapT, (int)TextureWrapMode.ClampToEdge);

            return texID;
        }

        public static Texture LoadTexture(string textureName, bool smooth = false)
        {
            try
            {
                var bmp = (Bitmap)Image.FromFile($"SharpCraft_Data/assets/textures/{textureName}.png");

                int id = LoadTexture(bmp, smooth);

                return new Texture(id, bmp.Size);
            }
            catch
            {
                Console.WriteLine($"Error: the texture '{textureName}' failed to load!");
            }

            return new Texture(LoadTexture(TEXTURE_MISSING, smooth), TEXTURE_MISSING.Size);
        }

        private static Dictionary<FaceSides, Bitmap> LoadSkyboxTextures()
        {
            Dictionary<FaceSides, Bitmap> bitmaps = new Dictionary<FaceSides, Bitmap>();

            string[] files = new string[0];

            var dir = "SharpCraft_Data/assets/textures/skybox";

            if (Directory.Exists(dir))
                files = Directory.GetFiles(dir, "*.png");

            for (int i = 0; i < files.Length; i++)
            {
                files[i] = Path.GetFileNameWithoutExtension(files[i])?.ToLower();
            }

            foreach (var side in FaceSides.AllSides)
            {
                var sideName = side.ToString().ToLower();

                if (files.Contains($"sky_{sideName}"))
                {
                    var file = $"{dir}/sky_{sideName}.png";

                    bitmaps.Add(side, (Bitmap)Image.FromFile(file));
                }
                else
                {
                    bitmaps.Add(side, TEXTURE_MISSING);
                }
            }

            return bitmaps;
        }

        public static int LoadCubeMap()
        {
            int texID = GL.GenTexture();

            _allTextures.Add(texID);

            GL.ActiveTexture(TextureUnit.Texture0);
            GL.BindTexture(TextureTarget.TextureCubeMap, texID);

            var cubeMapTextures = LoadSkyboxTextures();

            foreach (var dictValues in cubeMapTextures)
            {
                var target = TextureTarget.Texture2D;

                if (dictValues.Key.z == 1) target = TextureTarget.TextureCubeMapPositiveZ;
                else if (dictValues.Key.z == -1) target = TextureTarget.TextureCubeMapNegativeZ;
                else if (dictValues.Key.x == 1) target = TextureTarget.TextureCubeMapPositiveX;
                else if (dictValues.Key.x == -1) target = TextureTarget.TextureCubeMapNegativeX;
                else if (dictValues.Key.y == 1) target = TextureTarget.TextureCubeMapPositiveY;
                else if (dictValues.Key.y == -1) target = TextureTarget.TextureCubeMapNegativeY;

                var bmp = (Bitmap)dictValues.Value.Clone();
                var size = bmp.Size;

                BitmapData data = bmp.LockBits(new Rectangle(0, 0, size.Width, size.Height),
                    ImageLockMode.ReadOnly, System.Drawing.Imaging.PixelFormat.Format32bppArgb);

                GL.TexImage2D(target, 0, PixelInternalFormat.Rgba, data.Width, data.Height, 0, OpenTK.Graphics.OpenGL.PixelFormat.Bgra,
                    PixelType.UnsignedByte, data.Scan0);

                bmp.UnlockBits(data);
            }

            GL.TexParameter(TextureTarget.TextureCubeMap, TextureParameterName.TextureMagFilter, (int)TextureMagFilter.Linear);
            GL.TexParameter(TextureTarget.TextureCubeMap, TextureParameterName.TextureMinFilter, (int)TextureMinFilter.Linear);

            GL.TexParameter(TextureTarget.TextureCubeMap, TextureParameterName.TextureWrapS, (int)TextureWrapMode.ClampToEdge);
            GL.TexParameter(TextureTarget.TextureCubeMap, TextureParameterName.TextureWrapT, (int)TextureWrapMode.ClampToEdge);

            return texID;
        }

        public static TextureBlockUV GetUVsFromBlock(EnumBlock block)
        {
            TextureBlockUV uv;

            _blockUVs.TryGetValue(block, out uv);

            if (uv == null)
                _blockUVs.TryGetValue(EnumBlock.MISSING, out uv);

            return uv;
        }

        private static void DrawToBitmap(Bitmap to, int x, int y, string file)
        {
            using (var bmp = (Bitmap)Image.FromFile(file))
            {
                DrawToBitmap(to, x, y, bmp);
            }
        }

        private static void DrawToBitmap(Bitmap to, int x, int y, Bitmap bmp)
        {
            using (var g = Graphics.FromImage(to))
            {
                g.DrawImage(bmp, x, y, 16, 16);
            }
        }

        private static int ContainsContaining(Array a, string s)
        {
            int res = 0;

            for (int i = 0; i < a.Length; i++)
            {
                if (((string)a.GetValue(i)).Contains(s))
                    res++;
            }

            return res;
        }

        public static void DestroyTexture(int id)
        {
            GL.DeleteTexture(id);
        }

        public static void Reload()
        {
            DestroyTexture(TEXTURE_BLOCKS.ID);

            _blockUVs.Clear();

            LoadTextures();
        }

        public static void Destroy()
        {
            DestroyTexture(TEXTURE_BLOCKS.ID);

            for (int i = 0; i < _allTextures.Count; i++)
            {
                DestroyTexture(_allTextures[i]);
            }
        }
    }
}