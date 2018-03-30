using System;
using System.IO;
using System.Text;
using System.Threading;

namespace SharpCraft.world.chunk.region
{
	public class RegionPointerBlockImpl<TCord> : IRegion where TCord : IRegionCord
	{
		/*
		 File format:
		 Note: all pointers are absolute position of file from start

		 -- C++ view:

		 class Chunk {
		 	int size;
		 	Block* next;
		 }
		 class Block{
		 	byte[] data;
		 	Block* next;
		 }

		 Chunk[] file;

		 */
		private static readonly object CreateLock = new object();

		private readonly RegionInfo<TCord> _info;
		private readonly TCord             _cordinate;
		private readonly int               _hash;

		private readonly string _filePath;
		private          int[]  _pointerTable;
		private          bool   _hasFile;

		private readonly ReaderWriterLockSlim _rwLock = new ReaderWriterLockSlim();

		public RegionPointerBlockImpl(RegionInfo<TCord> info, TCord cordinate, string dataRoot)
		{
			_info = info;
			_hash = info.CoordHash(cordinate);

			StringBuilder nam = new StringBuilder();
			for (int i = 0; i < cordinate.Length;)
			{
				nam.Append(cordinate[i]);
				if (++i == cordinate.Length) nam.Append('.');
			}

			_filePath = $"{dataRoot}/.reg_{nam}.bin";

			_cordinate = cordinate;
			CheckFile();
		}


		private void CheckFile()
		{
			if (_hasFile) return;
			_hasFile = File.Exists(_filePath);
			if (_hasFile) InitFile();
		}

		private void InitFile()
		{
			using (var stream = Read())
			{
				var pointerCount = stream.ReadInt32();
				if (pointerCount != _info.ChunkCount) throw new IOException("Foormat change");

				_pointerTable = new int[_info.ChunkCount];

				for (var i = 0; i < _pointerTable.Length; i++)
				{
					_pointerTable[i] = stream.ReadInt32();
				}
			}
		}

		private void CreateFile()
		{
			using (var stream = File.Create(_filePath))
			{
				stream.WriteInt32(_info.ChunkCount);
				for (int i = 0; i < _info.ChunkCount; i++)
				{
					stream.WriteInt32(0);
					stream.WriteInt32(0);
				}
			}

			_pointerTable = new int[_info.ChunkCount];
		}

		public void WriteChunkData(int id, byte[] data)
		{
			lock (CreateLock)
			{
				CheckFile();
				if (!_hasFile) CreateFile();
			}

			FileStream stream;
			using (stream = Write())
			{
				int nextChunk;

				// update/init chunk in array
				if (_pointerTable[id] == 0)
				{
					stream.Seek(ArrayPos(id), SeekOrigin.Begin);
					stream.WriteInt32(nextChunk = ArrayPos(_info.ChunkCount + 1));
					stream.WriteInt32(data.Length);
				}
				else
				{
					stream.Seek(ArrayPos(id) + 4, SeekOrigin.Begin);
					nextChunk = _pointerTable[id];
					stream.WriteInt32(data.Length);
				}

				int pos = 0;
				while (pos != data.Length)
				{
					stream.Seek(nextChunk, SeekOrigin.Begin);
					var blockSize = stream.ReadInt32();
					stream.Write(data, pos, blockSize);
					pos += blockSize;

					int lastChunkPtr = (int) stream.Position;

					nextChunk = stream.ReadInt32();
					if (nextChunk == 0)
					{
						stream.Close();
						int siz = data.Length - pos - 1;
						int pointer = AllocateBlock(data, pos, siz);
						stream = Write();
						stream.Seek(lastChunkPtr, SeekOrigin.Begin);
						stream.WriteInt32(pointer);
						pos += siz;
					}
				}
			}
		}

		private int AllocateBlock(byte[] data,int pos,int size)
		{
			//TODO fill gaps

			int pointer=(int) new FileInfo(_filePath).Length-1;
			using (var stream=Write())
			{
				stream.Seek(pointer, SeekOrigin.Begin);
				stream.WriteInt32(size);//array size
				stream.Write(data,pos,size);//array data
				stream.WriteInt32(0);//next block
			}

			return pointer;
		}

		public byte[] ReadChunkData(int id)
		{
			lock (CreateLock)
			{
				CheckFile();
				if (!_hasFile) return null;
			}

			int pointer = _pointerTable[id];
			if (pointer == 0) return null;

			using (var stream = Read())
			{
				stream.Seek(ArrayPos(id) + 4, SeekOrigin.Begin);

				int totalSize = stream.ReadInt32();
				byte[] dest = new byte[totalSize];
				int destPos = 0;

				while (true)
				{
					stream.Seek(pointer, SeekOrigin.Begin);

					var blockSize = stream.ReadInt32();
					stream.Read(dest, destPos, blockSize);

					pointer = stream.ReadInt32();
					if (pointer != 0) destPos += blockSize;
					else break;
				}

				return dest;
			}
		}

		private int ArrayPos(int pos)
		{
			return 4 + pos * 2 * 4;
		}

		public void Optimize()
		{
			throw new System.NotImplementedException();
		}


		private FileStream Write()
		{
			CheckFile();
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
			private RegionPointerBlockImpl<TCord> r;

			public WriteC(RegionPointerBlockImpl<TCord> r) : base(r._filePath, FileMode.Open, FileAccess.Write, FileShare.Write)
			{
				this.r = r;
			}

			public override void Close()
			{
				base.Close();
				if (r._rwLock.IsWriteLockHeld) r._rwLock.ExitWriteLock();
			}
		}

		protected class ReadC : FileStream
		{
			private RegionPointerBlockImpl<TCord> r;

			public ReadC(RegionPointerBlockImpl<TCord> r) : base(r._filePath, FileMode.Open, FileAccess.Read, FileShare.Read)
			{
				this.r = r;
			}

			public override void Close()
			{
				base.Close();
				if (r._rwLock.IsReadLockHeld) r._rwLock.ExitReadLock();
			}
		}
	}
}