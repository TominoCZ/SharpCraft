using System.Collections.Generic;
using System.Linq;
using SharpCraft.block;

namespace SharpCraft.model
{
    internal class ModelBlockRaw : ModelRaw
    {
        private Dictionary<EnumFacing, RawQuad> _quads;

        public ModelBlockRaw(int vaoID, Dictionary<EnumFacing, RawQuad> _quads, params int[] bufferIDs) : base(vaoID, 3, _quads.Values.ToList(), bufferIDs)
        {
            this._quads = _quads;
        }

        public RawQuad getQuadForSide(EnumFacing side)
        {
            _quads.TryGetValue(side, out var quad);

            return quad;
        }
    }
}