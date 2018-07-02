using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace SharpCraft.util
{
    public static class LangUtil
    {
        private static Dictionary<string, string> _dictionary = new Dictionary<string, string>();

        //TODO - load language files from mods
        public static void LoadLang(string langName)
        {
            var file = $"{SharpCraft.Instance.GameFolderDir}/assets/sharpcraft/lang/{langName}.lang";

            if (!File.Exists(file))
            {
                Console.WriteLine($"ERROR: Could not find file for language '{langName}'");
                return;
            }

            _dictionary.Clear();

            var dict = File.ReadAllLines(file);

            foreach (var line in dict)
            {
                var split = line.Split(new []{'='}, StringSplitOptions.RemoveEmptyEntries);

                if (split.Length != 2)
                    continue;

                var unlocalizedName = split[0];
                var localizedName = split[1];

                _dictionary.Add(unlocalizedName, localizedName);
            }
        }

        public static string GetLocalized(string unlocalizedString)
        {
            if (!_dictionary.TryGetValue(unlocalizedString, out var localizedString))
                localizedString = unlocalizedString;

            return localizedString;
        }

        public static string GetUnlocalizedNameLast(string unlocalizedName)
        {
            return unlocalizedName.Split(new []{'.'}, StringSplitOptions.RemoveEmptyEntries).LastOrDefault();
        }
    }
}
