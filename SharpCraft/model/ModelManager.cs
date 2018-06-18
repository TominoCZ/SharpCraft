using OpenTK.Graphics.OpenGL;
using SharpCraft.block;
using System.Collections.Generic;

namespace SharpCraft.model
{
    internal class ModelManager
    {
        private static List<int> VAOs = new List<int>();
        private static List<int> VBOs = new List<int>();

        public static ModelBlockRaw LoadBlockModelToVAO(Dictionary<FaceSides, RawQuad> quads)
        {
            int vaoID = CreateVAO();

            List<float> vertices = new List<float>();
            List<float> normals = new List<float>();
            List<float> UVs = new List<float>();

            foreach (KeyValuePair<FaceSides, RawQuad> q in quads)
            {
                vertices.AddRange(q.Value.vertices);
                normals.AddRange(q.Value.normal);
                UVs.AddRange(q.Value.UVs);
            }

            int buff0 = StoreDataInAttribList(0, 3, vertices.ToArray());
            int buff1 = StoreDataInAttribList(1, 2, UVs.ToArray());
            int buff2 = StoreDataInAttribList(2, 3, normals.ToArray());

            UnbindVAO();

            return new ModelBlockRaw(vaoID, quads, buff0, buff1, buff2);
        }

        public static ModelBlockRaw LoadBlockModelToVAO(float[] vertexes, float[] normals, float[] UVs)
        {
            int vaoID = CreateVAO();

            int buff0 = StoreDataInAttribList(0, 3, vertexes);
            int buff1 = StoreDataInAttribList(1, 2, UVs);
            int buff2 = StoreDataInAttribList(2, 3, normals);

            UnbindVAO();

            return new ModelBlockRaw(vaoID, vertexes, normals, UVs, buff0, buff1, buff2);
        }

        public static ModelRaw LoadModelToVAO(List<RawQuad> quads, int coordSize)
        {
            int vaoID = CreateVAO();

            List<float> vertices = new List<float>();
            List<float> normals = new List<float>();
            List<float> UVs = new List<float>();

            for (int index = 0; index < quads.Count; index++)
            {
                RawQuad quad = quads[index];

                vertices.AddRange(quad.vertices);
                normals.AddRange(quad.normal);
                UVs.AddRange(quad.UVs);
            }

            int buff0 = StoreDataInAttribList(0, coordSize, vertices.ToArray());
            int buff1 = StoreDataInAttribList(1, 2, UVs.ToArray());
            int buff2 = StoreDataInAttribList(2, 3, normals.ToArray());

            GL.Flush();

            UnbindVAO();

            return new ModelRaw(vaoID, coordSize, quads, buff0, buff1, buff2);
        }

        public static ModelRaw OverrideModelInVAO(int ID, int[] buffers, List<RawQuad> quads, int coordSize)
        {
            List<float> vertices = new List<float>();
            List<float> normals = new List<float>();
            List<float> UVs = new List<float>();

            for (int index = 0; index < quads.Count; index++)
            {
                RawQuad quad = quads[index];

                vertices.AddRange(quad.vertices);
                normals.AddRange(quad.normal);
                UVs.AddRange(quad.UVs);
            }

            OverrideDataInAttributeList(buffers[0], 0, coordSize, vertices.ToArray());
            OverrideDataInAttributeList(buffers[1], 1, 2, UVs.ToArray());
            OverrideDataInAttributeList(buffers[2], 2, 3, normals.ToArray());

            return new ModelRaw(ID, coordSize, quads, buffers);
        }

        public static void OverrideModelUVsInVAO(int bufferID, float[] UVs)
        {
            OverrideDataInAttributeList(bufferID, 1, 2, UVs);
            GL.Flush();
        }

        private static void OverrideDataInAttributeList(int ID, int attrib, int coordSize, float[] data)
        {
            GL.BindBuffer(BufferTarget.ArrayBuffer, ID);
            GL.BufferData(BufferTarget.ArrayBuffer, sizeof(float) * data.Length, data, BufferUsageHint.DynamicDraw);
            GL.VertexAttribPointer(attrib, coordSize, VertexAttribPointerType.Float, false, 0, 0);
            GL.BindBuffer(BufferTarget.ArrayBuffer, 0);
        }

        private static int CreateVAO()
        {
            int vaoID = GL.GenVertexArray();

            VAOs.Add(vaoID);

            GL.BindVertexArray(vaoID);

            return vaoID;
        }

        private static void UnbindVAO()
        {
            GL.BindVertexArray(0);
        }

        private static int StoreDataInAttribList(int attrib, int coordSize, float[] data)
        {
            int vboID = GL.GenBuffer();

            VBOs.Add(vboID);

            GL.BindBuffer(BufferTarget.ArrayBuffer, vboID);
            GL.BufferData(BufferTarget.ArrayBuffer, sizeof(float) * data.Length, data, BufferUsageHint.DynamicDraw);
            GL.VertexAttribPointer(attrib, coordSize, VertexAttribPointerType.Float, false, 0, 0);
            GL.BindBuffer(BufferTarget.ArrayBuffer, 0);

            return vboID;
        }

        public static void Cleanup()
        {
            foreach (int vao in VAOs)
                GL.DeleteVertexArray(vao);

            foreach (int vbo in VBOs)
                GL.DeleteVertexArray(vbo);
        }
    }
}