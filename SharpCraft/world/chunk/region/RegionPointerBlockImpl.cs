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

		 : header
		 int 32 - table size
		 <pointer table> - table size * pointer size

		 : chunk
		 int 32 - total size
		 pointer - block (0 if chunk does not exist)

		 :block
		 int 32 - block size
		 <data> - block size * 8
		 pointer - block (0 if end)

		 */
		private static readonly object CreateLock = new object();

		private readonly RegionInfo<TCord> _info;
		private readonly TCord             _cordinate;
		private readonly int               _hash;

		private readonly string _filePath;
		private          int[]  _pointerTable;
		private          bool   _hasFile;
		private          int    _dataStart;

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

			_dataStart = 4 + _info.ChunkCount * 4;
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

		private void ReadBlock(byte[] dest, int destPos, Stream stream, int pointer)
		{
		}

		public void WriteChunkData(int id, byte[] data)
		{
			throw new System.NotImplementedException();
		}

		public byte[] ReadChunkData(int id)
		{

			using (var stream = Read())
			{
				int pointer = _pointerTable[id] + _dataStart;
				int totalSize = stream.ReadInt32();
				byte[] dest=new byte[totalSize];

				while (true)
				{
					stream.Seek(pointer, SeekOrigin.Begin);

					var blockSize = stream.ReadInt32();
					stream.Read(dest, destPos, blockSize);

					pointer = stream.ReadInt32();
					if (pointer != 0)destPos += blockSize;
					else break;
				}
				return dest;
			}

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