using SharpCraft.entity;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace SharpCraft.world.chunk
{
    internal class ChunkLoadManager
    {
        private readonly ConcurrentQueue<Chunk> _importantChunkBuilds = new ConcurrentQueue<Chunk>();
        private readonly object _buildLock = new object();

        private readonly ConcurrentQueue<ChunkPos> _importantChunkLoads = new ConcurrentQueue<ChunkPos>();
        private readonly object _loadLock = new object();

        private SwapList<Chunk> _chunkBuilds = new SwapList<Chunk>(
            c => c.BuildChunkModelDo(),
            (l, r) => l.Pos.DistanceTo(SharpCraft.Instance.Player.pos).CompareTo(l.Pos.DistanceTo(SharpCraft.Instance.Player.pos)));

        private SwapList<ChunkPos> _chunkLoads = new SwapList<ChunkPos>(
            c =>
            {
                var w = SharpCraft.Instance.World;
                if (w.GetChunk(c) != null) return;

                if (w.LoadChunk(c)) ;//Console.WriteLine($"chunk loaded    @ {c.x} x {c.z}");
                else
                {
                    w.GenerateChunk(c, true);
                    //Console.WriteLine($"chunk generated @ {c.x} x {c.z}");
                }
            },
            (l, r) => l.DistanceTo(SharpCraft.Instance.Player.pos).CompareTo(l.DistanceTo(SharpCraft.Instance.Player.pos)));

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
                if (_importantChunkBuilds.TryDequeue(out var chunk)) chunk.BuildChunkModelDo();
            }
            SharpCraft.Instance.RunGlTasks();
        }

        public void NotifyImportantBuild(Chunk chunk)
        {
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
                if (_importantChunkLoads.TryDequeue(out var pos) && SharpCraft.Instance.World.GetChunk(pos) == null)
                {
                    lock (_chunkLoads)
                    {
                        _chunkLoads.remove(pos);
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
            var world = SharpCraft.Instance.World;

            var playerChunkPos = ChunkPos.FromWorldSpace(SharpCraft.Instance.Player.pos);

            for (var z = -renderDistance; z <= renderDistance; z++)
            {
                for (var x = -renderDistance; x <= renderDistance; x++)
                {
                    var pos = playerChunkPos + new ChunkPos(x, z);
                    if (pos.DistanceTo(player.pos.Xz) < renderDistance * Chunk.ChunkSize)
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

    internal class SwapList<T>
    {
        private List<T> _primary = new List<T>();
        private List<T> _backup = new List<T>();
        internal bool Building;
        private readonly Action<T> _action;
        private readonly Comparison<T> _comparison;

        internal SwapList(Action<T> action, Comparison<T> comparison)
        {
            _action = action;
            _comparison = comparison;
        }

        internal void Exec(bool parallel)
        {
            _primary.Sort(_comparison);
            if (parallel) Parallel.ForEach(_primary, _action);
            else _primary.ForEach(_action);
            _primary.Clear();

            var ch = _primary;
            _primary = _backup;
            _backup = ch;

            if (_primary.Count > 0) Exec(parallel);

            Building = false;
        }

        internal void Add(T t) => (Building ? _backup : _primary).Add(t);

        public void remove(T pos)
        {
            _primary.Remove(pos);
            _backup.Remove(pos);
        }
    }
}