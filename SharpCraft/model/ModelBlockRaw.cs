using SharpCraft.block;
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

        public RawQuad getQuadForSide(FaceSides side)
        {
            _quads.TryGetValue(side, out var quad);

            return quad;
        }
    }
}