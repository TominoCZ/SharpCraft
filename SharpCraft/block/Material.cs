using System.Collections.Generic;

namespace SharpCraft_Client.block
{
    public class Material
    {
        public bool CanWalkThrough { get; }

        public string Name { get; }

        private static readonly Dictionary<string, Material> _materials = new Dictionary<string, Material>();

        public Material(string name, bool canWalkThrough)
        {
            Name = name;
            CanWalkThrough = canWalkThrough;
        }

        public static bool RegisterMaterial(Material mat)
        {
            var ok = !_materials.ContainsKey(mat.Name);

            if (ok)
                _materials.Add(mat.Name, mat);

            return ok;
        }

        public static Material GetMaterial(string name)
        {
            _materials.TryGetValue(name, out Material mat);

            return mat;
        }
    }
}