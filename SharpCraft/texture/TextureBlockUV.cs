using System;
using System.Collections.Generic;
using OpenTK;

namespace SharpCraft
{
    class TextureBlockUV
    {
        private Dictionary<EnumFacing, TextureUVNode> UVs;

        public TextureBlockUV()
        {
            UVs = new Dictionary<EnumFacing, TextureUVNode>();
        }

        public void setUVForSide(EnumFacing side, Vector2 from, Vector2 to)
        {
            if (UVs.ContainsKey(side))
                UVs.Remove(side);

            UVs.Add(side, new TextureUVNode(from, to));
        }

        public TextureUVNode getUVForSide(EnumFacing side)
        {
            UVs.TryGetValue(side, out var uv);

            return uv;
        }

        public void fill(Vector2 from, Vector2 to)
        {
            foreach (EnumFacing side in FacingUtil.SIDES)
            {
                setUVForSide(side, from, to);
            }
        }

        public void fillEmptySides(TextureUVNode with)
        {
            foreach (EnumFacing side in FacingUtil.SIDES)
            {
                if (getUVForSide(side) == null)
                    setUVForSide(side, with.start, with.end);
            }
        }
    }
}
