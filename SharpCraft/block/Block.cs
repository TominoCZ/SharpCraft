using System;
using System.Collections.Generic;
using System.Text;
using SharpCraft.model;

namespace SharpCraft.block
{
    class Block
    {
        public int Hardness;

        public bool IsOpaque;
        public bool HasTransparency;

        public Block()
        {
            Hardness = 8;
            IsOpaque = true;
            HasTransparency = false;
        }
    }

    internal class BlockState
    {
        public ModelBlock Model { get; }
        public int meta { get; }

        public BlockState(ModelBlock model, int meta)
        {
            Model = model;

            this.meta = meta;
        }
    }
}
