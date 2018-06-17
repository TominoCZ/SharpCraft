using OpenTK.Graphics.OpenGL;
using System.Collections.Generic;

namespace SharpCraft.model
{
    internal class ModelRaw : IModelRaw
    {
        public int vaoID { get; }
        public int[] bufferIDs { get; }

        public int vertexCount { get; protected set; }

        public ModelRaw(int vaoID, int valuesPerVertice, List<RawQuad> quads, params int[] bufferIDs)
        {
            this.vaoID = vaoID;
            this.bufferIDs = bufferIDs;

            foreach (RawQuad quad in quads)
                vertexCount += quad.vertices.Length / valuesPerVertice;
        }

        public bool hasLocalData() => true;

        public void Render(PrimitiveType primitiveType) => GL.DrawArrays(primitiveType, 0, vertexCount);
    }
}