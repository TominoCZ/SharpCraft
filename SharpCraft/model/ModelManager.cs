using System.Collections.Generic;
using OpenTK.Graphics.OpenGL;
using SharpCraft.block;

namespace SharpCraft.model
{
    internal class ModelManager
    {
        private static List<int> VAOs = new List<int>();
        private static List<int> VBOs = new List<int>();

        public static ModelBlockRaw loadBlockModelToVAO(Dictionary<EnumFacing, RawQuad> quads)
        {
            int vaoID = createVAO();

            List<float> vertices = new List<float>();
            List<float> normals = new List<float>();
            List<float> UVs = new List<float>();

            foreach (var q in quads)
            {
                vertices.AddRange(q.Value.vertices);
                normals.AddRange(q.Value.normal);
                UVs.AddRange(q.Value.UVs);
            }

            int buff0 = storeDataInAttribList(0, 3, vertices.ToArray());
            int buff1 = storeDataInAttribList(1, 2, UVs.ToArray());
            int buff2 = storeDataInAttribList(2, 3, normals.ToArray());

            unbindVAO();

            return new ModelBlockRaw(vaoID, quads, buff0, buff1, buff2);
        }

        public static ModelRaw loadModelToVAO(List<RawQuad> quads, int coordSize)
        {
            int vaoID = createVAO();

            List<float> vertices = new List<float>();
            List<float> normals = new List<float>();
            List<float> UVs = new List<float>();

            for (var index = 0; index < quads.Count; index++)
            {
                var quad = quads[index];

                vertices.AddRange(quad.vertices);
                normals.AddRange(quad.normal);
                UVs.AddRange(quad.UVs);
            }

            var buff0 = storeDataInAttribList(0, coordSize, vertices.ToArray());
            var buff1 = storeDataInAttribList(1, 2, UVs.ToArray());
            var buff2 = storeDataInAttribList(2, 3, normals.ToArray());

            unbindVAO();

            return new ModelRaw(vaoID, coordSize, quads, buff0, buff1, buff2);
        }

        public static ModelRaw overrideModelInVAO(int ID, int[] buffers, List<RawQuad> quads, int coordSize)
        {
            List<float> vertices = new List<float>();
            List<float> normals = new List<float>();
            List<float> UVs = new List<float>();

            for (var index = 0; index < quads.Count; index++)
            {
                var quad = quads[index];

                vertices.AddRange(quad.vertices);
                normals.AddRange(quad.normal);
                UVs.AddRange(quad.UVs);
            }

            overrideDataInAttributeList(buffers[0], 0, coordSize, vertices.ToArray());
            overrideDataInAttributeList(buffers[1], 1, 2, UVs.ToArray());
            overrideDataInAttributeList(buffers[2], 2, 3, normals.ToArray());

            return new ModelRaw(ID, coordSize, quads, buffers);
        }

        public static void overrideModelUVsInVAO(int bufferID, float[] UVs)
        {
            overrideDataInAttributeList(bufferID, 1, 2, UVs);
        }

        private static void overrideDataInAttributeList(int ID, int attrib, int coordSize, float[] data)
        {
            GL.BindBuffer(BufferTarget.ArrayBuffer, ID);
            GL.BufferData(BufferTarget.ArrayBuffer, sizeof(float) * data.Length, data, BufferUsageHint.DynamicDraw);
            GL.VertexAttribPointer(attrib, coordSize, VertexAttribPointerType.Float, false, 0, 0);
            GL.BindBuffer(BufferTarget.ArrayBuffer, 0);
        }

        private static int createVAO()
        {
            int vaoID = GL.GenVertexArray();

            VAOs.Add(vaoID);

            GL.BindVertexArray(vaoID);

            return vaoID;
        }

        private static void unbindVAO()
        {
            GL.BindVertexArray(0);
        }

        private static int storeDataInAttribList(int attrib, int coordSize, float[] data)
        {
            int vboID = GL.GenBuffer();

            VBOs.Add(vboID);

            GL.BindBuffer(BufferTarget.ArrayBuffer, vboID);
            GL.BufferData(BufferTarget.ArrayBuffer, sizeof(float) * data.Length, data, BufferUsageHint.DynamicDraw);
            GL.VertexAttribPointer(attrib, coordSize, VertexAttribPointerType.Float, false, 0, 0);
            GL.BindBuffer(BufferTarget.ArrayBuffer, 0);

            return vboID;
        }

        public static void cleanup()
        {
            foreach (var vao in VAOs)
                GL.DeleteVertexArray(vao);

            foreach (var vbo in VBOs)
                GL.DeleteVertexArray(vbo);
        }
    }
}