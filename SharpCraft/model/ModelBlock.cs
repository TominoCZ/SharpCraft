using SharpCraft.block;
using SharpCraft.entity;
using SharpCraft.render.shader;
using SharpCraft.util;
using System.Collections.Generic;

namespace SharpCraft.model
{
    internal class ModelBlock : ModelBaked<ModelBlock>
    {
        public EnumBlock block { get; }

        public AxisAlignedBB boundingBox { get; }

        public bool hasTransparency { get; }

        public bool canBeInteractedWith { get; }

        public ModelBlock(EnumBlock block, Shader<ModelBlock> shader, bool canBeInteractedWith = false, bool hasTransparency = false) : base(null, shader)
        {
            this.block = block;
            this.hasTransparency = hasTransparency;
            this.canBeInteractedWith = canBeInteractedWith;

            Dictionary<FaceSides, RawQuad> cube = ModelHelper.createTexturedCubeModel(block);

            RawModel = ModelManager.LoadBlockModelToVAO(cube);

            boundingBox = AxisAlignedBB.BLOCK_FULL;
        }

        public ModelBlock(EnumBlock block, Shader<ModelBlock> shader, AxisAlignedBB bb, bool canBeInteractedWith = false, bool hasTransparency = false) : this(block, shader, canBeInteractedWith, hasTransparency)
        {
            boundingBox = bb;
        }
    }
}