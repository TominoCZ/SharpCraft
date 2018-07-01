using System;
using System.IO;
using System.Text;
using System.Threading;

namespace SharpCraft.world.chunk.region
{
    public class RegionStaticImpl<TCord> : IRegion where TCord : IRegionCord
    {
        private static readonly byte BlankChunk = 0b00000001;
        private static readonly object CreateLock = new object();

        private readonly RegionInfo<TCord> _info;
        private readonly TCord _cordinate;
        private readonly int _hash;

        private readonly string _filePath;

        private byte[] _cacheFlags;
        private bool _hasFile;

        private readonly ReaderWriterLockSlim _rwLock = new ReaderWriterLockSlim();

        public RegionStaticImpl(RegionInfo<TCord> info, TCord cordinate, string dataRoot)
        {
            _info = info;
            _hash = info.CoordHash(cordinate);

            StringBuilder nam = new StringBuilder();
            for (int i = 0; i < cordinate.Length;)
            {
                nam.Append(cordinate[i]);
                if (++i != cordinate.Length) nam.Append('.');
            }
            _filePath = $"{dataRoot}/.reg_{nam}.bin";

            _cordinate = cordinate;
            _hasFile = File.Exists(_filePath);
            if (_hasFile) CacheFlags(info.ChunkCount);
        }

        private void CreateAndPopulate()
        {
            int chunkCount = _info.ChunkCount;
            PopulateBlank(chunkCount);
            CacheFlags(chunkCount);
        }

        private void CacheFlags(int chunkCount)
        {
            _cacheFlags = new byte[chunkCount];

            using (FileStream stream = Read())
            {
                for (int i = 0; i < chunkCount; i++)
                {
                    stream.Seek((_info.ChunkByteSize + 1) * i, SeekOrigin.Begin);
                    _cacheFlags[i] = (byte)stream.ReadByte();
                }
            }
        }

        private void PopulateBlank(int chunkCount)
        {
            if (File.Exists(_filePath)) return;
            Console.WriteLine($"Allocating chunk at: {_filePath}");

            using (FileStream newFile = File.OpenWrite(_filePath))
            {
                byte[] blankChunk = new byte[_info.ChunkByteSize + 1];
                blankChunk[0] = BlankChunk;

                for (int i = 0; i < chunkCount; i++)
                {
                    newFile.Write(blankChunk, 0, blankChunk.Length);
                }
            }
        }

        public void WriteChunkData(int id, byte[] data)
        {
            using (FileStream stream = Write())
            {
                _cacheFlags[id] = (byte)(_cacheFlags[id] & NotFlag(BlankChunk));

                stream.Seek((_info.ChunkByteSize + 1) * id, SeekOrigin.Begin);
                stream.WriteByte(_cacheFlags[id]);
                stream.Write(data, 0, data.Length);
                stream.Close();
            }
        }

        private byte NotFlag(byte flag)
        {
            return (byte)(~flag & 0xFF);
        }

        public byte[] ReadChunkData(int id)
        {
            if (IsBlank(id)) return null;

            using (FileStream stream = Read())
            {
                stream.Seek((_info.ChunkByteSize + 1) * id, SeekOrigin.Begin);

                _cacheFlags[id] = (byte)stream.ReadByte();
                if (IsBlank(id)) return null;

                byte[] data = new byte[_info.ChunkByteSize];
                stream.Read(data, 0, data.Length);
                stream.Close();
                return data;
            }
        }

        public void Optimize()
        {
            throw new NotImplementedException();
        }

        private bool IsBlank(int id)
        {
            return !_hasFile || (_cacheFlags[id] & BlankChunk) == BlankChunk;
        }

        public override bool Equals(object obj)
        {
            return obj is RegionStaticImpl<TCord> other && other._cordinate.Equals(_cordinate);
        }

        public override int GetHashCode()
        {
            return _hash;
        }

        private void CheckCreateFile()
        {
            lock (CreateLock)
            {
                if (_hasFile) return;
                CreateAndPopulate();
                _hasFile = true;
            }
        }

        ////////////// PAIN BEGINS HERE, READ WITH PROTTECTIVE GOGGLES ////////////////

        private FileStream Write()
        {
            CheckCreateFile();
            _rwLock.EnterWriteLock();
            while (true)
            {
                try
                {
                    return new WriteC(this);
                }
                catch
                {
                }
            }
        }

        private FileStream Read()
        {
            _rwLock.EnterReadLock();
            while (true)
            {
                try
                {
                    return new ReadC(this);
                }
                catch
                {
                }
            }
        }

        protected class WriteC : FileStream
        {
            private readonly RegionStaticImpl<TCord> _r;

            public WriteC(RegionStaticImpl<TCord> r) : base(r._filePath, FileMode.Open, FileAccess.Write, FileShare.Write)
            {
                _r = r;
            }

            public override void Close()
            {
                base.Close();
                if (_r._rwLock.IsWriteLockHeld) _r._rwLock.ExitWriteLock();
            }
        }

        protected class ReadC : FileStream
        {
            private readonly RegionStaticImpl<TCord> _r;

            public ReadC(RegionStaticImpl<TCord> r) : base(r._filePath, FileMode.Open, FileAccess.Read, FileShare.Read)
            {
                _r = r;
            }

            public override void Close()
            {
                base.Close();
                if (_r._rwLock.IsReadLockHeld) _r._rwLock.ExitReadLock();
            }
        }

        public static RegionStaticImpl<T> Ctor<T>(RegionInfo<T> info, T cordinate, string dataRoot) where T : IRegionCord
        {
            return new RegionStaticImpl<T>(info, cordinate, dataRoot);
        }
    }
}