using System;

namespace SharpCraft_Client
{
    internal class ModInfo
    {
        public readonly string ID;

        public readonly string Name;
        public readonly string Version;
        public readonly string Author;

        public bool IsValid =>
            String.IsNullOrEmpty(ID) ||
            String.IsNullOrEmpty(Name) ||
            String.IsNullOrEmpty(Version) ||
            String.IsNullOrEmpty(Author);

        public ModInfo(string id, string name, string version, string author)
        {
            ID = id;
            Name = name;
            Version = version;
            Author = author;
        }
    }
}