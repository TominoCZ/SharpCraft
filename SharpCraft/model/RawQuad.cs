using SharpCraft.block;

namespace SharpCraft.model
{
    internal struct RawQuad
    {
        public float[] vertices { get; }
        public float[] normal { get; }
        public float[] UVs { get; }
        public int valuesPerVertice { get; }

        public bool Loaded { get; }

        public RawQuad(float[] vertices, float[] UVs, float[] normal, int valuesPerVertice)
        {
            this.vertices = vertices;
            this.normal = normal;
            this.UVs = UVs;

            this.valuesPerVertice = valuesPerVertice;
            Loaded = true;
        }

        public RawQuad(float[] vertices, float[] UVs, int valuesPerVertice) : this(vertices, UVs, new float[0], valuesPerVertice)
        {
        }

        public RawQuad(float[] vertices, int valuesPerVertice) : this(vertices, new float[0], new float[0], valuesPerVertice)
        {
        }

        public RawQuad offset(BlockPos pos)
        {
            float[] newVertices = new float[vertices.Length];

            for (int i = 0; i < newVertices.Length; i += 3)
            {
                newVertices[i] = vertices[i] + pos.X;
                newVertices[i + 1] = vertices[i + 1] + pos.Y;
                newVertices[i + 2] = vertices[i + 2] + pos.Z;
            }

            return new RawQuad(newVertices, UVs, normal, valuesPerVertice);
        }
    }
}