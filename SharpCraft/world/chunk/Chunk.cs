using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using OpenTK;
using SharpCraft.block;
using SharpCraft.entity;
using SharpCraft.model;
using SharpCraft.shader;
using SharpCraft.util;

namespace SharpCraft.world.chunk
{
    internal class Chunk
    {
        internal short[,,] ChunkBlocks;

        private bool _withinRenderDistance;

        public bool IsDirty { get; private set; }
        public bool NeedsSave { get; set; }

        public BlockPos ChunkPos { get; }

        public AxisAlignedBB BoundingBox { get; }

        public World World { get; }

        public Chunk(BlockPos chunkPos, World world)
        {
            ChunkPos = chunkPos;
            World = world;
            
            BoundingBox = new AxisAlignedBB(Vector3.Zero, Vector3.One * 16 + Vector3.UnitY * 240).offset(chunkPos.toVec());

            ChunkBlocks = new short[16, 256, 16];
        }

        private Chunk(BlockPos chunkPos, World world, short[,,] blockData)
        {
            ChunkPos = chunkPos;
            World = world;
            BoundingBox = new AxisAlignedBB(Vector3.Zero, Vector3.One * 16 + Vector3.UnitY * 240).offset(chunkPos.toVec());

            ChunkBlocks = blockData;
        }

        public static Chunk CreateWithData(BlockPos chunkPos, World world, short[,,] blockData)
        {
            return new Chunk(chunkPos, world, blockData);
        }

        public void Tick()
        {
            var pos = ChunkPos.offset(8, 0, 8);
            
            var dist = MathUtil.Distance(pos.toVec().Xz, Game.Instance.Camera.pos.Xz);

            _withinRenderDistance = dist <= Game.Instance.WorldRenderer.RenderDistance * 16;
        }

        public void SetBlock(BlockPos pos, EnumBlock blockType, int meta)
        {
            short id = (short)((short)blockType << 4 | meta);

            if (IsPosInChunk(pos))
            {
                ChunkBlocks[pos.x, pos.y, pos.z] = id;
                NeedsSave = true;
            }
        }

        public EnumBlock GetBlock(BlockPos pos)
        {
            if (pos.y >= 0 && pos.y < 256)
            {
                if (IsPosInChunk(pos))
                    return (EnumBlock)(ChunkBlocks[pos.x, pos.y, pos.z] >> 4);

                var block = World.GetBlock(pos + ChunkPos);

                return block;
            }

            return EnumBlock.AIR;
        }

        public int GetMetadata(BlockPos pos)
        {
            if (IsPosInChunk(pos))
                return ChunkBlocks[pos.x, pos.y, pos.z] & 15;

            return World.GetMetadata(pos + ChunkPos);
        }

        public void SetMetadata(BlockPos pos, int meta, bool redraw)
        {
            if (IsPosInChunk(pos))
            {
                var id = ChunkBlocks[pos.x, pos.y, pos.z];

                ChunkBlocks[pos.x, pos.y, pos.z] = (short)(id & 4095 | meta);
            }

            World.SetMetadata(pos + ChunkPos, meta, redraw);
        }

        private bool IsPosInChunk(BlockPos pos)
        {
            return
                pos.x >= 0 && pos.x < 16 &&
                pos.y >= 0 && pos.y < 256 &&
                pos.z >= 0 && pos.z < 16;
        }

        public void BuildChunkModel(ModelChunk previousChunkModel)
        {
            var modelRaw = new Dictionary<ShaderProgram, List<RawQuad>>();

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

                        var block = GetBlock(pos);

                        if (block == EnumBlock.AIR)
                            continue;

                        var blockModel = ModelRegistry.getModelForBlock(block, GetMetadata(pos));

                        if (!modelRaw.TryGetValue(blockModel.shader, out quads))
                            modelRaw.Add(blockModel.shader, quads = new List<RawQuad>());

                        for (int i = 0; i < FacingUtil.SIDES.Length; i++)
                        {
                            var dir = FacingUtil.SIDES[i];
                            var blockO = GetBlock(pos.offset(dir));

                            if (blockO == EnumBlock.AIR || blockO == EnumBlock.GLASS && block != EnumBlock.GLASS)
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
            //Console.WriteLine($"DEBUG: built chunk model [{sw.Elapsed.TotalMilliseconds:F}ms]");

            //var newShaders = MODEL_RAW.Keys.ToArray();

            foreach (var oldShader in previousChunkModel.fragmentPerShader.Keys)
            {
                if (!modelRaw.Keys.Contains(oldShader))
                {
                    Game.Instance.RunGlContext(() => previousChunkModel.destroyFragmentModelWithShader(oldShader));
                }
            }

            foreach (var value in modelRaw)
            {
                var newShader = value.Key;
                var newData = value.Value;

                if (!previousChunkModel.fragmentPerShader.Keys.Contains(newShader))
                {
                    Game.Instance.RunGlContext(() =>
                    {
                        var newFragment = new ModelChunkFragment(newShader, newData);
                        previousChunkModel.setFragmentModelWithShader(newShader, newFragment);
                    });
                }
                else
                {
                    Game.Instance.RunGlContext(() => previousChunkModel.getFragmentModelWithShader(newShader)?.overrideData(newData));
                }
            }

            IsDirty = false;
        }

        public void MarkDirty()
        {
            IsDirty = true;
        }

        public bool IsWithinRenderDistance()
        {
            return _withinRenderDistance;
        }
    }

}