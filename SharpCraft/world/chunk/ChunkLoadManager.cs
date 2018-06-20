using SharpCraft.entity;
using System.Collections.Concurrent;
using System.Threading;

namespace SharpCraft.world.chunk
{
    public class ChunkLoadManager
    {
        private readonly ConcurrentQueue<Chunk> _importantChunkBuilds = new ConcurrentQueue<Chunk>();
        private readonly object _buildLock = new object();

        private readonly ConcurrentQueue<ChunkPos> _importantChunkLoads = new ConcurrentQueue<ChunkPos>();
        private readonly object _loadLock = new object();

        private readonly SwapList<Chunk> _chunkBuilds = new SwapList<Chunk>(
            c => c.BuildChunkModelNow(),
            (l, r) => l.Pos.DistanceTo(SharpCraft.Instance.Player.Pos).CompareTo(r.Pos.DistanceTo(SharpCraft.Instance.Player.Pos)));

        private readonly SwapList<ChunkPos> _chunkLoads = new SwapList<ChunkPos>(
            c =>
            {
                World w = SharpCraft.Instance.World;
                if (w.GetChunk(c) != null) return;

#pragma warning disable CS0642 // Possible mistaken empty statement
                if (w.LoadChunk(c)) ;//Console.WriteLine($"chunk loaded    @ {c.x} x {c.z}");
#pragma warning restore CS0642 // Possible mistaken empty statement
                else
                {
                    w.GenerateChunk(c, true);
                    //Console.WriteLine($"chunk generated @ {c.x} x {c.z}");
                }
            },
            (l, r) => l.DistanceTo(SharpCraft.Instance.Player.Pos).CompareTo(r.DistanceTo(SharpCraft.Instance.Player.Pos)));

        public void BuildChunks()
        {
            lock (_chunkBuilds)
            {
                if (_chunkBuilds.Building) return;
                _chunkBuilds.Building = true;
            }

            ThreadPool.QueueUserWorkItem(e =>
            {
                lock (_buildLock)
                {
                    _chunkBuilds.Exec(false);
                }
            });
        }

        public void BuildImportantChunks()
        {
            while (!_importantChunkBuilds.IsEmpty)
            {
                if (_importantChunkBuilds.TryDequeue(out Chunk chunk)) chunk.BuildChunkModelNow();
            }
            SharpCraft.Instance.RunGlTasks();
        }

        public void NotifyImportantBuild(Chunk chunk)
        {
            chunk.QueuedForModelBuild = true;
            _importantChunkBuilds.Enqueue(chunk);
        }

        public void NotifyBuild(Chunk chunk)
        {
            lock (_chunkBuilds)
            {
                _chunkBuilds.Add(chunk);
            }
        }

        public void LoadChunks()
        {
            lock (_chunkLoads)
            {
                if (_chunkLoads.Building) return;
                _chunkLoads.Building = true;
            }

            ThreadPool.QueueUserWorkItem(e =>
            {
                lock (_loadLock)
                {
                    _chunkLoads.Exec(true);
                }
            });
        }

        public void LoadImportantChunks()
        {
            while (!_importantChunkLoads.IsEmpty)
            {
                if (_importantChunkLoads.TryDequeue(out ChunkPos pos) && SharpCraft.Instance.World.GetChunk(pos) == null)
                {
                    lock (_chunkLoads)
                    {
                        _chunkLoads.Remove(pos);
                    }
                    SharpCraft.Instance.World.LoadChunk(pos);
                }
            }
        }

        public void NotifyImportantLoad(ChunkPos chunk)
        {
            _importantChunkLoads.Enqueue(chunk);
        }

        public void NotifyLoad(ChunkPos chunk)
        {
            lock (_chunkLoads)
            {
                _chunkLoads.Add(chunk);
            }
        }

        public void UpdateLoad(EntityPlayerSP player, int renderDistance, bool important)
        {
            World world = SharpCraft.Instance.World;

            ChunkPos playerChunkPos = ChunkPos.FromWorldSpace(SharpCraft.Instance.Player.Pos);

            for (int z = -renderDistance; z <= renderDistance; z++)
            {
                for (int x = -renderDistance; x <= renderDistance; x++)
                {
                    ChunkPos pos = playerChunkPos + new ChunkPos(x, z);
                    if (pos.DistanceTo(player.Pos.Xz) < renderDistance * Chunk.ChunkSize)
                    {
                        if (world.GetChunk(pos) == null)
                        {
                            if (important) NotifyImportantLoad(pos);
                            else NotifyLoad(pos);
                        }
                    }
                }
            }

            LoadChunks();
            BuildChunks();
        }
    }
}