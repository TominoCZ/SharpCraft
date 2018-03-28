using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using OpenTK;
using SharpCraft.block;
using SharpCraft.entity;
using SharpCraft.model;
using SharpCraft.util;
using SharpCraft.world.chunk;

namespace SharpCraft.world
{
	public class World
	{
		public ConcurrentDictionary<ChunkPos, Chunk> Chunks { get; }

		public List<Entity> Entities;

		public readonly int    Seed;
		public readonly string LevelName;

		private           NoiseUtil        _noiseUtil;
		private           int              _dimension = 0;
		internal readonly ChunkDataManager ChunkManager;
		public readonly   String           SaveRoot;

		public World(string saveName, string levelName, int seed)
		{
			Chunks = new ConcurrentDictionary<ChunkPos, Chunk>();
			Entities = new List<Entity>();

			_noiseUtil = new NoiseUtil(seed);
			_noiseUtil.SetFractalType(NoiseUtil.FractalType.FBM);

			Seed = seed;
			LevelName = levelName;
			SaveRoot = $"SharpCraft_Data/saves/{saveName}/";
			ChunkManager = new ChunkDataManager($"{SaveRoot}{_dimension}/chunks",
				new RegionInfo(new[] {12, 12}, 2 * Chunk.ChunkSize * Chunk.ChunkHeight * Chunk.ChunkSize));
		}

		public void AddEntity(Entity e)
		{
			if (!Entities.Contains(e))
				Entities.Add(e);
		}

		public void UpdateEntities()
		{
			for (var i = 0; i < Entities.Count; i++)
			{
				Entities[i].Update();
			}
		}

		public List<AxisAlignedBB> GetIntersectingEntitiesBBs(AxisAlignedBB with)
		{
			var bbs = new List<AxisAlignedBB>();

			for (var i = 0; i < Entities.Count; i++)
			{
				var bb = Entities[i].getEntityBoundingBox();

				if (bb.intersectsWith(with))
					bbs.Add(bb);
			}

			return bbs;
		}

		public List<AxisAlignedBB> GetBlockCollisionBoxes(AxisAlignedBB box)
		{
			var blocks = new List<AxisAlignedBB>();

			var bb = box.union(box);

			for (int x = (int) bb.min.X, maxX = (int) bb.max.X; x < maxX; x++)
			{
				for (int y = (int) bb.min.Y, maxY = (int) bb.max.Y; y < maxY; y++)
				{
					for (int z = (int) bb.min.Z, maxZ = (int) bb.max.Z; z < maxZ; z++)
					{
						var pos = new BlockPos(x, y, z);
						var block = Game.Instance.World.GetBlock(pos);
						if (block == EnumBlock.AIR)
							continue;

						blocks.Add(
							ModelRegistry.getModelForBlock(block, GetMetadata(pos)).boundingBox.offset(pos.ToVec()));
					}
				}
			}

			return blocks;
		}

		public Chunk GetChunk(ChunkPos pos)
		{
			return !Chunks.TryGetValue(pos, out var chunkData) ? null : chunkData;
		}

		public EnumBlock GetBlock(BlockPos pos)
		{
			var chunk = GetChunk(new ChunkPos(pos));
			if (chunk == null)
				return EnumBlock.AIR;

			return chunk.GetBlock(ChunkPos.ToChunkLocal(pos));
		}

		public void SetBlock(BlockPos pos, EnumBlock blockType, int meta)
		{
			var chunk = GetChunk(new ChunkPos(pos));
			if (chunk == null)
				return;

			chunk.SetBlock(ChunkPos.ToChunkLocal(pos), blockType, meta);
		}

		public void UnloadChunk(ChunkPos pos)
		{
			if (Chunks.TryRemove(pos, out var chunk)) // && data.model.isGenerated)
			{
				chunk.DestroyModel();
				chunk.Save();
			}
		}

		public bool LoadChunk(ChunkPos chunkPos)
		{
			var data = ChunkManager.GetChunkData(new[] {chunkPos.x / 16, chunkPos.z / 16});
			if (data == null) return false;


			var blockData = new short[16, 256, 16];
			Buffer.BlockCopy(data, 0, blockData, 0, data.Length);

			var chunk = new Chunk(chunkPos, this, blockData);
			if (Chunks.TryAdd(chunkPos, chunk)) throw new Exception("Chunk already exists at " + chunkPos);

			return true;
		}

		public void SaveAllChunks()
		{
			foreach (var data in Chunks.Values)
			{
				data.Save();
			}
		}

