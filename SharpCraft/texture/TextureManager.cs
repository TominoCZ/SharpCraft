using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using OpenTK;
using OpenTK.Graphics.OpenGL;
using SharpCraft.block;
using SharpCraft.gui;
using SharpCraft.util;
using PixelFormat = System.Drawing.Imaging.PixelFormat;

namespace SharpCraft.texture
{
    internal class TextureManager
    {
        // private static Dictionary<EnumBlock, TextureBlockUV> _blockUVs = new Dictionary<EnumBlock, TextureBlockUV>();

        private static readonly List<int> _allTextures = new List<int>();

        public static readonly Bitmap TEXTURE_MISSING = CreateMissingTexture();

        public static Texture TEXTURE_DESTROY_PROGRESS;

        //public static Texture TEXTURE_BLOCKS;
        public static Texture TEXTURE_GUI_WIDGETS;

        public static Texture TEXTURE_TEXT;

        public static void LoadTextures()
        {
            //StitchTextures();

            TEXTURE_GUI_WIDGETS = LoadTexture("gui/widgets");
            TEXTURE_DESTROY_PROGRESS = LoadTexture("blocks/destroy_progress");
            TEXTURE_TEXT = LoadTexture("font/default");

            FontManager.LoadCharacters(TEXTURE_TEXT, "font/default");
        }

        private static Bitmap CreateMissingTexture()
        {
            Bitmap bmp = new Bitmap(16, 16);

            SolidBrush pink = new SolidBrush(Color.FromArgb(228, 0, 228));

            using (Graphics g = Graphics.FromImage(bmp))
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
                ImageLockMode.ReadOnly, PixelFormat.Format32bppPArgb);

