using OpenTK.Graphics.OpenGL;

namespace SharpCraft.model
{
    public class ModelRaw : IModelRaw
    {
        public int VaoID { get; }
        public int[] BufferIDs { get; }

        public int VertexCount { get; protected set; }

        /*
        public ModelRaw(int vaoID, int valuesPerVertice, List<RawQuad> quads, params int[] bufferIDs)
        {
            VaoID = vaoID;
            BufferIDs = bufferIDs;

            foreach (RawQuad quad in quads)
                VertexCount += quad.vertices.Length / valuesPerVertice;
        }*/

        public ModelRaw(int vaoID, int vertexCount, params int[] bufferIDs)
        {
            VaoID = vaoID;
            BufferIDs = bufferIDs;

            VertexCount = vertexCount;
        }

        public bool hasLocalData() => true;

        public void Render(PrimitiveType primitiveType) => GL.DrawArrays(primitiveType, 0, VertexCount);
    }
}