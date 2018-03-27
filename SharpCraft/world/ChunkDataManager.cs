using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Security.Permissions;

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
			var regionCoord = new int[coordinate.Length];
			var regionLocalCoord = new int[coordinate.Length];

			for (var i = 0; i < regionCoord.Length; i++)
			{
				var size = Info.DimSize(i);
				var cord = coordinate[i];
				regionCoord[i] = cord / size;
				regionLocalCoord[i] = cord % size;
			}

			GetRegion(Info.CoordHash(regionCoord), regionCoord).WriteChunkData(Info.CoordHash(regionLocalCoord),data);
		}

		public byte[] GetChunkData(int[] coordinate)
		{
			var regionCoord = new int[coordinate.Length];
			var regionLocalCoord = new int[coordinate.Length];

			for (var i = 0; i < regionCoord.Length; i++)
			{
				var size = Info.DimSize(i);
				var cord = coordinate[i];
				regionCoord[i] = cord / size;
				regionLocalCoord[i] = cord % size;
			}

			return GetRegion(Info.CoordHash(regionCoord), regionCoord).ReadChunkData(Info.CoordHash(regionLocalCoord));
		}

		private Region GetRegion(int hash, int[] regionCoord)
		{
			var pos = _regions.BinarySearch(null, Comparer<Region>.Create((x, y) => x.GetHashCode().CompareTo(hash)));
			if (pos == -1)
			{
				_regions.Add(new Region(Info, (int[]) regionCoord.Clone(), DataRoot));
				_regions.Sort((x, y) => x.GetHashCode().CompareTo(y.GetHashCode()));
				return GetRegion(hash, regionCoord);
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