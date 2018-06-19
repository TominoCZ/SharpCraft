using SharpCraft.block;
using SharpCraft.entity;
using SharpCraft.render.shader;
using SharpCraft.util;
using System.Collections.Generic;

namespace SharpCraft.model
{
    public class ModelBlock : ModelBaked<ModelBlock>
    {
        public AxisAlignedBB boundingBox { get; }

        public bool hasTransparency { get; }

        public bool canBeInteractedWith { get; }

        public ModelBlock(Shader<ModelBlock> shader, ModelBlockRaw rawModel, bool canBeInteractedWith = false, bool hasTransparency = false) : base(null, shader)
        {
            this.hasTransparency = hasTransparency;
            this.canBeInteractedWith = canBeInteractedWith;

            RawModel = rawModel;

            boundingBox = AxisAlignedBB.BLOCK_FULL;
        }

        public ModelBlock(Shader<ModelBlock> shader, ModelBlockRaw rawModel, AxisAlignedBB bb, bool canBeInteractedWith = false, bool hasTransparency = false) : this(shader, rawModel, canBeInteractedWith, hasTransparency)
        {
            boundingBox = bb;
        }
    }
}