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
using Bitmap = System.Drawing.Bitmap;
using Image = System.Drawing.Image;
using Rectangle = System.Drawing.Rectangle;

namespace SharpCraft.texture
{
    internal class TextureManager
    {
        private static Dictionary<EnumBlock, TextureBlockUV> UVs = new Dictionary<EnumBlock, TextureBlockUV>();

        private static List<int> textures = new List<int>();

        private static Bitmap missing = createMissingBMP();

        public static int blockTextureAtlasID;

        public static void stitchTextures()
        {
            blockTextureAtlasID = loadTexture(generateTextureMap(), false);
        }

        private static Bitmap generateTextureMap()
        {
            Bitmap map = new Bitmap(256, 256);

            var blocks = Enum.GetValues(typeof(EnumBlock));

            var dir = "SharpCraft_Data/assets/textures/blocks";

            var files = new string[0];
            if (Directory.Exists(dir))
                files = Directory.GetFiles("SharpCraft_Data/assets/textures/blocks", "*.png");

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

                        drawToBitmap(map, countX * 16, countY * 16, missing);

                        countX++;

                        //check
                        if (countX * 16 >= map.Size.Width)
                        {
                            countX = 0;
                            countY++;
                        }
                    }
                    else if (containsContaining(files, texName) > 0)
                    {
                        //found

                        //found texture for all 6 sides
                        if (files.Contains(texName))
                        {
                            var pos = new Vector2(countX * size.X, countY * size.Y);
                            var end = pos + size;

                            uvs.fill(pos, end);

                            drawToBitmap(map, countX * 16, countY * 16, $"{dir}/{texName}.png");

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

                            drawToBitmap(map, countX * 16, countY * 16, $"{dir}/{texName}_side.png");

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

                                drawToBitmap(map, countX * 16, countY * 16, $"{dir}/{texName}_{faceName}.png");

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

                    UVs.Add(block, uvs);
                }

                var missingUVs = getUVsFromBlock(EnumBlock.MISSING);

                foreach (var uv in UVs)
                {
                    if (uv.Key != EnumBlock.AIR)
                        uv.Value.fillEmptySides(missingUVs.getUVForSide(FaceSides.Down));
                }

                #region fuj

                /*
				foreach (EnumBlock block in blocks)
				{
				    var name = block.ToString().ToLower();

				    if (containsContaining(files, name))
				    {
				        var uvs = new BlockTextureUV();

				        if (files.Contains(name))
				        {
				            if (countX * 16 >= map.Size.Width)
				            {
				                countX = 0;
				                countY++;
				            }

				            var pos = new Vector2(countX * size.X, countY * size.Y);
				            var end = pos + size;

				            uvs.fill(pos, end);

				            using (var bmp = Image.FromFile(dir + name + ".png"))
				            {
				                using (var g = Graphics.FromImage(map))
				                {
				                    g.DrawImage(bmp, countX * 16, countY * 16, 16, 16);
				                }
				            }

				            countX++;
				        }

				        var textureName = "";

				        if (files.Contains(textureName = name + "_side"))
				        {
				            if (countX * 16 >= map.Size.Width)
				            {
				                countX = 0;
				                countY++;
				            }

				            var pos = new Vector2(countX * size.X, countY * size.Y);
				            var end = pos + size;

				            uvs.setUVForSide(EnumFacing.NORTH, pos, end);
				            uvs.setUVForSide(EnumFacing.SOUTH, pos, end);
				            uvs.setUVForSide(EnumFacing.WEST, pos, end);
				            uvs.setUVForSide(EnumFacing.EAST, pos, end);

				            using (var bmp = Image.FromFile(dir + textureName + ".png"))
				            {
				                using (var g = Graphics.FromImage(map))
				                {
				                    g.DrawImage(bmp, countX * 16, countY * 16, 16, 16);
				                }
				            }

				            countX++;
				        }

				        foreach (EnumFacing side in sides)
				        {
				            var sideName = side.ToString().ToLower();

				            if (files.Contains(textureName = name + "_" + sideName))
				            {
				                if (countX * 16 >= map.Size.Width)
				                {
				                    countX = 0;
				                    countY++;
				                }

				                var pos = new Vector2(countX * size.X, countY * size.Y);
				                var end = pos + size;

				                uvs.setUVForSide(side, pos, end);

				                using (var bmp = Image.FromFile(dir + textureName + ".png"))
				                {
				                    using (var g = Graphics.FromImage(map))
				                    {
				                        g.DrawImage(bmp, countX * 16, countY * 16, 16, 16);
				                    }
				                }

				                countX++;
				            }
				        }

				        Uv.Add(block, uvs);
				    }
				}*/

                #endregion fuj

                map.Save("terrain_debug.png");

                return (Bitmap)map.Clone();
            }
        }

