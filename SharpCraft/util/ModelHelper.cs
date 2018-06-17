using OpenTK;
using SharpCraft.block;
using SharpCraft.model;
using SharpCraft.texture;
using System.Collections.Generic;

namespace SharpCraft.util
{
    internal class ModelHelper
    {
        private static Vector3 V2, V3, V4, NORMAL;

        private static Dictionary<FaceSides, float[]> CUBE = new Dictionary<FaceSides, float[]>();

        static ModelHelper()
        {
            CUBE.Add(FaceSides.North, new float[]
            {
                1, 1, 0,
                1, 0, 0,
                0, 0, 0,
                0, 1, 0
            });
            CUBE.Add(FaceSides.South, new float[]
            {
                0, 1, 1,
                0, 0, 1,
                1, 0, 1,
                1, 1, 1
            });
            CUBE.Add(FaceSides.East, new float[]
            {
                1, 1, 1,
                1, 0, 1,
                1, 0, 0,
                1, 1, 0
            });
            CUBE.Add(FaceSides.West, new float[]
            {
                0, 1, 0,
                0, 0, 0,
                0, 0, 1,
                0, 1, 1
            });
            CUBE.Add(FaceSides.Up, new float[]
            {
                0, 1, 0,
                0, 1, 1,
                1, 1, 1,
                1, 1, 0
            });
            CUBE.Add(FaceSides.Down, new float[]
            {
                0, 0, 1,
                0, 0, 0,
                1, 0, 0,
                1, 0, 1
            });
        }

        public static float[] calculateNormals(float[] vertices)
        {
            float[] normals = new float[vertices.Length];

            for (int i = 0; i < vertices.Length; i += 12)
            {
                V2.X = vertices[i + 3];
                V2.Y = vertices[i + 4];
                V2.Z = vertices[i + 5];

                V3.X = vertices[i + 6];
                V3.Y = vertices[i + 7];
                V3.Z = vertices[i + 8];

                V4.X = vertices[i + 9];
                V4.Y = vertices[i + 10];
                V4.Z = vertices[i + 11];

                NORMAL = Vector3.Cross(V4 - V2, V2 - V3);

                for (int j = 0; j < 4; j++)
                {
                    for (int k = 0; k < 3; k++)
                    {
                        normals[i + j * 3 + k] = NORMAL[k];
                    }
                }
            }

            return normals;
        }

        public static Dictionary<FaceSides, RawQuad> createTexturedCubeModel(EnumBlock block)
        {
            Dictionary<FaceSides, RawQuad> quads = new Dictionary<FaceSides, RawQuad>();
            TextureBlockUV uvs = TextureManager.GetUVsFromBlock(block);

            foreach (FaceSides face in CUBE.Keys)
            {
                if (CUBE.TryGetValue(face, out float[] data))
                {
                    TextureUVNode uvNode = uvs.getUVForSide(face);

                    if (uvNode != null)
                    {
                        RawQuad quad = new RawQuad(
                            data,
                            uvNode.ToArray(),
                            calculateNormals(data),
                            3);

                        quads.Add(face, quad);
                    }
                }
            }

            return quads;
        }

        private static float[] toFloats(Vector3 vec)
        {
            return new[]
            {
                vec.X, vec.Y, vec.Z,
                vec.X, vec.Y, vec.Z,
                vec.X, vec.Y, vec.Z,
                vec.X, vec.Y, vec.Z
            };
        }

        public static List<RawQuad> createCubeModel()
        {
            List<RawQuad> quads = new List<RawQuad>();

            foreach (FaceSides face in CUBE.Keys)
            {
                if (CUBE.TryGetValue(face, out float[] vertices))
                {
                    quads.Add(new RawQuad(vertices, 3));
                }
            }

            return quads;
        }
    }
}