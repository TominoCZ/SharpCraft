using System.Collections.Generic;
using System.Linq;

namespace SharpCraft
{
    class ModelBlockRaw : ModelRaw
    {
        private Dictionary<EnumFacing, RawQuad> _quads;

        public ModelBlockRaw(int vaoID, Dictionary<EnumFacing, RawQuad> _quads, params int[] bufferIDs) : base(vaoID, 3, _quads.Values.ToList(), bufferIDs)
        {
            this._quads = _quads;

            foreach (var value in _quads)
            {
                vertexCount += value.Value.vertices.Length / 3;

                var uv = value.Value.UVs.Length > 0;
                var normal = value.Value.normal.Length > 0;

                if (uv)
                    hasUVs = true;
                if (normal)
                    hasNormals = true;
            }
        }

        public RawQuad getQuadForSide(EnumFacing side)
        {
            _quads.TryGetValue(side, out var quad);

            return quad;
        }
    }
}
