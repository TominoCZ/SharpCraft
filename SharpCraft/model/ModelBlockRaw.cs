using OpenTK;
using SharpCraft.block;
using SharpCraft.texture;
using System.Collections.Generic;
using System.Linq;

namespace SharpCraft.model
{
    internal class ModelBlockRaw : ModelRaw
    {
        private Dictionary<FaceSides, RawQuad> _quads;

        public ModelBlockRaw(int vaoID, Dictionary<FaceSides, RawQuad> _quads, params int[] bufferIDs) : base(vaoID, 3, _quads.Values.ToList(), bufferIDs)
        {
            this._quads = _quads;
        }

        public RawQuad GetQuadForSide(FaceSides side)
        {
            _quads.TryGetValue(side, out var quad);

            return quad;
        }

        public TextureUVNode GetUVs(FaceSides side)
        {
            if (_quads.TryGetValue(side, out var quad))
            {
                if (quad.UVs.Length == 8)
                {
                    var start = new Vector2(quad.UVs[0], quad.UVs[1]);
                    var end = new Vector2(quad.UVs[4], quad.UVs[5]);

                    return new TextureUVNode(start, end);
                }
            }

            return TextureManager.GetUVsFromBlock(EnumBlock.MISSING).getUVForSide(FaceSides.South);
        }
    }
}