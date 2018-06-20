using SharpCraft.block;
using System;
using System.Collections.Generic;
using OpenTK;

namespace SharpCraft.model
{
    internal static class CubeModelBuilder
    {
        private static readonly Dictionary<FaceSides, float[]> _cube = new Dictionary<FaceSides, float[]>();

        static CubeModelBuilder()
        {
            _cube.Add(FaceSides.North, new float[]
            {
                1, 1, 0,
                1, 0, 0,
                0, 0, 0,
                0, 1, 0
            });
            _cube.Add(FaceSides.South, new float[]
            {
                0, 1, 1,
                0, 0, 1,
                1, 0, 1,
                1, 1, 1
            });
            _cube.Add(FaceSides.East, new float[]
            {
                1, 1, 1,
                1, 0, 1,
                1, 0, 0,
                1, 1, 0
            });
            _cube.Add(FaceSides.West, new float[]
            {
                0, 1, 0,
                0, 0, 0,
                0, 0, 1,
                0, 1, 1
            });
            _cube.Add(FaceSides.Up, new float[]
            {
                0, 1, 0,
                0, 1, 1,
                1, 1, 1,
                1, 1, 0
            });
            _cube.Add(FaceSides.Down, new float[]
            {
                0, 0, 1,
                0, 0, 0,
                1, 0, 0,
                1, 0, 1
            });
        }

        public static void AppendCubeModel(JsonCube cube, Dictionary<string, string> modelTextureVariables, Dictionary<string, TextureMapElement> textureMap, ref float[] vertexes, ref float[] normals, ref float[] uvs, int n)
        {
            int startIndex2 = n * 48;
            int startIndex3 = n * 72;

            int faceIndex = 0;

            foreach (var pair in cube.faces)
            {
                int uvIndex = 8 * faceIndex;

                TextureType side = pair.Key;
                JsonCubeFaceUv textureNode = pair.Value;

                //edit: textureNode.Texture can be anything. it is a variable defined by the modeller
                //textureNode.Texture isn't the name of the texture file! it is '#side', '#block', '#bottom', ... TODO - if '#' is not present, use the texture from the texture map
                if (textureNode.texture[0] == '#')
                {
                    string faceTextureVariable = textureNode.texture.Substring(1);

                    string textureNameForFace = modelTextureVariables[faceTextureVariable];

                    if (textureMap.TryGetValue(textureNameForFace, out var tme))
                    {
                        var percentageU1 = textureNode.uv[0] / 16f;
                        var percentageV1 = textureNode.uv[1] / 16f;
                        var percentageU2 = textureNode.uv[2] / 16f;
                        var percentageV2 = textureNode.uv[3] / 16f;

                        Vector2 size = tme.UVMax - tme.UVMin;

                        var minU = tme.UVMin.X + size.X * percentageU1;
                        var minV = tme.UVMin.Y + size.Y * percentageV1;
                        var maxU = tme.UVMin.X + size.X * percentageU2;
                        var maxV = tme.UVMin.Y + size.Y * percentageV2;

                        uvs[startIndex2 + uvIndex] = minU;
                        uvs[startIndex2 + uvIndex + 1] = minV;

                        uvs[startIndex2 + uvIndex + 2] = minU;
                        uvs[startIndex2 + uvIndex + 3] = maxV;

                        uvs[startIndex2 + uvIndex + 4] = maxU;
                        uvs[startIndex2 + uvIndex + 5] = maxV;

                        uvs[startIndex2 + uvIndex + 6] = maxU;
                        uvs[startIndex2 + uvIndex + 7] = minV;
                    }
                    //}
                }

                AppendFace(side, cube.@from, cube.to, ref vertexes, ref normals, startIndex3 + 12 * faceIndex);

                faceIndex++;
            }
        }

        public static void AppendFace(TextureType side, float[] from, float[] to, ref float[] vertexes, ref float[] normals, int startIndex)
        {
            FaceSides normal = FaceSides.Parse(side); //TextureType parsed to FaceSides, also a normal of this face
            float[] unitFace = _cube[normal]; //one side of the cube in unit size

            float x = from[0] / 16f;
            float y = from[1] / 16f;
            float z = from[2] / 16f;

            float sx = to[0] / 16f - x; //the size of the cube part
            float sy = to[1] / 16f - y;
            float sz = to[2] / 16f - z;

            for (var i = 0; i < unitFace.Length; i += 3)
            {
                float vx = unitFace[i] * sx;
                float vy = unitFace[i + 1] * sy;
                float vz = unitFace[i + 2] * sz;

                vertexes[startIndex + i] = x + vx;
                vertexes[startIndex + i + 1] = y + vy;
                vertexes[startIndex + i + 2] = z + vz;

                normals[startIndex + i] = normal.x;
                normals[startIndex + i + 1] = normal.y;
                normals[startIndex + i + 2] = normal.z;
            }
        }

        public static float[] CreateCubeVertexes()
        {
            List<float> vertexes = new List<float>();

            void AppendVertexes(TextureType side)
            {
                vertexes.AddRange(_cube[FaceSides.Parse(side)]);
            }

            AppendVertexes(TextureType.up);
            AppendVertexes(TextureType.down);
            AppendVertexes(TextureType.north);
            AppendVertexes(TextureType.south);
            AppendVertexes(TextureType.east);
            AppendVertexes(TextureType.west);

            return vertexes.ToArray();
        }

        public static float[] CreateCubeNormals()
        {
            List<float> normals = new List<float>();

            void AppendVertexes(TextureType side)
            {
                var normal = FaceSides.Parse(side);

                for (int i = 0; i < 4; i++)
                {
                    normals.Add(normal.x);
                    normals.Add(normal.y);
                    normals.Add(normal.z);
                }
            }

            AppendVertexes(TextureType.up);
            AppendVertexes(TextureType.down);
            AppendVertexes(TextureType.north);
            AppendVertexes(TextureType.south);
            AppendVertexes(TextureType.east);
            AppendVertexes(TextureType.west);

            return normals.ToArray();
        }

        public static float[] CreateCubeUvs()
        {
            List<float> uvs = new List<float>();

            float[] faceUv =
            {
                0, 0,
                0, 1,
                1, 1,
                1, 0
            };

            for (int i = 0; i < 6; i++)
            {
                uvs.AddRange(faceUv);
            }

            return uvs.ToArray();
        }
    }
}