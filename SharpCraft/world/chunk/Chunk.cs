using OpenTK;
using OpenTK.Graphics.OpenGL;
using SharpCraft.block;
using SharpCraft.entity;
using SharpCraft.model;
using SharpCraft.util;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
#pragma warning disable 618

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

        private readonly ConcurrentDictionary<BlockPos, TileEntity> _tileEntities = new ConcurrentDictionary<BlockPos, TileEntity>();

        private readonly ChunkLoadManager _loadManager;

        private ModelChunk _model;

        public bool ModelBuilding;
        public bool QueuedForModelBuild;

        public bool HasData => _chunkBlocks != null;

        public Chunk(ChunkPos pos, World world)
        {
            Pos = pos;
            World = world;
            _loadManager = World.LoadManager;
            BoundingBox = new AxisAlignedBb(Vector3.Zero, Vector3.One * ChunkSize + Vector3.UnitY * 240).Offset(Pos.ToVec());

            //Load();
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
            foreach (var tileEntity in _tileEntities.Values)
            {
                tileEntity.Update();
            }
        }

        private void CheckPos(BlockPos localPos)
        {
            if (localPos.Y < 0) throw new IndexOutOfRangeException($"Block Pos y({localPos.Y}) is less than 0");
            if (localPos.Y >= ChunkHeight) throw new IndexOutOfRangeException($"Block Pos y({localPos.Y}) is bigger or equal to ChunkHeight");
            CheckPosXZ(localPos);
        }

        private void CheckPosXZ(BlockPos localPos)
        {
            if (localPos.X < 0) throw new IndexOutOfRangeException($"Block Pos x({localPos.X}) is less than 0");
            if (localPos.Z < 0) throw new IndexOutOfRangeException($"Block Pos z({localPos.Z}) is less than 0");
            if (localPos.X >= ChunkSize) throw new IndexOutOfRangeException($"Block Pos x({localPos.X}) is bigger or equal to ChunkSize");
            if (localPos.Z >= ChunkSize) throw new IndexOutOfRangeException($"Block Pos z({localPos.Z}) is bigger or equal to ChunkSize");
        }

        public void SetBlockState(BlockPos localPos, BlockState state)
        {
            CheckPos(localPos);

            short id = World.GetLocalBlockId(state.Block.UnlocalizedName);
            short meta = state.Block.GetMetaFromState(state);

#pragma warning disable CS0675 // Bitwise-or operator used on a sign-extended operand
            short value = (short)(id << 4 | meta);
#pragma warning restore CS0675 // Bitwise-or operator used on a sign-extended operand

            if (_chunkBlocks[localPos.X, localPos.Y, localPos.Z] != value)
            {
                _chunkBlocks[localPos.X, localPos.Y, localPos.Z] = value;

                if (ModelBuilding || !QueuedForModelBuild) //this is so that we prevent double chunk build calls and invisible placed blocks(if the model is already generating, there is a chance that the block on this position was already processed, so the rebuild is queued again)
                {
                    //BuildChunkModel(); TODO - make this run on another thread
                    NotifyModelChange(localPos);
                }

                NeedsSave = true;
            }
        }

        public BlockState GetBlockState(BlockPos localPos)
        {
            if (localPos.Y < 0 || localPos.Y >= ChunkHeight)
                return BlockRegistry.GetBlock<BlockAir>().GetState();

            CheckPosXZ(localPos);

            short value = _chunkBlocks[localPos.X, localPos.Y, localPos.Z];
            short id = (short)(value >> 4);
            short meta = (short)(value & 15);

            string blockName = World.GetLocalBlockName(id);

            return BlockRegistry.GetBlock(blockName).GetState(meta);
        }

        public void AddTileEntity(BlockPos localPos, TileEntity te)
        {
            if (localPos.Y < 0 || localPos.Y >= ChunkHeight)
                return;

            if (_tileEntities.TryAdd(localPos, te))
            {
                var worldPos = new BlockPos(localPos.ToVec() + Pos.ToVec());

                var file = $"{World.SaveRoot}\\{World.Dimension}\\te\\te_{worldPos.X}.{worldPos.Y}.{worldPos.Z}.te";

                if (!File.Exists(file))
                    return;

                using (ByteBufferReader bbr = new ByteBufferReader(File.ReadAllBytes(file)))
                {
                    te.ReadData(bbr);
                }
            }
        }

        public void RemoveTileEntity(BlockPos localPos)
        {
            if (localPos.Y < 0 || localPos.Y >= ChunkHeight)
                return;

            if (!_tileEntities.TryRemove(localPos, out var te))
                return;

            var worldPos = new BlockPos(Pos.ToVec() + localPos.ToVec());

            var file = $"{World.SaveRoot}\\{World.Dimension}\\te\\te_{Pos.WorldSpaceX() + localPos.X}.{localPos.Y}.{Pos.WorldSpaceZ() + localPos.Z}.te";

            if (File.Exists(file))
                File.Delete(file);

            te.OnDestroyed(World, worldPos);
        }

        public TileEntity GetTileEntity(BlockPos localPos)
        {
            if (localPos.Y < 0 || localPos.Y >= ChunkHeight)
                return null;

            _tileEntities.TryGetValue(localPos, out var te);

            return te;
        }

        public void SaveTileEntity(BlockPos localPos)
        {
            if (localPos.Y < 0 || localPos.Y >= ChunkHeight)
                return;

            SaveTileEntityData(localPos);
        }

        private void SaveTileEntityData(BlockPos localPos)
        {
            if (!_tileEntities.TryGetValue(localPos, out var te))
                return;

            var dir = $"{World.SaveRoot}\\{World.Dimension}\\te";

            if (!Directory.Exists(dir))
                Directory.CreateDirectory(dir);

            var file =
                $"{dir}\\te_{Pos.WorldSpaceX() + localPos.X}.{localPos.Y}.{Pos.WorldSpaceZ() + localPos.Z}.te";

            ByteBufferWriter bbw = new ByteBufferWriter(0);

            te.WriteData(bbw);

            File.WriteAllBytes(file, bbw.ToArray());
        }

        public bool IsAir(BlockPos pos)
        {
            return GetBlockState(pos).Block == BlockRegistry.GetBlock<BlockAir>();
        }

        private void NotifyModelChange(BlockPos localPos)
        {
            MarkDirty();
            if (localPos.X == 0)
                World.GetChunk(Pos + FaceSides.West).MarkDirty();
            if (localPos.X == ChunkSize - 1)
                World.GetChunk(Pos + FaceSides.East).MarkDirty();
            if (localPos.Z == 0)
                World.GetChunk(Pos + FaceSides.North).MarkDirty();
            if (localPos.Z == ChunkSize - 1)
                World.GetChunk(Pos + FaceSides.South).MarkDirty();
        }

        public int GetHeightAtPos(int x, int z)
        {
            BlockPos pos = new BlockPos(x, 256, z);

            for (int y = ChunkHeight - 1; y >= 0; y--)
            {
                if (!IsAir(pos = pos.Offset(FaceSides.Down)))
                    return y + 1;
            }

            return 0;
        }

        public void Render(float partialTicks)
        {
            if (_model == null)
            {
                BuildChunkModel();
                return;
            }

            //foreach (Shader<ModelBlock> shader in _model.fragmentPerShader.Keys)
            //{
            //ModelChunkFragment chunkFragmentModel = _model.getFragmentModelWithShader(shader);

            var shader = Block.DefaultShader;

            _model.Bind();
            shader.UpdateGlobalUniforms();
            shader.UpdateModelUniforms();
            shader.UpdateInstanceUniforms(MatrixHelper.CreateTransformationMatrix(Pos), null);

            _model.RawModel.Render(PrimitiveType.Quads);

            _model.Unbind();

            foreach (var tileEntity in _tileEntities)
            {
                tileEntity.Value.Render(partialTicks);
            }

            GL.BindTexture(TextureTarget.Texture2D, JsonModelLoader.TEXTURE_BLOCKS);
            //}
        }

        public void BuildChunkModel()
        {
            if (!CheckCanBuild())
                return;

            QueuedForModelBuild = true;
            _loadManager.NotifyBuild(this);
        }

        private bool CheckCanBuild()
        {
            return HasData && !QueuedForModelBuild && !ModelBuilding && World.AreNeighbourChunksGenerated(Pos);
        }

        public void BuildChunkModelNow()
        {
            if (ModelBuilding || !QueuedForModelBuild)
                return;

            ModelBuilding = true;

            //ConcurrentDictionary<Shader<ModelBlock>, List<RawQuad>> modelRaw = new ConcurrentDictionary<Shader<ModelBlock>, List<RawQuad>>();

            //List<RawQuad> quads;

            Stopwatch sw = Stopwatch.StartNew(); //this is just a debug thing....

            var air = BlockRegistry.GetBlock<BlockAir>();

            var vertexes = new List<float>();
            var normals = new List<float>();
            var uvs = new List<float>();

            object locker = new object();

            //generate the model - fill MODEL_RAW
            Enumerable.Range(0, ChunkHeight).AsParallel().ForAll(y =>
            {
                for (int x = 0; x < ChunkSize; x++)
                {
                    for (int z = 0; z < ChunkSize; z++)
                    {
                        BlockPos worldPos = new BlockPos(x + Pos.WorldSpaceX(), y, z + Pos.WorldSpaceZ());

                        BlockState state = World.GetBlockState(worldPos);
                        if (state.Block == air)
                            continue;

                        BlockPos localPos = new BlockPos(x, y, z);

                        ModelBlock model = JsonModelLoader.GetModelForBlock(state.Block.UnlocalizedName);
                        ModelBlockRaw mbr = (ModelBlockRaw)model?.RawModel;

                        if (mbr == null)
                            continue;

                        if (!state.Block.IsFullCube)
                        {
                            lock (locker)
                                mbr.AppendAllVertexData(vertexes, normals, uvs, localPos);

                            continue;
                        }

                        for (var index = 0; index < FaceSides.AllSides.Count; index++)
                        {
                            FaceSides dir = FaceSides.AllSides[index];

                            BlockPos worldPosO = worldPos.Offset(dir);
                            BlockState stateO = World.GetBlockState(worldPosO);

                            if (!(stateO.Block == air ||
                                  stateO.Block.HasTransparency && !state.Block.HasTransparency) &&
                                stateO.Block.IsFullCube)
                                continue;

                            lock (locker)
                            {
                                mbr.AppendVertexDataForSide(dir, vertexes, normals, uvs, localPos);
                                //mbr.AppendNormalsForSide(dir, normals);
                                //mbr.AppendUvsForSide(dir, uvs);
                            }
                        }
                    }
                }
            });

            sw.Stop();
            Console.WriteLine($"DEBUG: built chunk model [{sw.Elapsed.TotalMilliseconds:F}ms]");

            float[] vtx = vertexes.ToArray(); //this is here because this takes time and I don't want it to slow down the main thread by running it in GlContext
            float[] nrm = normals.ToArray();
            float[] uv = uvs.ToArray();

            SharpCraft.Instance.RunGlContext(() =>
            {
                if (_model == null)
                    _model = new ModelChunk(vtx, nrm, uv, Block.DefaultShader);
                else
                    _model.OverrideData(vtx, nrm, uv);

                ModelBuilding = false;
            });

            QueuedForModelBuild = false;
        }

        public void MarkDirty()
        {
            _loadManager.NotifyImportantBuild(this);
        }

        public void DestroyModel()
        {
            if (_model == null) return;
            SharpCraft.Instance.RunGlContext(_model.Destroy);
            _model = null;
        }

        public bool ShouldRender(int renderDistance)
        {
            return Pos.DistanceTo(SharpCraft.Instance.Camera.Pos.Xz) < renderDistance * ChunkSize;
        }

        public void Save()
        {
            if (!NeedsSave) return;
            NeedsSave = false;

            Console.WriteLine($"Saving chunk @ {Pos.x} x {Pos.z}");

            //TODO - svae tile entities

            foreach (var pair in _tileEntities.Keys)
            {
                SaveTileEntityData(pair);
            }

            byte[] data = new byte[World.ChunkData.Info.ChunkByteSize];

            Buffer.BlockCopy(_chunkBlocks, 0, data, 0, data.Length);
            World.ChunkData.WriteChunkData(Pos, data);
        }

        public void GeneratedData(short[,,] chunkData)
        {
            _chunkBlocks = chunkData;
            NeedsSave = true;
            BuildChunkModel();
        }
    }
}