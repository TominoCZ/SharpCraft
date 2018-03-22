using System;
using System.Collections.Generic;
using System.Text;
using System.Xml.Schema;

namespace SharpCraft
{
    class ModelRaw : IModelRaw
    {
        public int vaoID { get; }
        public int[] bufferIDs { get; }
        public int vertexCount { get; protected set; }
        public bool hasUVs { get; protected set; }
        public bool hasNormals { get; protected set; }

        public List<RawQuad> quads;

        public ModelRaw(int vaoID, int valuesPerVertice, List<RawQuad> quads, params int[] bufferIDs)
        {
            this.vaoID = vaoID;
            this.quads = quads;

            this.bufferIDs = bufferIDs;

            foreach (var quad in quads)
            {
                vertexCount += quad.vertices.Length / valuesPerVertice;

                var uv = quads[0].UVs.Length > 0;
                var normal = quads[0].normal.Length > 0;

                if (uv)
                    hasUVs = true;
                if (normal)
                    hasNormals = true;
            }
        }
    }
}
