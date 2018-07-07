﻿using OpenTK;
using SharpCraft.entity;

namespace SharpCraft.block
{
    public class BlockTulipOrange : Block
    {
        public BlockTulipOrange() : base(Material.GetMaterial("tallgrass"))
        {
            SetUnlocalizedName("sharpcraft", "tulip_orange");

            IsFullCube = false;
            IsReplaceable = true;

            BoundingBox = new AxisAlignedBb(0.85f).Offset(new Vector3(0.075f, 0, 0.075f));

            Hardness = 0;
        }
    }
}