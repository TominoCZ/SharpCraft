using OpenTK;
using SharpCraft.block;
using SharpCraft.entity;
using SharpCraft.model;
using SharpCraft.util;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using OpenTK.Graphics.OpenGL;
using SharpCraft.render.shader;

namespace SharpCraft.world.chunk
{
	public class Chunk
    {
        public const int ChunkSize = 16;
        public const int ChunkHeight = 256;

        private short[,,] _chunkBlocks;

        private bool NeedsSave { get; set; }

        public ChunkPos Pos { get; }

        public AxisAlignedBb BoundingBox { get; }

        public World World { get; }
        private readonly ChunkLoadManager _loadManager;

        private ModelChunk _model;

        public bool ModelGenerating;

        public bool HasData => _chunkBlocks != null;

        public Chunk(ChunkPos pos, World world)
        {
            Pos = pos;
            World = world;
            _loadManager = World.LoadManager;
            BoundingBox = new AxisAlignedBb(Vector3.Zero, Vector3.One * ChunkSize + Vector3.UnitY * 240).offset(Pos.ToVec());
        }

        public Chunk(ChunkPos pos, World world, short[,,] blockData) : this(pos, world)
        {
            _chunkBlocks = blockData;
            BuildChunkModel();
            NeedsSave = false;
        }

        public void Update()
        {
            //update entities here
        }

        private void CheckPos(BlockPos localPos)
        {
            if (localPos.Y < 0) throw new IndexOutOfRangeException($"Block pos y({localPos.Y}) is less than 0");
            if (localPos.Y >= ChunkHeight) throw new IndexOutOfRangeException($"Block pos y({localPos.Y}) is bigger or equal to ChunkHeight");
            CheckPosXZ(localPos);
        }

        private void CheckPosXZ(BlockPos localPos)
        {
            if (localPos.X < 0) throw new IndexOutOfRangeException($"Block pos x({localPos.X}) is less than 0");
            if (localPos.Z < 0) throw new IndexOutOfRangeException($"Block pos z({localPos.Z}) is less than 0");
            if (localPos.X >= ChunkSize) throw new IndexOutOfRangeException($"Block pos x({localPos.X}) is bigger or equal to ChunkSize");
            if (localPos.Z >= ChunkSize) throw new IndexOutOfRangeException($"Block pos z({localPos.Z}) is bigger or equal to ChunkSize");
        }

        public void SetBlock(BlockPos localPos, EnumBlock blockType, int meta)
        {
            CheckPos(localPos);
            short id = (short)((short)blockType << 4 | meta);

            if (_chunkBlocks[localPos.X, localPos.Y, localPos.Z] != id)
            {
                _chunkBlocks[localPos.X, localPos.Y, localPos.Z] = id;
                NotifyModelChange(localPos);
            }

            NeedsSave = true;
        }

        public EnumBlock GetBlock(BlockPos localPos)
        {
            if (localPos.Y <= 0 || localPos.Y >= ChunkHeight) return EnumBlock.AIR;
            CheckPosXZ(localPos);

            return (EnumBlock)(_chunkBlocks[localPos.X, localPos.Y, localPos.Z] >> 4);
        }

        public int GetMetadata(BlockPos localPos)
        {
            CheckPos(localPos);
            return _chunkBlocks[localPos.X, localPos.Y, localPos.Z] & 15;
        }

        public void SetMetadata(BlockPos localPos, int meta)
        {
            CheckPos(localPos);
            var id = (short)(_chunkBlocks[localPos.X, localPos.Y, localPos.Z] & 4095 | meta);

            if (id != _chunkBlocks[localPos.X, localPos.Y, localPos.Z])
            {
                _chunkBlocks[localPos.X, localPos.Y, localPos.Z] = id;
                NotifyModelChange(localPos);
            }
        }

        private void NotifyModelChange(BlockPos localPos)
        {
            MarkDirty();
            if (localPos.X == 0) World.GetChunk(Pos + FaceSides.West).MarkDirty();
            if (localPos.X == ChunkSize - 1) World.GetChunk(Pos + FaceSides.East).MarkDirty();
            if (localPos.Z == 0) World.GetChunk(Pos + FaceSides.North).MarkDirty();
            if (localPos.Z == ChunkSize - 1) World.GetChunk(Pos + FaceSides.South).MarkDirty();
        }

        public int GetHeightAtPos(int x, int z)
        {
            var pos = new BlockPos(x, 256, z);

            for (var y = ChunkHeight - 1; y >= 0; y--)
            {
                var block = GetBlock(pos = pos.Offset(FaceSides.Down));

                if (block != EnumBlock.AIR)
                    return y + 1;
            }

            return 0;
        }

