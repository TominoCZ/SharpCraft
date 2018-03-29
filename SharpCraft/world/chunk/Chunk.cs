using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using OpenTK;
using OpenTK.Graphics.OpenGL;
using SharpCraft.block;
using SharpCraft.entity;
using SharpCraft.model;
using SharpCraft.shader;
using SharpCraft.util;

namespace SharpCraft.world.chunk
{
	public class Chunk
	{
		public const int ChunkSize   = 16;
		public const int ChunkHeight = 256;

		private short[,,] ChunkBlocks;

		private bool NeedsSave { get; set; }

		public ChunkPos Pos { get; }

		public AxisAlignedBB BoundingBox { get; }

		public World World { get; }

		private ModelChunk _model;

		private bool ModelGenerating;

		public bool HasData => ChunkBlocks != null;

		public Chunk(ChunkPos pos, World world)
		{
			Pos = pos;
			World = world;
			BoundingBox = new AxisAlignedBB(Vector3.Zero, Vector3.One * ChunkSize + Vector3.UnitY * 240).offset(Pos.ToVec());
		}

		public Chunk(ChunkPos pos, World world, short[,,] blockData)
		{
			Pos = pos;
			World = world;
			BoundingBox = new AxisAlignedBB(Vector3.Zero, Vector3.One * ChunkSize + Vector3.UnitY * 240).offset(Pos.ToVec());

			ChunkBlocks = blockData;
			BuildChunkModelNow();
		}

		public void Tick()
		{
			//update entitys here
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

		private bool EdgeCase(BlockPos localPos)
		{
			return localPos.X == 0 || localPos.Y == 0 || localPos.X == ChunkSize - 1 || localPos.Y == ChunkSize - 1;
		}

		public void SetBlock(BlockPos localPos, EnumBlock blockType, int meta)
		{
			CheckPos(localPos);
			short id = (short) ((short) blockType << 4 | meta);

			ChunkBlocks[localPos.X, localPos.Y, localPos.Z] = id;
			NeedsSave = true;
		}

		public EnumBlock GetBlock(BlockPos localPos)
		{
			if (localPos.Y <= 0 || localPos.Y >= ChunkHeight) return EnumBlock.AIR;
			CheckPosXZ(localPos);

			return (EnumBlock) (ChunkBlocks[localPos.X, localPos.Y, localPos.Z] >> 4);
		}

		public int GetMetadata(BlockPos localPos)
		{
			CheckPos(localPos);
			return ChunkBlocks[localPos.X, localPos.Y, localPos.Z] & 15;
		}

		public void SetMetadata(BlockPos localPos, int meta)
		{
			CheckPos(localPos);
			var id = (short) (ChunkBlocks[localPos.X, localPos.Y, localPos.Z] & 4095 | meta);

			if (id != ChunkBlocks[localPos.X, localPos.Y, localPos.Z])
			{
				ChunkBlocks[localPos.X, localPos.Y, localPos.Z] = id;
				MarkDirty();
				if (EdgeCase(localPos))
				{
				}
			}
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

				shader.loadVec3(Vector3.One, "lightColor");
				shader.loadViewMatrix(viewMatrix);

				shader.loadTransformationMatrix(MatrixHelper.createTransformationMatrix(Pos));

				chunkFragmentModel.rawModel.Render(shader.renderType);

				chunkFragmentModel.unbind();
			}
		}

		private void BuildChunkModel()
		{
			if (checkCanBuild())
			{
				ModelGenerating = true;
				ThreadPool.QueueUserWorkItem(e => BuildChunkModelUnsafe());
			}
		}

		private void BuildChunkModelNow()
		{
			if (checkCanBuild())
			{
				ModelGenerating = true;
				BuildChunkModelUnsafe();
			}
		}

		private bool checkCanBuild()
		{
			if (!HasData || ModelGenerating) return false;

			if (!World.AreNeighbourChunksGenerated(Pos)) return false;

			return true;
		}

		private void BuildChunkModelUnsafe()
		{

			var modelRaw = new Dictionary<ShaderProgram, List<RawQuad>>();

			List<RawQuad> quads;

			var sw = Stopwatch.StartNew();

			//generate the model / fill MODEL_RAW
			for (int z = 0; z < ChunkSize; z++)
			{
				for (int y = 0; y < 256; y++)
				{
					for (int x = 0; x < ChunkSize; x++)
					{
						var localPos = new BlockPos(x, y, z);
						var worldPos = new BlockPos(x + Pos.WorldSpaceX(), y, z + Pos.WorldSpaceZ());

						var block = World.GetBlock(worldPos);

						if (block == EnumBlock.AIR)
							continue;

						var blockModel = ModelRegistry.getModelForBlock(block, World.GetMetadata(worldPos));

						if (!modelRaw.TryGetValue(blockModel.shader, out quads))
							modelRaw.Add(blockModel.shader, quads = new List<RawQuad>());

						for (int i = 0; i < FaceSides.AllSides.Count; i++)
						{
							var dir = FaceSides.AllSides[i];
							var blockO = World.GetBlock(worldPos.Offset(dir));

							if (blockO == EnumBlock.AIR || blockO == EnumBlock.GLASS && block != EnumBlock.GLASS)
							{
								var quad = ((ModelBlockRaw) blockModel.rawModel)?.getQuadForSide(dir)?.offset(localPos);

								if (quad != null)
									quads.Add(quad);
							}
						}
					}
				}
			}

			sw.Stop();
			Console.WriteLine($"DEBUG: built chunk model [{sw.Elapsed.TotalMilliseconds:F}ms]");

			Game.Instance.RunGlContext(() =>
			{
				if (_model != null)
				{
					foreach (var oldShader in _model.fragmentPerShader.Keys)
					{
						if (!modelRaw.Keys.Contains(oldShader))
						{
							Game.Instance.RunGlContext(() => _model.destroyFragmentModelWithShader(oldShader));
						}
					}
				}
				else _model = new ModelChunk();

				foreach (var value in modelRaw)
				{
					var newShader = value.Key;
					var newData = value.Value;

					if (!_model.fragmentPerShader.Keys.Contains(newShader))
					{
						var newFragment = new ModelChunkFragment(newShader, newData);
						_model.setFragmentModelWithShader(newShader, newFragment);

					}
					else
					{
						_model.getFragmentModelWithShader(newShader)?.overrideData(newData);
					}
				}

			});
			ModelGenerating = false;
		}

		public void MarkDirty()
		{
			DestroyModel();
		}

		public void DestroyModel()
		{
			while (ModelGenerating) Thread.Sleep(2);
			if (_model == null) return;
			Game.Instance.RunGlContext(_model.destroy);
			_model = null;
		}

		public bool ShouldRender(int renderDistance)
		{
			return Pos.DistanceTo(Game.Instance.Camera.pos.Xz) < renderDistance * ChunkSize;
		}

		public void Save()
		{
			lock (this)
			{
				if (!NeedsSave) return;
				NeedsSave = false;

				//Console.WriteLine($"Saving chunk @ {chunk.Chunk.ChunkPos.x / 16} x {chunk.Chunk.ChunkPos.z / 16}");

				ChunkDataManager chunkManager = World.ChunkManager;
				var data = new byte[chunkManager.Info.ChunkByteSize];
				Buffer.BlockCopy(ChunkBlocks, 0, data, 0, data.Length);
				chunkManager.WriteChunkData(new[] {Pos.x, Pos.z}, data);
			}
		}

		public void GeneratedData(short[,,] chunkData)
		{
			ChunkBlocks = chunkData;
			NeedsSave = true;
			BuildChunkModelNow();
		}
	}
}