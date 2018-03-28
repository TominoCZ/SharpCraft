using SharpCraft.block;
using SharpCraft.entity;
using SharpCraft.shader;
using SharpCraft.util;

namespace SharpCraft.model
{
    internal class ModelBlock : ModelBaked
    {
        public EnumBlock block { get; }

        public AxisAlignedBB boundingBox { get; }

        public bool canBeInteractedWith { get; }

        public ModelBlock(EnumBlock block, ShaderProgram shader, bool canBeInteractedWith) : base(null, shader)
        {
            this.block = block;
            this.canBeInteractedWith = canBeInteractedWith;

            var cube = ModelHelper.createTexturedCubeModel(block);

            rawModel = ModelManager.loadBlockModelToVAO(cube);

            boundingBox = AxisAlignedBB.BLOCK_FULL;
        }

        public ModelBlock(EnumBlock block, ShaderProgram shader, AxisAlignedBB bb, bool canBeInteractedWith) : this(block, shader, canBeInteractedWith)
        {
            boundingBox = bb;
        }
    }
}