using System.Collections.Generic;
using OpenTK;
using SharpCraft.block;
using SharpCraft.util;

namespace SharpCraft.texture
{
    internal class TextureBlockUV
    {
        private Dictionary<FaceSides, TextureUVNode> UVs;

        public TextureBlockUV()
        {
            UVs = new Dictionary<FaceSides, TextureUVNode>();
        }

        public void setUVForSide(FaceSides side, Vector2 from, Vector2 to)
        {
            if (UVs.ContainsKey(side))
                UVs.Remove(side);

            UVs.Add(side, new TextureUVNode(from, to));
        }

        public TextureUVNode getUVForSide(FaceSides side)
        {
            UVs.TryGetValue(side, out var uv);

            return uv;
        }

        public void fill(Vector2 from, Vector2 to)
        {
            foreach (FaceSides side in FaceSides.AllSides)
            {
                setUVForSide(side, from, to);
            }
        }

        public void fillEmptySides(TextureUVNode with)
        {
            foreach (FaceSides side in FaceSides.AllSides)
            {
                if (getUVForSide(side) == null)
                    setUVForSide(side, with.start, with.end);
            }
        }
    }
}