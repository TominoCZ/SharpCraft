using OpenTK;
using SharpCraft.world.chunk;
using System.Collections.Generic;

namespace SharpCraft.block
{
    public struct FaceSides
    {
        public static bool operator ==(FaceSides f1, FaceSides f2)
        {
            return f1.id == f2.id;
        }

        public static bool operator !=(FaceSides f1, FaceSides f2)
        {
            return f1.id != f2.id;
        }

        public static readonly FaceSides East = new FaceSides("East", 2, 1, 0, 0);
        public static readonly FaceSides West = new FaceSides("West", 3, -1, 0, 0);
        public static readonly FaceSides Up = new FaceSides("Up", 4, 0, 1, 0);
        public static readonly FaceSides Down = new FaceSides("Down", 5, 0, -1, 0);
        public static readonly FaceSides South = new FaceSides("South", 1, 0, 0, 1);
        public static readonly FaceSides North = new FaceSides("North", 0, 0, 0, -1);
        public static readonly FaceSides Null = new FaceSides("Null", -1, 0, 0, 0);

        public static readonly IReadOnlyList<FaceSides> XPlane = new[] { Up, South, Down, North };
        public static readonly IReadOnlyList<FaceSides> YPlane = new[] { East, South, West, North };
        public static readonly IReadOnlyList<FaceSides> ZPlane = new[] { Up, East, Down, West };
        public static readonly IReadOnlyList<FaceSides> AllSides = new[] { East, West, Up, Down, South, North };

        public readonly int x, y, z, id;

        private readonly string Name;

        private FaceSides(string name, int id, int x, int y, int z)
        {
            Name = name;
            this.id = id;
            this.x = x;
            this.y = y;
            this.z = z;
        }

        public Vector3 ToVec()
        {
            return new Vector3(x, y, z);
        }

        public override string ToString()
        {
            return Name;
        }

        public override bool Equals(object obj)
        {
            if (!(obj is FaceSides))
            {
                return false;
            }

            FaceSides sides = (FaceSides)obj;
            return x == sides.x &&
                   y == sides.y &&
                   z == sides.z &&
                   id == sides.id &&
                   Name == sides.Name;
        }

        public override int GetHashCode()
        {
            int hashCode = -1813808738;
            hashCode = hashCode * -1521134295 + x.GetHashCode();
            hashCode = hashCode * -1521134295 + y.GetHashCode();
            hashCode = hashCode * -1521134295 + z.GetHashCode();
            hashCode = hashCode * -1521134295 + id.GetHashCode();
            hashCode = hashCode * -1521134295 + EqualityComparer<string>.Default.GetHashCode(Name);
            return hashCode;
        }

        public static BlockPos operator +(BlockPos l, FaceSides r) => new BlockPos(l.X + r.x, l.Y + r.y, l.Z + r.z);

        public static ChunkPos operator +(ChunkPos l, FaceSides r) => new ChunkPos(l.x + r.x, l.z + r.z);

        public static Vector3 operator +(Vector3 l, FaceSides r) => new Vector3(l.X + r.x, l.Y + r.y, l.Z + r.z);

        public static Vector3 operator *(FaceSides l, float r) => new Vector3(l.x + r, l.y + r, l.z + r);
    }
}