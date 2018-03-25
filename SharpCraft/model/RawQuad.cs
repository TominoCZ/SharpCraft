namespace SharpCraft
{
    internal class RawQuad
    {
        public float[] vertices { get; }
        public float[] normal { get; }
        public float[] UVs { get; }

        public RawQuad(float[] vertices, float[] UVs, float[] normal)
        {
            this.vertices = vertices;
            this.normal = normal;
            this.UVs = UVs;
        }

        public RawQuad(float[] vertices, float[] UVs) : this(vertices, UVs, new float[0])
        {
        }

        public RawQuad(float[] vertices) : this(vertices, new float[0], new float[0])
        {
        }

        public RawQuad offset(BlockPos pos)
        {
            float[] newVertices = new float[vertices.Length];

            for (int i = 0; i < newVertices.Length; i += 3)
            {
                newVertices[i] = vertices[i] + pos.x;
                newVertices[i + 1] = vertices[i + 1] + pos.y;
                newVertices[i + 2] = vertices[i + 2] + pos.z;
            }

            return new RawQuad(newVertices, UVs, normal);
        }
    }
}