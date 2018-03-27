using System;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Threading;

namespace SharpCraft.world
{
	public class Region
	{
		public static readonly byte BLANK_CHUNK = 0b00000001;

		private readonly RegionInfo info;
		private readonly int[] cordinate;
		private readonly int _hash;

		private readonly string _filePath;
		private int _readLock;
		private bool _writeLock;
		private byte[] _cacheFlags;

		public Region(RegionInfo info, int[] cordinate, string DataRoot)
		{
			this.info = info;
			_hash = info.CoordHash(cordinate);

			this.cordinate = cordinate;
			_filePath = $"{DataRoot}/.reg_{string.Join(".", cordinate)}.bin";
			CreateAndPopulate();
		}

		private void CreateAndPopulate()
		{
			var chunkCount = 1;
			for (var i = 0; i < cordinate.Length; i++)
			{
				chunkCount *= info.DimSize(i);
			}

			if (!File.Exists(_filePath)) populateBlank(chunkCount);
			cacheFlags(chunkCount);
			Console.WriteLine("Allocated chunk at: "+_filePath);
		}

		private void cacheFlags(int chunkCount)
		{
			_cacheFlags = new byte[chunkCount];
			FileStream stream = null;
			try
			{
				stream = Read();
				for (int i = 0; i < chunkCount; i++)
				{
					stream.Seek((info.ChunkByteSize + 1) * i, SeekOrigin.Begin);
					_cacheFlags[i] = (byte) stream.ReadByte();
				}
			}
			finally
			{
				stream?.Close();
				_readLock--;
			}
		}

		private void populateBlank(int chunkCount)
		{
			using (FileStream newFile = File.Create(_filePath))
			{
				byte[] blankChunk = new byte[info.ChunkByteSize + 1];
				blankChunk[0] |= BLANK_CHUNK;


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
				stream.Seek((info.ChunkByteSize + 1) * id, SeekOrigin.Begin);
				//stream.WriteByte((byte) (_cacheFlags[id]&NotFlag(BLANK_CHUNK)));
				stream.WriteByte(2);
				
				stream.Write(data, 0, data.Length);
			}
			finally
			{
				stream?.Close();
				_writeLock = false;
			}
		}

		public byte[] ReadChunkData(int id)
		{
			FileStream stream = null;
			try
			{
				stream = Read();
				stream.Seek((info.ChunkByteSize + 1) * id, SeekOrigin.Begin);

				byte flags = _cacheFlags[id] = (byte) stream.ReadByte();

				if ((flags & BLANK_CHUNK) == BLANK_CHUNK)
				{
					return null;
				}

				var data = new byte[info.ChunkByteSize];
				stream.Read(data, 0, data.Length);
				return data;
			}
			finally
			{
				stream?.Close();
				_readLock--;
			}
		}

		private FileStream Read()
		{
			//it's ok to have multiple reads at once but you can't mix read and write streams at once so locks are here for that
			while (_writeLock) Thread.Sleep(1); //file is currently written to, can't read or you'll get corrupted data
			_readLock++;
			return File.Open(_filePath, FileMode.Open, FileAccess.Read);
		}

		private FileStream Write()
		{
			_writeLock = true; //ok no more reads can be called, those that are reading need to finish then this can run
			while (_readLock > 0) Thread.Sleep(1);
			return File.Open(_filePath, FileMode.Open, FileAccess.Write);
		}

		public override bool Equals(object obj)
		{
			Region other = obj as Region;
			return other != null && other.cordinate.Equals(cordinate);
		}

		public override int GetHashCode()
		{
			return _hash;
		}

		private byte NotFlag(byte flag)
		{
			return (byte) (~flag & 0xFF);
		}
	}
}