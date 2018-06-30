using System.Collections.Generic;

namespace SharpCraft.block
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
            return _materials.TryAdd(mat.Name, mat);
        }

        public static Material GetMaterial(string name)
        {
            _materials.TryGetValue(name, out Material mat);

            return mat;
        }
    }
}