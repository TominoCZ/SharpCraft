using System;
using System.IO;
using System.Threading;

namespace SharpCraft.world
{
	public class Region
	{
		private static readonly byte BlankChunk = 0b00000001;
		private static readonly object CreateLock=new object();

		private readonly RegionInfo _info;
		private readonly int[] _cordinate;
		private readonly int _hash;

		private readonly string _filePath;

		private byte[] _cacheFlags;
		private bool _hasFile;

		private readonly ReaderWriterLockSlim _rwLock = new ReaderWriterLockSlim();
		public Region(RegionInfo info, int[] cordinate, string dataRoot)
		{
			_info = info;
			_hash = info.CoordHash(cordinate);

			_cordinate = cordinate;
			_filePath = $"{dataRoot}/.reg_{string.Join(".", cordinate)}.bin";
			_hasFile = File.Exists(_filePath);
			if(_hasFile)CacheFlags(CalcChunkCount());
		}

		private int CalcChunkCount()
		{
			
			var chunkCount = 1;
			for (var i = 0; i < _cordinate.Length; i++)
			{
				chunkCount *= _info.DimSize(i);
			}

			return chunkCount;
		}
		private void CreateAndPopulate()
		{
			var chunkCount = CalcChunkCount();
			PopulateBlank(chunkCount);
			CacheFlags(chunkCount);
		}

		private void CacheFlags(int chunkCount)
		{
			_cacheFlags = new byte[chunkCount];

			using(FileStream stream  =Read()){
				for (var i = 0; i < chunkCount; i++)
				{
					stream.Seek((_info.ChunkByteSize + 1) * i, SeekOrigin.Begin);
					_cacheFlags[i] = (byte) stream.ReadByte();
				}
			}
		}

		private void PopulateBlank(int chunkCount)
		{
			if (File.Exists(_filePath)) return;
			Console.WriteLine($"Allocating chunk at: {_filePath}");
			using (FileStream newFile = File.Create(_filePath))
			{
				byte[] blankChunk = new byte[_info.ChunkByteSize + 1];
				blankChunk[0] = BlankChunk;


				for (var i = 0; i < chunkCount; i++)
				{
					newFile.Write(blankChunk, 0, blankChunk.Length);
				}
			}
		}

		public void WriteChunkData(int id, byte[] data)
		{
			using(FileStream stream = Write())
			{
				_cacheFlags[id] = (byte) (_cacheFlags[id] & NotFlag(BlankChunk));
				
				stream.Seek((_info.ChunkByteSize + 1) * id, SeekOrigin.Begin);
				stream.WriteByte(_cacheFlags[id]);
				stream.Write(data, 0, data.Length);
				stream.Close();
			}
		}
		private byte NotFlag(byte flag) 
		{ 
			return (byte) (~flag & 0xFF); 
		} 

		public byte[] ReadChunkData(int id)
		{
			if (IsBlank(id)) return null;

			using(var stream =Read())
			{
				stream.Seek((_info.ChunkByteSize + 1) * id, SeekOrigin.Begin);

				_cacheFlags[id] = (byte) stream.ReadByte();
				if (IsBlank(id)) return null;

				var data = new byte[_info.ChunkByteSize];
				stream.Read(data, 0, data.Length);
				stream.Close();
				return data;
			}
		}

		private bool IsBlank(int id)
		{
			return !_hasFile||(_cacheFlags[id] & BlankChunk) == BlankChunk;
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

		private void checkCreateFile()
		{
			lock (CreateLock)
			{
				if(_hasFile)return;
				CreateAndPopulate();
				_hasFile = true;
			}
		}
		////////////// PAIN BEGINS HERE, READ WITH PROTTECTIVE GOGGLES ////////////////

		private FileStream Write()
		{
			checkCreateFile();
			_rwLock.EnterWriteLock();
			while (true)
			{
				try
				{
					return new WriteC(this);
				}
				catch{}
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
				catch{}
			}
		}
		protected  class WriteC : FileStream
		{
			private Region r;
			public WriteC(Region r):base(r._filePath, FileMode.Open, FileAccess.ReadWrite, FileShare.ReadWrite)
			{
				this.r = r;
			}

			public override void Close()
			{
				base.Close();
				if(r._rwLock.IsWriteLockHeld)r._rwLock.ExitWriteLock();
			}
		}
		protected  class ReadC : FileStream
		{
			private Region r;
			public ReadC(Region r):base(r._filePath, FileMode.Open, FileAccess.Read, FileShare.Read)
			{
				this.r = r;
			}

			public override void Close()
			{
				base.Close();
				if(r._rwLock.IsReadLockHeld)r._rwLock.ExitReadLock();
			}
		}
	}
}