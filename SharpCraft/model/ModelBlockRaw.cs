using OpenTK;
using SharpCraft.block;
using SharpCraft.texture;
using System.Collections.Generic;
using System.Linq;

namespace SharpCraft.model
{
    public class ModelBlockRaw : ModelRaw
    {
        private Dictionary<FaceSides, RawQuad> _quads;

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

        public float[] GetVertexesForSide(FaceSides sides)
        {
            return null; //TODO
        }

        public float[] GetUVsForSide(FaceSides sides)
        {
            return null; //TODO
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