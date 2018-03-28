using System.Collections.Generic;
using OpenTK;
using SharpCraft.block;
using SharpCraft.model;
using SharpCraft.texture;

namespace SharpCraft.util
{
    internal class ModelHelper
    {
        private static Vector3 V0, V1, V2, NORMAL;

        private static Dictionary<EnumFacing, float[]> CUBE = new Dictionary<EnumFacing, float[]>();

        static ModelHelper()
        {
            CUBE.Add(EnumFacing.NORTH, new float[]
            {
                1, 1, 0,
                1, 0, 0,
                0, 0, 0,
                0, 1, 0
            });
            CUBE.Add(EnumFacing.SOUTH, new float[]
            {
                0, 1, 1,
                0, 0, 1,
                1, 0, 1,
                1, 1, 1
            });
            CUBE.Add(EnumFacing.EAST, new float[]
            {
                1, 1, 1,
                1, 0, 1,
                1, 0, 0,
                1, 1, 0
            });
            CUBE.Add(EnumFacing.WEST, new float[]
            {
                0, 1, 0,
                0, 0, 0,
                0, 0, 1,
                0, 1, 1
            });
            CUBE.Add(EnumFacing.UP, new float[]
            {
                0, 1, 0,
                0, 1, 1,
                1, 1, 1,
                1, 1, 0
            });
            CUBE.Add(EnumFacing.DOWN, new float[]
            {
                0, 0, 1,
                0, 0, 0,
                1, 0, 0,
                1, 0, 1
            });
        }

        public static float[] calculateNormals(float[] vertices)
        {
            var normals = new float[vertices.Length];

            for (var i = 0; i < vertices.Length; i += 12)
            {
                V0.Z = vertices[i];
                V0.Y = vertices[i + 1];
                V0.X = vertices[i + 2];

                V1.Z = vertices[i + 3];
                V1.Y = vertices[i + 4];
                V1.X = vertices[i + 5];

                V2.Z = vertices[i + 6];
                V2.Y = vertices[i + 7];
                V2.X = vertices[i + 8];

                NORMAL = Vector3.Cross(V1 - V2, V0 - V1);

                for (var j = 0; j < 4; j++)
                {
                    normals[i + j * 3] = NORMAL.X;
                    normals[i + j * 3 + 1] = NORMAL.Y;
                    normals[i + j * 3 + 2] = NORMAL.Z;
                }
            }

            return normals;
        }

        public static Dictionary<EnumFacing, RawQuad> createTexturedCubeModel(EnumBlock block)
        {
            var quads = new Dictionary<EnumFacing, RawQuad>();
            var uvs = TextureManager.getUVsFromBlock(block);

            foreach (var face in CUBE.Keys)
            {
                if (CUBE.TryGetValue(face, out var data))
                {
                    var uvNode = uvs.getUVForSide(face);

                    if (uvNode != null)
                    {
                        var quad = new RawQuad(data, uvNode.ToArray(), calculateNormals(data), 3);

                        quads.Add(face, quad);
                    }
                }
            }

            return quads;
        }

        public static List<RawQuad> createCubeModel()
        {
            var quads = new List<RawQuad>();

            foreach (var face in CUBE.Keys)
            {
                if (CUBE.TryGetValue(face, out var vertices))
                {
                    quads.Add(new RawQuad(vertices, 3));
                }
            }

            return quads;
        }
    }
}