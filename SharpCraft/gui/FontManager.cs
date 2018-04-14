using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using OpenTK;
using SharpCraft.texture;
using SharpCraft.util;

namespace SharpCraft.gui
{
    class FontManager
    {
        private static Dictionary<char, TextureUVNode> _dictionary = new Dictionary<char, TextureUVNode>();

        public static void LoadCharacters(Texture tex, string fntFileName)
        {
            var file = $"SharpCraft_Data/assets/textures/{fntFileName}.fnt";

            var lines = File.ReadAllLines(file);

            var start = false;

            for (var i = 0; i < lines.Length; i++)
            {
                var line = lines[i];

                if (!start && line.StartsWith("char"))
                    start = true;
                else
                    continue;

                var parsed = ParseFntCharLine(line);

                var uv = GetUV(tex.textureSize.Width, tex.textureSize.Height, parsed.X, parsed.Y, parsed.W, parsed.H);

                _dictionary.Add(parsed.Char, uv);
            }
        }

        private static FontMapCharacter ParseFntCharLine(string line)
        {
            line = Regex.Replace(line, @"\s+", " ");
            var dataStartIndex = line.IndexOf("id=", StringComparison.Ordinal);
            line = line.Substring(dataStartIndex, line.Length - dataStartIndex);

            var data = line.Split(' ');
            //x=250   y=33    width=3     height=32
            var character = (char)int.Parse(data[0].Replace("id=", ""));
            var x = int.Parse(data[1].Replace("x=", ""));
            var y = int.Parse(data[2].Replace("y=", ""));

            var w = int.Parse(data[1].Replace("width=", ""));
            var h = int.Parse(data[2].Replace("height=", ""));

            return new FontMapCharacter(character, x, y, w, h);
        }

        private static TextureUVNode GetUV(int textureSizeX, int textureSizeY, int x, int y, int sizeX, int sizeY) //TODO i might move this to TextureManager or TextureHelper
        {
            var mapSize = new Vector2(textureSizeX, textureSizeY);
            var sampleSize = new Vector2(sizeX, sizeY);

            var pos_start = new Vector2(x, y);
            var pos_end = pos_start + sampleSize;

            var start = Vector2.Divide(pos_start, mapSize);
            var end = Vector2.Divide(pos_end, mapSize);

            return new TextureUVNode(start, end);
        }
    }

    class FontMapCharacter
    {
        public char Char;
        public int X;
        public int Y;

        public int W;
        public int H;

        public FontMapCharacter(char c, int x, int y, int w, int h)
        {
            Char = c;
            X = x;
            Y = y;
            W = w;
            H = h;
        }
    }
}
