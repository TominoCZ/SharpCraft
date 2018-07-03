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
        private static readonly Dictionary<char, FontMapCharacter> _dictionary = new Dictionary<char, FontMapCharacter>();

        public static void LoadCharacters(Texture tex, string fntFileName)
        {
            _dictionary.Clear();

            var file = $"{SharpCraft.Instance.GameFolderDir}/assets/sharpcraft/textures/{fntFileName}.fnt";

            if (!File.Exists(file))
            {
                Console.WriteLine($"ERROR: Couldn't find .fnt file for font {fntFileName}");
                return;
            }

            var lines = File.ReadAllLines(file);

            foreach (var line in lines)
            {
                var parsed = ParseFntCharLine(line);

                if (parsed == null)
                    continue;

                var uv = TextureManager.GetUV(tex.TextureSize.Width, tex.TextureSize.Height, parsed.X, parsed.Y, parsed.W, parsed.H);

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
    }
}