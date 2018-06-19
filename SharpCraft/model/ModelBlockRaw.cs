using OpenTK;
using SharpCraft.block;
using SharpCraft.texture;
using System.Collections.Generic;
using System.Linq;

namespace SharpCraft.model
{
    public class ModelBlockRaw : ModelRaw
    {
        //private Dictionary<FaceSides, RawQuad> _quads;

        private float[] _vertexes;
        private float[] _normals;
        private float[] _uvs;

        public ModelBlockRaw(int vaoID, float[] vertexes, float[] normals, float[] uvs, params int[] bufferIDs) : base(vaoID, vertexes.Length / 3, bufferIDs)
        {
            _vertexes = vertexes;
            _normals = normals;
            _uvs = uvs;
        }

        /*
        public ModelBlockRaw(int vaoID, Dictionary<FaceSides, RawQuad> quads, params int[] bufferIDs) : base(vaoID, 3, quads.Values.ToList(), bufferIDs)
        {
            _quads = quads;
        }*/

        public void AppendVertexesForSide(FaceSides side, ref float[] vertexes, int startIndex)
        {
            /*
               top,
               bottom,
               north,
               south,
               west,
               east,
            */
            TextureType parsed = FaceSides.Parse(side);
            int faceIndex = (int)parsed * 12;

            for (int i = 0; i < 12; i += 3)
            {
                vertexes[startIndex + faceIndex + i] = _vertexes[faceIndex + i];
                vertexes[startIndex + faceIndex + i + 1] = _vertexes[faceIndex + i + 1];
                vertexes[startIndex + faceIndex + i + 2] = _vertexes[faceIndex + i + 2];
            }
        }

        public void AppendNormalsForSide(FaceSides side, ref float[] normals, int startIndex)
        {
            TextureType parsed = FaceSides.Parse(side);
            int faceIndex = (int)parsed * 12;

            for (int i = 0; i < 8; i += 2)
            {
                normals[startIndex + faceIndex + i] = _normals[faceIndex + i];
                normals[startIndex + faceIndex + i + 1] = _normals[faceIndex + i + 1];
            }
        }
        /*
        public RawQuad GetQuadForSide(FaceSides side)
        {
            _quads.TryGetValue(side, out RawQuad quad);

            return quad;
        }

        public TextureUVNode GetUVs(FaceSides side)
        {
            if (_quads.TryGetValue(side, out RawQuad quad))
            {
                if (quad.UVs.Length == 8)
                {
                    Vector2 start = new Vector2(quad.UVs[0], quad.UVs[1]);
                    Vector2 end = new Vector2(quad.UVs[4], quad.UVs[5]);

                    return new TextureUVNode(start, end);
                }
            }

            return TextureManager.GetUVsFromBlock(EnumBlock.MISSING).getUVForSide(FaceSides.South);
        }*/
    }
}