        private static Bitmap createMissingBMP()
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

        public static int loadTexture(Bitmap textureMap, bool smooth)
        {
            GL.Hint(HintTarget.PerspectiveCorrectionHint, HintMode.Nicest);

            int texID = GL.GenTexture();
            textures.Add(texID);

            GL.BindTexture(TextureTarget.Texture2D, texID);

            BitmapData data = textureMap.LockBits(new Rectangle(0, 0, textureMap.Width, textureMap.Height),
                ImageLockMode.ReadOnly, System.Drawing.Imaging.PixelFormat.Format32bppPArgb);

            GL.TexImage2D(TextureTarget.Texture2D, 0, PixelInternalFormat.Rgba, data.Width, data.Height, 0,
                OpenTK.Graphics.OpenGL.PixelFormat.Bgra, PixelType.UnsignedByte, data.Scan0);
            textureMap.UnlockBits(data);

            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter,
                (int)(smooth ? TextureMinFilter.Linear : TextureMinFilter.Nearest));
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter,
                (int)(smooth ? TextureMagFilter.Linear : TextureMagFilter.Nearest));
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapS,
                (int)TextureWrapMode.ClampToEdge);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapT,
                (int)TextureWrapMode.ClampToEdge);

            return texID;
        }

        public static Texture loadTexture(string textureName, bool smooth)
        {
            try
            {
                var bmp = (Bitmap)Image.FromFile($"SharpCraft_Data/assets/textures/{textureName}.png");

                int id = loadTexture(bmp, smooth);

                return new Texture(id, bmp.Size);
            }
            catch
            {
                Console.WriteLine($"Error: the texture '{textureName}' failed to load!");
            }

            return new Texture(loadTexture(missing, smooth), missing.Size);
        }

        private static Dictionary<FaceSides, Bitmap> loadCubeMapTextures()
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
                    bitmaps.Add(side, missing);
                }
            }

            return bitmaps;
        }

        public static int loadCubeMap()
        {
            int texID = GL.GenTexture();

            textures.Add(texID);

            GL.ActiveTexture(TextureUnit.Texture0);
            GL.BindTexture(TextureTarget.TextureCubeMap, texID);

            var cubeMapTextures = loadCubeMapTextures();

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

        public static TextureBlockUV getUVsFromBlock(EnumBlock block)
        {
            TextureBlockUV uv;

            UVs.TryGetValue(block, out uv);

            if (uv == null)
                UVs.TryGetValue(EnumBlock.MISSING, out uv);

            return uv;
        }

        private static void drawToBitmap(Bitmap to, int x, int y, string file)
        {
            using (var bmp = (Bitmap)Image.FromFile(file))
            {
                drawToBitmap(to, x, y, bmp);
            }
        }

        private static void drawToBitmap(Bitmap to, int x, int y, Bitmap bmp)
        {
            using (var g = Graphics.FromImage(to))
            {
                g.DrawImage(bmp, x, y, 16, 16);
            }
        }

        private static int containsContaining(Array a, string s)
        {
            int res = 0;

            for (int i = 0; i < a.Length; i++)
            {
                if (((string)a.GetValue(i)).Contains(s))
                    res++;
            }

            return res;
        }

        public static void destroyTexture(int ID)
        {
            GL.DeleteTexture(ID);
        }

        public static void reload()
        {
            destroyTexture(blockTextureAtlasID);

            UVs.Clear();

            stitchTextures();
        }

        public static void cleanUp()
        {
            destroyTexture(blockTextureAtlasID);

            for (int i = 0; i < textures.Count; i++)
            {
                destroyTexture(textures[i]);
            }
        }
    }
}