            GL.TexImage2D(TextureTarget.Texture2D, 0, PixelInternalFormat.Rgba, data.Width, data.Height, 0,
                OpenTK.Graphics.OpenGL.PixelFormat.Bgra, PixelType.UnsignedByte, data.Scan0);
            textureMap.UnlockBits(data);

            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)(smooth ? TextureMinFilter.Linear : TextureMinFilter.Nearest));
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)(smooth ? TextureMagFilter.Linear : TextureMagFilter.Nearest));//TODO correct this in the main branch
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapS, (int)TextureWrapMode.ClampToEdge);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapT, (int)TextureWrapMode.ClampToEdge);

            return texID;
        }

        public static int LoadTextureWithMipMap(Bitmap textureMap)
        {
            int texID = GL.GenTexture();
            _allTextures.Add(texID);

            GL.BindTexture(TextureTarget.Texture2D, texID);

            BitmapData data = textureMap.LockBits(new Rectangle(0, 0, textureMap.Width, textureMap.Height),
                ImageLockMode.ReadOnly,
                PixelFormat.Format32bppPArgb);

            GL.TexImage2D(TextureTarget.Texture2D, 0,
                PixelInternalFormat.Rgba, data.Width, data.Height, 0,
                OpenTK.Graphics.OpenGL.PixelFormat.Bgra, PixelType.UnsignedByte, data.Scan0);

            textureMap.UnlockBits(data);

            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.GenerateMipmap, (int)GenerateMipmapTarget.Texture2D);

            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)TextureMinFilter.NearestMipmapLinear);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)TextureMagFilter.Nearest);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMaxLevel, (int)Math.Floor(Math.Log(16) / Math.Log(2)));
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureLodBias, 0);

            GL.GenerateMipmap(GenerateMipmapTarget.Texture2D);

            return texID;
        }

        public static Texture LoadTexture(string textureName, bool smooth = false)
        {
            try
            {
                Bitmap bmp = (Bitmap)Image.FromFile($"{SharpCraft.Instance.GameFolderDir}/assets/sharpcraft/textures/{textureName}.png");

                using (bmp)
                {
                    int id = LoadTexture(bmp, smooth);

                    return new Texture(id, bmp.Size);
                }
            }
            catch
            {
                Console.WriteLine($"Error: the texture '{textureName}' failed to load!");
            }

            return new Texture(LoadTexture(TEXTURE_MISSING, smooth), TEXTURE_MISSING.Size);
        }

        public static int LoadCubeMap()
        {
            int texID = GL.GenTexture();

            _allTextures.Add(texID);

            GL.BindTexture(TextureTarget.TextureCubeMap, texID);

            Dictionary<FaceSides, Bitmap> cubeMapTextures = LoadSkyboxTextures();

            foreach (KeyValuePair<FaceSides, Bitmap> dictValues in cubeMapTextures)
            {
                TextureTarget target = TextureTarget.Texture2D;

                if (dictValues.Key.z == 1) target = TextureTarget.TextureCubeMapPositiveZ;
                else if (dictValues.Key.z == -1) target = TextureTarget.TextureCubeMapNegativeZ;
                else if (dictValues.Key.x == 1) target = TextureTarget.TextureCubeMapPositiveX;
                else if (dictValues.Key.x == -1) target = TextureTarget.TextureCubeMapNegativeX;
                else if (dictValues.Key.y == 1) target = TextureTarget.TextureCubeMapPositiveY;
                else if (dictValues.Key.y == -1) target = TextureTarget.TextureCubeMapNegativeY;

                using (dictValues.Value)
                {
                    using (var bmp = (Bitmap)dictValues.Value.Clone())
                    {
                        Size size = bmp.Size;

                        BitmapData data = bmp.LockBits(new Rectangle(0, 0, size.Width, size.Height),
                            ImageLockMode.ReadOnly, PixelFormat.Format32bppArgb);

                        GL.TexImage2D(target, 0, PixelInternalFormat.Rgba, data.Width, data.Height, 0,
                            OpenTK.Graphics.OpenGL.PixelFormat.Bgra,
                            PixelType.UnsignedByte, data.Scan0);

                        bmp.UnlockBits(data);
                    }
                }
            }

            GL.TexParameter(TextureTarget.TextureCubeMap, TextureParameterName.TextureMagFilter, (int)TextureMagFilter.Linear);
            GL.TexParameter(TextureTarget.TextureCubeMap, TextureParameterName.TextureMinFilter, (int)TextureMinFilter.Linear);

            GL.TexParameter(TextureTarget.TextureCubeMap, TextureParameterName.TextureWrapS, (int)TextureWrapMode.ClampToEdge);
            GL.TexParameter(TextureTarget.TextureCubeMap, TextureParameterName.TextureWrapT, (int)TextureWrapMode.ClampToEdge);

            return texID;
        }

        public static TextureUvNode GetUV(int textureSizeX, int textureSizeY, int x, int y, int sizeX, int sizeY) //TODO i might move this to TextureManager or TextureHelper
        {
            var mapSize = new Vector2(textureSizeX, textureSizeY);
            var sampleSize = new Vector2(sizeX, sizeY);

            var pos_start = new Vector2(x, y);
            var pos_end = pos_start + sampleSize;

            var start = Vector2.Divide(pos_start, mapSize);
            var end = Vector2.Divide(pos_end, mapSize);

            return new TextureUvNode(start, end);
        }

        private static Dictionary<FaceSides, Bitmap> LoadSkyboxTextures()
        {
            Dictionary<FaceSides, Bitmap> bitmaps = new Dictionary<FaceSides, Bitmap>();

            string[] files = new string[0];

            string dir = SharpCraft.Instance.GameFolderDir + "/assets/sharpcraft/textures/skybox";

            if (Directory.Exists(dir))
                files = Directory.GetFiles(dir, "*.png");

            for (int i = 0; i < files.Length; i++)
            {
                files[i] = Path.GetFileNameWithoutExtension(files[i])?.ToLower();
            }

            foreach (FaceSides side in FaceSides.AllSides)
            {
                string sideName = side.ToString().ToLower();

                if (files.Contains($"sky_{sideName}"))
                {
                    string file = $"{dir}/sky_{sideName}.png";

                    bitmaps.Add(side, (Bitmap) Image.FromFile(file));
                }
                else
                {
                    bitmaps.Add(side, TEXTURE_MISSING);
                }
            }

            return bitmaps;
        }

        /*
        public static TextureBlockUV GetUVsFromBlock(EnumBlock block)
        {
            _blockUVs.TryGetValue(block, out TextureBlockUV uv);

            if (uv == null)
                _blockUVs.TryGetValue(EnumBlock.MISSING, out uv);

            return uv;
        }*/

        private static void DrawToBitmap(Bitmap to, int x, int y, string file)
        {
            using (Bitmap bmp = (Bitmap)Image.FromFile(file))
            {
                DrawToBitmap(to, x, y, bmp);
            }
        }

        private static void DrawToBitmap(Bitmap to, int x, int y, Bitmap bmp)
        {
            using (Graphics g = Graphics.FromImage(to))
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
            //DestroyTexture(TEXTURE_BLOCKS.ID);

            //_blockUVs.Clear();

            LoadTextures();
        }

        public static void Destroy()
        {
            //DestroyTexture(TEXTURE_BLOCKS.ID);

            for (int i = 0; i < _allTextures.Count; i++)
            {
                DestroyTexture(_allTextures[i]);
            }
        }
    }
}