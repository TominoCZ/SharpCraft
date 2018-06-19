using SharpCraft.block;
using System;
using System.Collections.Generic;

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

        public static void AppendCubeModel(JsonCube cube, Dictionary<TextureType, string> modelTextures, Dictionary<string, TextureMapElement> textureMap, ref float[] vertexes, ref float[] normals, ref float[] uvs, int n)
        {
            int startIndex2 = n * 48;
            int startIndex3 = n * 72;

            int faceIndex = 0;

            foreach (var pair in cube.Faces)
            {
                int uvIndex = 8 * faceIndex;

                TextureType side = pair.Key;
                JsonCubeFaceUv textureNode = pair.Value;

                //edit: textureNode.Texture can be anything. it is a variable defined by the modeller
                //textureNode.Texture isn't the name of the texture file! it is '#side', '#block', '#bottom', ... TODO - if '#' is not present, use the texture from the texture map
                if (textureNode.Texture[0] == '#')
                {
                    string sideName = textureNode.Texture.Substring(1).ToLower();

                    if (Enum.TryParse(sideName, out TextureType sideParsed))
                    {
                        string textureNameForFace = modelTextures[sideParsed];

                        if (textureMap.TryGetValue(textureNameForFace, out var tme))
                        {
                            var minU = tme.UVMin.X + textureNode.UV[0] / 16f / 16f;
                            var minV = tme.UVMin.Y + textureNode.UV[1] / 16f / 16f;
                            var maxU = minU + textureNode.UV[2] / 16f / 16f;
                            var maxV = minV + textureNode.UV[3] / 16f / 16f;

                            uvs[startIndex2 + uvIndex] = minU;
                            uvs[startIndex2 + uvIndex + 1] = minV;

                            uvs[startIndex2 + uvIndex + 2] = minU;
                            uvs[startIndex2 + uvIndex + 3] = maxV;

                            uvs[startIndex2 + uvIndex + 4] = maxU;
                            uvs[startIndex2 + uvIndex + 5] = maxV;

                            uvs[startIndex2 + uvIndex + 6] = maxU;
                            uvs[startIndex2 + uvIndex + 7] = minV;
                        }
                    }
                }

                AppendFace(side, cube.From, cube.To, ref vertexes, ref normals, startIndex3 + 12 * faceIndex);

                faceIndex++;
            }
        }

        public static void AppendFace(TextureType side, int[] from, int[] to, ref float[] vertexes, ref float[] normals, int startIndex)
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
    }
}