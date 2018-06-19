using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace SharpCraft
{
    internal class SettingsManager
    {
        private static readonly Dictionary<string, string> _settings = new Dictionary<string, string>();

        static SettingsManager()
        {
            _settings.Add("sensitivity", "1");
            _settings.Add("renderdistance", "8");
            _settings.Add("worldseed", "yeet");
        }

        public static void Load()
        {
            string file = SharpCraft.Instance.GameFolderDir + "/settings.txt";

            if (File.Exists(file))
            {
                IEnumerable<string> data = File.ReadLines(file);

                foreach (string line in data)
                {
                    string parsed = line.Trim().Replace(" ", "").ToLower();
                    string[] split = parsed.Split('=');

                    if (split.Length < 2)
                        continue;

                    string variable = split[0];
                    string value = split[1];

                    if (_settings.ContainsKey(variable))
                    {
                        _settings.Remove(variable);
                        _settings.Add(variable, value);
                    }
                }
            }

            Save();
        }

        public static void Save()
        {
            string file = SharpCraft.Instance.GameFolderDir + "/settings.txt";

            StringBuilder sb = new StringBuilder();

            string[] keys = _settings.Keys.ToArray();

            for (int index = 0; index < keys.Length - 1; index++)
            {
                string key = keys[index];

                sb.AppendLine($"{key}={GetValue(key)}");
            }

            KeyValuePair<string, string> last = _settings.Last();

            sb.Append($"{last.Key}={GetValue(last.Key)}");

            File.WriteAllText(file, sb.ToString());
        }

        public static string GetValue(string variable)
        {
            return _settings[variable];
        }

        public static int GetInt(string variable)
        {
            return int.Parse(GetValue(variable));
        }

        public static float GetFloat(string variable)
        {
            return float.Parse(GetValue(variable));
        }

        public static bool GetBool(string variable)
        {
            return bool.Parse(GetValue(variable));
        }
    }
}