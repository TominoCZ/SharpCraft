using OpenTK.Graphics.OpenGL;
using SharpCraft.block;
using System.Collections.Generic;

namespace SharpCraft.model
{
    internal class ModelManager
    {
        private static readonly List<int> VaOs = new List<int>();
        private static readonly List<int> VbOs = new List<int>();

        public static ModelBlockRaw LoadBlockModelToVao(float[] vertexes, float[] normals, float[] uVs)
        {
            int vaoId = CreateVao();

            int buff0 = StoreDataInAttribList(0, 3, vertexes);
            int buff1 = StoreDataInAttribList(1, 2, uVs);
            int buff2 = StoreDataInAttribList(2, 3, normals);

            UnbindVao();

            return new ModelBlockRaw(vaoId, vertexes, normals, uVs, buff0, buff1, buff2);
        }

        public static ModelRaw LoadModel3ToVao(float[] vertexes, float[] normals, float[] uvs)
        {
            int vaoId = CreateVao();

            int buff0 = StoreDataInAttribList(0, 3, vertexes);
            int buff1 = StoreDataInAttribList(1, 2, uvs);
            int buff2 = StoreDataInAttribList(2, 3, normals);

            UnbindVao();

            return new ModelRaw(vaoId, vertexes.Length / 3, buff0, buff1, buff2);
        }
        
        public static ModelRaw LoadModel3ToVao(float[] vertexes)
        {
            int vaoId = CreateVao();

            int buff0 = StoreDataInAttribList(0, 3, vertexes);

            UnbindVao();

            return new ModelRaw(vaoId, vertexes.Length / 3, buff0);
        }
        
        public static ModelRaw OverrideModel3InVao(int id, int[] buffers, float[] vertexes, float[] normals, float[] uvs)
        {
            OverrideDataInAttributeList(buffers[0], 0, 3, vertexes);
            OverrideDataInAttributeList(buffers[1], 1, 2, uvs);
            OverrideDataInAttributeList(buffers[2], 2, 3, normals);

            return new ModelRaw(id, vertexes.Length / 3, buffers);
        }

        public static ModelRaw LoadModel2ToVao(float[] vertexes, float[] uvs)
        {
            int vaoId = CreateVao();

            int buff0 = StoreDataInAttribList(0, 2, vertexes);
            int buff1 = StoreDataInAttribList(1, 2, uvs);

            UnbindVao();

            return new ModelRaw(vaoId, vertexes.Length / 2, buff0, buff1);
        }

        public static ModelRaw LoadModel2ToVao(float[] vertexes)
        {
            int vaoId = CreateVao();

            int buff0 = StoreDataInAttribList(0, 2, vertexes);

            UnbindVao();

            return new ModelRaw(vaoId, vertexes.Length / 2, buff0);
        }

        private static void OverrideDataInAttributeList(int id, int attrib, int coordSize, float[] data)
        {
            GL.BindBuffer(BufferTarget.ArrayBuffer, id);
            GL.BufferData(BufferTarget.ArrayBuffer, sizeof(float) * data.Length, data, BufferUsageHint.DynamicDraw);
            GL.VertexAttribPointer(attrib, coordSize, VertexAttribPointerType.Float, false, 0, 0);
            GL.BindBuffer(BufferTarget.ArrayBuffer, 0);
        }

        private static int CreateVao()
        {
            int vaoId = GL.GenVertexArray();

            VaOs.Add(vaoId);

            GL.BindVertexArray(vaoId);

            return vaoId;
        }

        private static void UnbindVao()
        {
            GL.BindVertexArray(0);
        }

        private static int StoreDataInAttribList(int attrib, int coordSize, float[] data)
        {
            int vboId = GL.GenBuffer();

            VbOs.Add(vboId);

            GL.BindBuffer(BufferTarget.ArrayBuffer, vboId);
            GL.BufferData(BufferTarget.ArrayBuffer, sizeof(float) * data.Length, data, BufferUsageHint.DynamicDraw);
            GL.VertexAttribPointer(attrib, coordSize, VertexAttribPointerType.Float, false, 0, 0);
            GL.BindBuffer(BufferTarget.ArrayBuffer, 0);

            return vboId;
        }

        public static void Cleanup()
        {
            foreach (int vao in VaOs)
                GL.DeleteVertexArray(vao);

            foreach (int vbo in VbOs)
                GL.DeleteVertexArray(vbo);
        }
    }
}