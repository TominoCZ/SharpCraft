using OpenTK;
using SharpCraft.texture;
using SharpCraft.util;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;

namespace SharpCraft.gui
{
    internal class FontManager
    {
        private static Dictionary<char, FontMapCharacter> _dictionary = new Dictionary<char, FontMapCharacter>();

        public static void LoadCharacters(Texture tex, string fntFileName)
        {
            _dictionary.Clear();

            var file = $"SharpCraft_Data/assets/textures/{fntFileName}.fnt";

            var lines = File.ReadAllLines(file);

            foreach (var line in lines)
            {
                var parsed = ParseFntCharLine(line);

                if (parsed == null)
                    continue;

                var uv = GetUV(tex.TextureSize.Width, tex.TextureSize.Height, parsed.X, parsed.Y, parsed.W, parsed.H);

                _dictionary.Add(parsed.Char, new FontMapCharacter(parsed, uv));
            }
        }

        public static FontMapCharacter GetCharacter(char c)
        {
            if (!_dictionary.TryGetValue(c, out var node))
                _dictionary.TryGetValue(' ', out node);

            return node;
        }

        private static FontMapCharacterNode ParseFntCharLine(string line)
        {
            line = Regex.Replace(line, @"\s+", " ");

            if (!line.Contains("char id="))
                return null;

            var startIndex = line.IndexOf("id=", StringComparison.Ordinal);

            line = line.Substring(startIndex, line.Length - startIndex);

            var data = line.Split(' ');
            //x=250   y=33    width=3     height=32
            var character = (char)int.Parse(data[0].Replace("id=", ""));
            var x = int.Parse(data[1].Replace("x=", ""));
            var y = int.Parse(data[2].Replace("y=", ""));

            var w = int.Parse(data[3].Replace("width=", ""));
            var h = int.Parse(data[4].Replace("height=", ""));

            var ox = int.Parse(data[5].Replace("xoffset=", ""));
            var oy = int.Parse(data[6].Replace("yoffset=", ""));

            return new FontMapCharacterNode(character, x, y, w, h, ox, oy);
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

    internal class FontMapCharacter
    {
        public readonly FontMapCharacterNode Character;
        public readonly TextureUVNode TextureUv;

        public FontMapCharacter(FontMapCharacterNode character, TextureUVNode textureUv)
        {
            Character = character;
            TextureUv = textureUv;
        }
    }

    internal class FontMapCharacterNode
    {
        public readonly char Char;
        public readonly int X;
        public readonly int Y;

        public readonly int W;
        public readonly int H;

        public readonly int OffsetX;
        public readonly int OffsetY;

        public FontMapCharacterNode(char c, int x, int y, int w, int h, int oX, int oY)
        {
            Char = c;
            X = x;
            Y = y;
            W = w;
            H = h;

            OffsetX = oX;
            OffsetY = oY;
        }
    }
}