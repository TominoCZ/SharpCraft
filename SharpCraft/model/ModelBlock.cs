﻿using SharpCraft.block;
using SharpCraft.entity;
using SharpCraft.render.shader;
using SharpCraft.util;

namespace SharpCraft.model
{
    internal class ModelBlock : ModelBaked<ModelBlock>
    {
        public EnumBlock block { get; }

        public AxisAlignedBb boundingBox { get; }

        public bool hasTransparency { get; }

        public bool canBeInteractedWith { get; }

        public ModelBlock(EnumBlock block, Shader<ModelBlock> shader, bool canBeInteractedWith = false, bool hasTransparency = false) : base(null, shader)
        {
            this.block = block;
            this.hasTransparency = hasTransparency;
            this.canBeInteractedWith = canBeInteractedWith;

            var cube = ModelHelper.createTexturedCubeModel(block);

            rawModel = ModelManager.loadBlockModelToVAO(cube);

            boundingBox = AxisAlignedBb.BLOCK_FULL;
        }

        public ModelBlock(EnumBlock block, Shader<ModelBlock> shader, AxisAlignedBb bb, bool canBeInteractedWith = false, bool hasTransparency = false) : this(block, shader, canBeInteractedWith, hasTransparency)
        {
            boundingBox = bb;
        }
    }
}