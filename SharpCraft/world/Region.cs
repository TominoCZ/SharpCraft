using System;
using System.IO;
using System.Threading;

namespace SharpCraft.world
{
	public class Region
	{
		private static readonly byte BlankChunk = 0b00000001;
		private static readonly object _createLock=new object();

		private readonly RegionInfo _info;
		private readonly int[] _cordinate;
		private readonly int _hash;

		private readonly string _filePath;

		private byte[] _cacheFlags;

		private readonly ReaderWriterLockSlim _rwLock = new ReaderWriterLockSlim();
		public Region(RegionInfo info, int[] cordinate, string dataRoot)
		{
			_info = info;
			_hash = info.CoordHash(cordinate);

			_cordinate = cordinate;
			_filePath = $"{dataRoot}/.reg_{string.Join(".", cordinate)}.bin";
			lock (_createLock)
			{
				CreateAndPopulate();
			}
		}

		private void CreateAndPopulate()
		{
			var chunkCount = 1;
			for (var i = 0; i < _cordinate.Length; i++)
			{
				chunkCount *= _info.DimSize(i);
			}

			PopulateBlank(chunkCount);
			CacheFlags(chunkCount);
		}

		private void CacheFlags(int chunkCount)
		{
			_cacheFlags = new byte[chunkCount];

			FileStream stream = null;
			try
			{
				stream = Read();
				_rwLock.EnterReadLock();
				for (int i = 0; i < chunkCount; i++)
				{
					stream.Seek((_info.ChunkByteSize + 1) * i, SeekOrigin.Begin);
					_cacheFlags[i] = (byte) stream.ReadByte();
				}
			}
			finally
			{
				stream?.Close();
				_rwLock.ExitReadLock();
			}
		}

		private void PopulateBlank(int chunkCount)
		{
			if (File.Exists(_filePath)) return;
			Console.WriteLine($"Allocating chunk at: {_filePath}");
			using (FileStream newFile = File.Create(_filePath))
			{
				byte[] blankChunk = new byte[_info.ChunkByteSize + 1];
				blankChunk[0] |= BlankChunk;


				for (var i = 0; i < chunkCount; i++)
				{
					newFile.Write(blankChunk, 0, blankChunk.Length);
				}
			}
		}

		public void WriteChunkData(int id, byte[] data)
		{
			FileStream stream = null;
			try
			{
				stream = Write();
				_rwLock.EnterWriteLock();
				if (id < 0)
				{
					Console.WriteLine(id);
				}

				stream.Seek((_info.ChunkByteSize + 1) * id, SeekOrigin.Begin);
				stream.WriteByte(2);

				stream.Write(data, 0, data.Length);
			}
			finally
			{
				stream?.Close();
				_rwLock.ExitWriteLock();
			}
		}

		public byte[] ReadChunkData(int id)
		{
			if (IsBlank(id)) return null;

			FileStream stream = null;
			try
			{
				stream = Read();
				_rwLock.EnterReadLock();
				stream.Seek((_info.ChunkByteSize + 1) * id, SeekOrigin.Begin);

				_cacheFlags[id] = (byte) stream.ReadByte();
				if (IsBlank(id)) return null;

				var data = new byte[_info.ChunkByteSize];
				stream.Read(data, 0, data.Length);
				return data;
			}
			finally
			{
				stream?.Close();
				_rwLock.ExitReadLock();
			}
		}

		private bool IsBlank(int id)
		{
			return (_cacheFlags[id] & BlankChunk) == BlankChunk;
		}

		private FileStream Read()
		{
			var fs = new FileStream(_filePath, FileMode.Open, FileAccess.Read, FileShare.Read);
			return fs;
		}

		private FileStream Write()
		{
			var fs = new FileStream(_filePath, FileMode.Open, FileAccess.Write, FileShare.Write);
			return fs;
		}

		public override bool Equals(object obj)
		{
			Region other = obj as Region;
			return other != null && other._cordinate.Equals(_cordinate);
		}

		public override int GetHashCode()
		{
			return _hash;
		}
	}
}