        public void Render(Matrix4 viewMatrix)
        {
            if (_model == null)
            {
                BuildChunkModel();
                return;
            }

            foreach (var shader in _model.fragmentPerShader.Keys)
            {
                var chunkFragmentModel = _model.getFragmentModelWithShader(shader);
                if (chunkFragmentModel == null) continue;

                chunkFragmentModel.bind();
	            shader.UpdateGlobalUniforms();
	            shader.UpdateModelUniforms(null);
	            shader.UpdateInstanceUniforms(MatrixHelper.createTransformationMatrix(Pos),null);

                chunkFragmentModel.rawModel.Render(PrimitiveType.Quads);

                chunkFragmentModel.unbind();
            }
        }

        private void BuildChunkModel()
        {
            if (checkCanBuild())
            {
                ModelGenerating = true;
                _loadManager.NotifyBuild(this);
            }
        }

        private bool checkCanBuild()
        {
            if (!HasData || ModelGenerating) return false;

            if (!World.AreNeighbourChunksGenerated(Pos)) return false;

            return true;
        }

        internal void BuildChunkModelDo()
        {
            if (!ModelGenerating && _model != null) return;

            var modelRaw = new Dictionary<Shader<ModelBlock>, List<RawQuad>>();

            List<RawQuad> quads;

            var sw = Stopwatch.StartNew();
            //generate the model / fill MODEL_RAW

            Enumerable.Range(0, ChunkHeight).AsParallel().ForAll(y =>
            {
                for (int x = 0; x < ChunkSize; x++)
                {
                    for (int z = 0; z < ChunkSize; z++)
                    {
                        var localPos = new BlockPos(x, y, z);
                        var worldPos = new BlockPos(x + Pos.WorldSpaceX(), y, z + Pos.WorldSpaceZ());

                        var block = World.GetBlock(worldPos);

                        if (block == EnumBlock.AIR)
                            continue;

                        var blockModel = ModelRegistry.getModelForBlock(block, World.GetMetadata(worldPos));

                        lock (modelRaw)
                        {
                            if (!modelRaw.TryGetValue(blockModel.shader, out quads))
                                modelRaw.Add(blockModel.shader, quads = new List<RawQuad>());
                        }

                        foreach (var dir in FaceSides.AllSides)
                        {
                            var worldPosO = worldPos.Offset(dir);
                            var blockO = World.GetBlock(worldPosO);
                            var blockModelO = ModelRegistry.getModelForBlock(blockO, World.GetMetadata(worldPosO));

                            if (blockO == EnumBlock.AIR || blockModelO.hasTransparency && !blockModel.hasTransparency)
                            {
                                var quad = ((ModelBlockRaw)blockModel.rawModel)?.getQuadForSide(dir)?.offset(localPos);

                                if (quad != null)
                                {
                                    lock (quads)
                                    {
                                        quads.Add(quad);
                                    }
                                }
                            }
                        }
                    }
                }
            });

            sw.Stop();
            Console.WriteLine($"DEBUG: built chunk model [{sw.Elapsed.TotalMilliseconds:F}ms]");

            SharpCraft.Instance.RunGlContext(() =>
            {
                if (_model != null)
                {
                    foreach (var oldShader in _model.fragmentPerShader.Keys)
                    {
                        if (!modelRaw.Keys.Contains<object>(oldShader))
                        {
                            _model.destroyFragmentModelWithShader(oldShader);
                        }
                    }
                }
                else _model = new ModelChunk();

                foreach (var value in modelRaw)
                {
                    var newShader = value.Key;
                    var newData = value.Value;

                    if (!_model.fragmentPerShader.Keys.Contains<object>(newShader))
                    {
                        var newFragment = new ModelChunkFragment(newShader, newData);
                        _model.setFragmentModelWithShader(newShader, newFragment);
                    }
                    else
                    {
                        _model.getFragmentModelWithShader(newShader)?.overrideData(newData);
                    }
                }

                ModelGenerating = false;
            });
        }

        public void MarkDirty()
        {
            DestroyModel();
            _loadManager.NotifyImportantBuild(this);
        }

        public void DestroyModel()
        {
            while (ModelGenerating) Thread.Sleep(2);
            if (_model == null) return;
            SharpCraft.Instance.RunGlContext(_model.destroy);
            _model = null;
        }

        public bool ShouldRender(int renderDistance)
        {
            return Pos.DistanceTo(SharpCraft.Instance.Camera.pos.Xz) < renderDistance * ChunkSize;
        }

        public void Save()
        {
            lock (this)
            {
                if (!NeedsSave) return;
                NeedsSave = false;

                Console.WriteLine($"Saving chunk @ {Pos.x} x {Pos.z}");

                var data = new byte[World.ChunkData.Info.ChunkByteSize];
                Buffer.BlockCopy(_chunkBlocks, 0, data, 0, data.Length);
                World.ChunkData.WriteChunkData(Pos, data);
            }
        }

        public void GeneratedData(short[,,] chunkData)
        {
            _chunkBlocks = chunkData;
            NeedsSave = true;
            BuildChunkModel();
        }
    }
}