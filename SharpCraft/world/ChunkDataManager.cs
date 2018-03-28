using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices.ComTypes;
using SharpCraft.util;

namespace SharpCraft.world
{
	public class ChunkDataManager
	{
		private readonly string DataRoot;

		private List<Region> _regions = new List<Region>();
		public RegionInfo Info { get; }

		public ChunkDataManager(string dataRoot, RegionInfo info)
		{
			Console.WriteLine(Path.GetFullPath(dataRoot));
			Directory.CreateDirectory(dataRoot);
			DataRoot = dataRoot;
			Info = info;
		}

		public void WriteChunkData(int[] coordinate, byte[] data)
		{
			calcCord(coordinate,out var regionCoord,out var regionLocalCoord);
			GetRegion(Info.CoordHash(regionCoord),regionCoord).WriteChunkData(Info.CoordHash(regionLocalCoord),data);
		}

		public byte[] GetChunkData(int[] coordinate)
		{
			calcCord(coordinate,out var regionCoord,out var regionLocalCoord);
			return GetRegion(Info.CoordHash(regionCoord),regionCoord).ReadChunkData(Info.CoordHash(regionLocalCoord));
		}

		private void calcCord(int[] coordinate,out int[] regionCoord,out int[] regionLocalCoord)
		{
			//  .| . . . . . . . .| . . . . . . . .| . .
			// -9 -8-7-6-5-4-3-2-1  0 1 2 3 4 5 6 7  8 9 global pos
			// -2        -1               0           1  region pos
			//  7  0 1 2 3 4 5 6 7  0 1 2 3 4 5 6 7  0 1 part pos

			regionCoord = new int[coordinate.Length];
			regionLocalCoord = new int[coordinate.Length];

			for (var i = 0; i < regionCoord.Length; i++)
			{
				MathUtil.ToLocal(coordinate[i], Info.DimSize(i), out var localPos, out var partPos);
				regionLocalCoord[i] = localPos;
				regionCoord[i] = partPos;
			}
		}

		private Region GetRegion(int hash, int[] regionCoord)
		{
			var pos = _regions.BinarySearch(null, Comparer<Region>.Create((x, y) => x.GetHashCode().CompareTo(hash)));
			if (pos <= -1||_regions[pos].GetHashCode()!=hash)
			{
				var r = new Region(Info, (int[]) regionCoord.Clone(), DataRoot);
				_regions.Add(r);
				_regions.Sort((x, y) => x.GetHashCode().CompareTo(y.GetHashCode()));
				return r;
			}
			
			return _regions[pos];
		}
	}

	public class RegionInfo
	{
		private readonly int[] _dimensionSizes;
		public int ChunkByteSize { get; }
		public int DimSize(int i) => _dimensionSizes[i];

		public RegionInfo(int[] dimensionSizes, int chunkByteSize)
		{
			_dimensionSizes = dimensionSizes;
			ChunkByteSize = chunkByteSize;
		}

		public int CoordHash(int[] coordinate)
		{
			//dimensionSizes is size of a region (16x16 chunks for example)
			if (_dimensionSizes.Length != coordinate.Length) throw new Exception();

			var hash = 0;
			for (var i = 0; i < coordinate.Length; i++)
			{
				int dimensionVal = coordinate[i];
				for (var j = 0; j < i; j++)
				{
					dimensionVal *= _dimensionSizes[j];
				}

				hash += dimensionVal;
			}

			return hash;
		}
	}
}