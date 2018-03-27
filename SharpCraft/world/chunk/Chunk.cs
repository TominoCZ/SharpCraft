using OpenTK;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace SharpCraft
{
    internal class Chunk
    {
        internal short[,,] _chunkBlocks;

        private bool withinRenderDistance;

        public bool isDirty { get; private set; }

        public BlockPos chunkPos { get; }

        public AxisAlignedBB boundingBox { get; }

        public World world { get; }

        public Chunk(BlockPos chunkPos, World world)
        {
            this.chunkPos = chunkPos;
            boundingBox =new AxisAlignedBB(Vector3.Zero, Vector3.One * 16 + Vector3.UnitY * 240).offset(chunkPos.vector);

            _chunkBlocks = new short[16, 256, 16];
        }

        private Chunk(BlockPos chunkPos, short[,,] blockData)
        {
            this.chunkPos = chunkPos;
            boundingBox =
                new AxisAlignedBB(Vector3.Zero, Vector3.One * 16 + Vector3.UnitY * 240).offset(chunkPos.vector);

            _chunkBlocks = blockData;
        }

        public static Chunk CreateWithData(BlockPos chunkPos, short[,,] blockData)
        {
            return new Chunk(chunkPos, blockData);
        }

        public void tick()
        {
            var pos = chunkPos.offset(8, 0, 8);
            
            var dist = MathUtil.distance(pos.vector.Xz, Game.INSTANCE.Camera.pos.Xz);

            withinRenderDistance = dist <= Game.INSTANCE.worldRenderer.RenderDistance * 16;
        }

        public void setBlock(BlockPos pos, EnumBlock blockType, int meta)
        {
            short id = (short)((short)blockType << 4 | meta);

            if (isPosInChunk(pos))
                _chunkBlocks[pos.x, pos.y, pos.z] = id;
        }

        public EnumBlock getBlock(BlockPos pos)
        {
            if (pos.y >= 0 && pos.y < 256)
            {
                if (isPosInChunk(pos))
                    return (EnumBlock)(_chunkBlocks[pos.x, pos.y, pos.z] >> 4);

                var block = world.getBlock(pos + chunkPos);

                return block;
            }

            return EnumBlock.AIR;
        }

        public int getMetadata(BlockPos pos)
        {
            if (isPosInChunk(pos))
                return _chunkBlocks[pos.x, pos.y, pos.z] & 15;

            return world.getMetadata(pos + chunkPos);
        }

        public void setMetadata(BlockPos pos, int meta, bool redraw)
        {
            if (isPosInChunk(pos))
            {
                var id = _chunkBlocks[pos.x, pos.y, pos.z];

                _chunkBlocks[pos.x, pos.y, pos.z] = (short)(id & 4095 | meta);
            }

            world.setMetadata(pos + chunkPos, meta, redraw);
        }

        private bool isPosInChunk(BlockPos pos)
        {
            return
                pos.x >= 0 && pos.x < 16 &&
                pos.y >= 0 && pos.y < 256 &&
                pos.z >= 0 && pos.z < 16;
        }

        public void buildChunkModel(ModelChunk previousChunkModel)
        {
            var MODEL_RAW = new Dictionary<ShaderProgram, List<RawQuad>>();

            List<RawQuad> quads;

            var sw = Stopwatch.StartNew();

            //generate the model / fill MODEL_RAW
            for (int z = 0; z < 16; z++)
            {
                for (int y = 0; y < 256; y++)
                {
                    for (int x = 0; x < 16; x++)
                    {
                        var pos = new BlockPos(x, y, z);

                        var block = getBlock(pos);

                        if (block == EnumBlock.AIR)
                            continue;

                        var blockModel = ModelRegistry.getModelForBlock(block, getMetadata(pos));

                        if (!MODEL_RAW.TryGetValue(blockModel.shader, out quads))
                            MODEL_RAW.Add(blockModel.shader, quads = new List<RawQuad>());

                        for (int i = 0; i < FacingUtil.SIDES.Length; i++)
                        {
                            var dir = FacingUtil.SIDES[i];
                            var block_o = getBlock(pos.offset(dir));

                            if (block_o == EnumBlock.AIR || block_o == EnumBlock.GLASS && block != EnumBlock.GLASS)
                            {
                                var quad = ((ModelBlockRaw)blockModel.rawModel)?.getQuadForSide(dir)?.offset(pos);

                                if (quad != null)
                                    quads.Add(quad);
                            }
                        }
                    }
                }
            }

            sw.Stop();
            Console.WriteLine($"DEBUG: built chunk model [{sw.Elapsed.TotalMilliseconds:F}ms]");

            //var newShaders = MODEL_RAW.Keys.ToArray();

            foreach (var oldShader in previousChunkModel.fragmentPerShader.Keys)
            {
                if (!MODEL_RAW.Keys.Contains(oldShader))
                {
                    Game.INSTANCE.runGlContext(() => previousChunkModel.destroyFragmentModelWithShader(oldShader));
                }
            }

            foreach (var value in MODEL_RAW)
            {
                var newShader = value.Key;
                var newData = value.Value;

                if (!previousChunkModel.fragmentPerShader.Keys.Contains(newShader))
                {
                    Game.INSTANCE.runGlContext(() =>
                    {
                        var newFragment = new ModelChunkFragment(newShader, newData);
                        previousChunkModel.setFragmentModelWithShader(newShader, newFragment);
                    });
                }
                else
                {
                    Game.INSTANCE.runGlContext(() => previousChunkModel.getFragmentModelWithShader(newShader)?.overrideData(newData));
                }
            }

            isDirty = false;
        }

        public void markDirty()
        {
            isDirty = true;
        }

        public bool isWithinRenderDistance()
        {
            return withinRenderDistance;
        }
    }

}