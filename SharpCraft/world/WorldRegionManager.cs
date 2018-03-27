using System;
using System.IO;
using System.Runtime.InteropServices;
using System.Runtime.Serialization.Formatters.Binary;
using System.Threading;

namespace SharpCraft
{
    internal static class WorldRegionManager
    {
        private static string dir = "SharpCraft_Data/saves/world/regions";

        private static ReaderWriterLock locker = new ReaderWriterLock();

        private static long _regionInfoSize;
        private static long _chunkSize;

        static WorldRegionManager()
        {
            var keySize = Marshal.SizeOf(typeof(BlockPos));
            var valueSize = Marshal.SizeOf(typeof(long));

            _regionInfoSize = 16 * (keySize + valueSize) + Marshal.SizeOf(typeof(RegionInfo));
            _chunkSize = sizeof(short) * 256 * 256 + Marshal.SizeOf(typeof(BlockPos));
        }

        public static void saveChunk(Chunk c)
        {
            var regionPos = new BlockPos(c.chunkPos.x / (4 * 16), 0, c.chunkPos.z / (4 * 16));
            var fileName = $"r.{regionPos.x}.{regionPos.z}.region";

            if (!Directory.Exists(dir))
                Directory.CreateDirectory(dir);

            try
            {
                locker.AcquireWriterLock(1000);
                locker.AcquireReaderLock(1000);

                /*
                var bf = new BinaryFormatter();

                if (!File.Exists($"{dir}/{fileName}"))
                {
                    using (var fs = File.Open(, FileMode.OpenOrCreate, FileAccess.ReadWrite,
                        FileShare.ReadWrite))
                    {
                        var ri = new RegionInfo(16);
                        bf.Serialize(fs, ri);
                        fs.Position = _regionInfoSize + 1;

                        for (int i = 0; i < 16; i++)
                        {
                            var cc = new ChunkCache(c.chunkPos, new short[16, 256, 16]);

                            bf.Serialize(fs, cc);

                            if (i < 15)
                                fs.Position += _chunkSize + 1;
                        }
                    }
                }

                //read region info
                using (var fs = File.Open($"{dir}/{fileName}", FileMode.Open, FileAccess.Read, FileShare.Read))
                {
                    fs.Seek(0, SeekOrigin.Begin); //.SetLength(_regionInfoSize);

                    var regionInfo = (RegionInfo) bf.Deserialize(fs);
                    var chunkFilePos = regionInfo.getChunkPos(c.chunkPos);

                    fs.Position = chunkFilePos;
                    fs.SetLength(_chunkSize);

                    var cache = c.createChunkCache();

                    bf.Serialize(fs, new ChunkCache(c.chunkPos, cache.chunkBlocks));
                }*/
            }
            catch
            {
            }
            finally
            {
                locker.ReleaseLock();
            }
        }

        public static Chunk loadChunk(BlockPos pos)
        {
            var chunkPos = pos.chunkPos();

            var regionPos = new BlockPos(chunkPos.x / (4 * 16), 0, chunkPos.z / (4 * 16));
            var fileName = $"r.{regionPos.x}.{regionPos.z}.region";

            var bf = new BinaryFormatter();

            try
            {
                locker.AcquireReaderLock(1000);

                if (!Directory.Exists(dir) || !File.Exists($"{dir}/{fileName}"))
                    return null;

                //read, if exists
                using (var fs = File.Open($"{dir}/{fileName}", FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
                {
                    fs.SetLength(_regionInfoSize);

                    var regionInfo = (RegionInfo)bf.Deserialize(fs);
                    var chunkFilePos = regionInfo.getChunkPos(chunkPos);

                    fs.Position = chunkFilePos;
                    fs.SetLength(_chunkSize);

                    var cc = (ChunkCache)bf.Deserialize(fs);

                    return Chunk.CreateFromCache(chunkPos, cc);
                }
            }
            catch
            {
                return null;
            }
            finally
            {
                locker.ReleaseLock();
            }
        }
    }

    [Serializable]
    internal struct RegionInfo
    {
        private MyDictionary<BlockPos, long> _pointers;

        public RegionInfo(int i)
        {
            _pointers = new MyDictionary<BlockPos, long>(i);
        }

        public void putChunk(Chunk c)
        {
            _pointers.Set(c.chunkPos, (sizeof(short) * 256 * 256 + Marshal.SizeOf(typeof(BlockPos))) * _pointers.Count);
        }

        public long getChunkPos(BlockPos pos)
        {
            if (!_pointers.TryGetValue(pos, out var l))
                return -1;

            return l;
        }
    }

    [Serializable]
    internal struct MyDictionary<TKey, TValue>
    {
        private ValueNode<TKey, TValue>[] _values;

        public int Count => _values.Length;

        private int _length;

        public MyDictionary(int length)
        {
            _values = new ValueNode<TKey, TValue>[_length = length];
        }

        public void Set(TKey key, TValue value)
        {
            Remove(key);

            if (_values.Length < _length)
                _values[_values.Length] = new ValueNode<TKey, TValue>(key, value);
        }

        public void Remove(TKey key)
        {
            for (int i = 0; i < _values.Length; i++)
            {
                var node = _values[i];

                if (node.key.Equals(key))
                    _values[i] = new ValueNode<TKey, TValue>();
            }
        }

        public bool TryGetValue(TKey key, out TValue value)
        {
            for (int i = 0; i < _values.Length; i++)
            {
                var node = _values[i];

                if (node.key.Equals(key))
                {
                    value = node.value;
                    return true;
                }
            }

            value = default(TValue);

            return false;
        }
    }

    [Serializable]
    internal struct ValueNode<TKey, TValue>
    {
        public TKey key { get; }
        public TValue value { get; }

        public ValueNode(TKey key, TValue value)
        {
            this.key = key;
            this.value = value;
        }
    }
}