		public void DestroyChunkModels()
		{
			foreach (var data in Chunks.Values)
			{
				data.DestroyModel();
			}
		}

		public int GetMetadata(BlockPos pos)
		{
			return GetChunk(new ChunkPos(pos))?.GetMetadata(ChunkPos.ToChunkLocal(pos)) ?? -1;
		}

		public void SetMetadata(BlockPos pos, int meta)
		{
			GetChunk(new ChunkPos(pos))?.SetMetadata(ChunkPos.ToChunkLocal(pos), meta);
		}

		public int GetHeightAtPos(float x, float z)
		{
			//TODO this code only for 2D

			var pos = new BlockPos(x, 256, z);

			var chunk = GetChunk(new ChunkPos(pos.X, pos.Z));

			if (chunk == null)
				return 0; //ThreadPool.ScheduleTask(false, () => generateChunk(pos));

			var lastPos = pos;

			for (var y = Chunk.ChunkHeight - 1; y >= 0; y--)
			{
				var block = GetBlock(lastPos = lastPos.Offset(FaceSides.Down));

				if (block != EnumBlock.AIR)
					return y + 1;
			}

			return 0;
		}

		public IEnumerable<Chunk> GetNeighbourChunks(ChunkPos pos)
		{
			return FaceSides.YPlane.Select(dir => GetChunk(pos + dir));
		}

		public void GenerateChunk(ChunkPos chunkPos, bool updateContainingEntities)
		{
			if (Chunks.ContainsKey(chunkPos))
				return;

			var chunk = new Chunk(chunkPos, this);
			if (Chunks.TryAdd(chunkPos, chunk)) throw new Exception("Chunk already exists at " + chunkPos);

			ThreadPool.QueueUserWorkItem(e =>
			{
				short[,,] chunkData = new short[Chunk.ChunkSize, Chunk.ChunkHeight, Chunk.ChunkSize];

				void SetBlock(BlockPos p, EnumBlock i)
				{
					short id = (short) ((short) i << 4);
					chunkData[p.X, p.Y, p.Z] = id;
				}

				for (var z = 0; z < 16; z++)
				{
					for (var x = 0; x < 16; x++)
					{
						var xCh = (x + chunkPos.x) / 1.25f;
						var yCh = (z + chunkPos.z) / 1.25f;

						var peakY = 32 + (int) Math.Abs(
							            MathHelper.Clamp(0.35f + _noiseUtil.GetPerlinFractal(xCh, yCh), 0, 1) * 30);

						for (var y = peakY; y >= 0; y--)
						{
							var p = new BlockPos(x, y, z);

							if (y == peakY) SetBlock(p, EnumBlock.GRASS);
							else if (y > 0 && peakY - y > 0 && peakY - y < 3) SetBlock(p, EnumBlock.DIRT);
							else if (y == 0) SetBlock(p, EnumBlock.BEDROCK);
							else
							{
								var f = _noiseUtil.GetNoise(xCh * 32 - y * 16, yCh * 32 + x * 16);

								SetBlock(p, f >= 0.75f ? EnumBlock.RARE : EnumBlock.STONE);
							}
						}

						var treeSeed = Math.Abs(MathHelper.Clamp(_noiseUtil.GetWhiteNoise(xCh, yCh), 0, 1));
						var treeSeed2 = Math.Abs(MathHelper.Clamp(0.35f + _noiseUtil.GetPerlinFractal(yCh, xCh), 0, 1));

						if (treeSeed >= 0.995f && treeSeed2 >= 0.233f)
						{
							for (var treeY = 0; treeY < 5; treeY++)
							{
								SetBlock(new BlockPos(x, peakY + 1 + treeY, z), EnumBlock.LOG);
							}
						}
					}
				}

				chunk.GeneratedData(chunkData);

				if (updateContainingEntities)
				{
					foreach (var entity in Entities)
					{
						var pos2 = new BlockPos(entity.pos).ChunkPos();

						if (chunkPos.x == pos2.x && chunkPos.z == pos2.z)
						{
							var height = GetHeightAtPos(entity.pos.X, entity.pos.Z);

							if (entity.pos.Y < height)
								entity.teleportTo(new Vector3(entity.pos.X, entity.lastPos.Y = height, entity.pos.Z));
						}
					}
				}
			});
		}

		public bool AreNeighbourChunksGenerated(ChunkPos pos)
		{
			return GetNeighbourChunks(pos).All(chunk => chunk == null || !chunk.HasData);
		}
	}
}