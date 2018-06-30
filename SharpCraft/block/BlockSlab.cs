using System;
using System.Collections.Generic;
using System.Text;
using OpenTK;
using SharpCraft.entity;

namespace SharpCraft.block
{
    class BlockSlab : Block
    {
        public BlockSlab() : base(Material.GetMaterial("stone"), "slab")
        {
            IsFullCube = false;

            var size = Vector3.One;

            size.Y = 0.5f;

            BoundingBox = new AxisAlignedBB(size);

            Hardness = 64; //TODO - set based on the state
        }
    }
}
