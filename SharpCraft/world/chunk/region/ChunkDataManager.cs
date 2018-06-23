using SharpCraft.util;
using System;
using System.Collections.Generic;
using System.IO;

namespace SharpCraft.world.chunk.region
{
    //WHAT THE FUCK FACES MADE THIS SHIT?! WHAT THE FUCKING HELL?!
    public class ChunkDataManager<TReg, TCord> where TReg : IRegion where TCord : IRegionCord, new()
    {
        private readonly string _dataRoot;

        private readonly List<TReg> _regions = new List<TReg>();
        public RegionInfo<TCord> Info { get; }

        private readonly Func<RegionInfo<TCord>, TCord, string, TReg> _regionConstructor;
        private readonly Func<int[], TCord> _cordinateConstructor;

        public ChunkDataManager(string dataRoot, RegionInfo<TCord> info, Func<RegionInfo<TCord>, TCord, string, TReg> regionConstructor, Func<int[], TCord> cordinateConstructor)
        {
            //Console.WriteLine(Path.GetFullPath(dataRoot));
            Directory.CreateDirectory(dataRoot);
            _dataRoot = dataRoot;
            Info = info;
            _regionConstructor = regionConstructor;
            _cordinateConstructor = cordinateConstructor;
        }

        /// <summary>
        /// When ran, manager will unload region objects that were not used in a while as to keep the number of regions active at once exploding<br/>
        /// May run internally
        /// </summary>
        public void RunGc()
        {
        }

        public void WriteChunkData(TCord coordinate, byte[] data)
        {
            CalcCord(coordinate, out TCord regionCoord, out TCord regionLocalCoord);
            GetRegion(Info.CoordHash(regionCoord), regionCoord).WriteChunkData(Info.CoordHash(regionLocalCoord), data);
        }

        public byte[] GetChunkData(TCord coordinate)
        {
            CalcCord(coordinate, out TCord regionCoord, out TCord regionLocalCoord);
            return GetRegion(Info.CoordHash(regionCoord), regionCoord).ReadChunkData(Info.CoordHash(regionLocalCoord));
        }

        private int[] _regionLocalCoordI, _regionCoordI;

        private void CalcCord(TCord coordinate, out TCord regionCoord, out TCord regionLocalCoord)
        {
            if (_regionCoordI == null)
            {
                _regionLocalCoordI = new int[coordinate.Length];
                _regionCoordI = new int[coordinate.Length];
            }

            for (int i = 0; i < coordinate.Length; i++)
            {
                MathUtil.ToLocal(coordinate[i], Info.DimSize(i), out int localPos, out int partPos);
                _regionLocalCoordI[i] = localPos;
                _regionCoordI[i] = partPos;
            }

            regionLocalCoord = _cordinateConstructor(_regionLocalCoordI);
            regionCoord = _cordinateConstructor(_regionCoordI);
        }

        private TReg GetRegion(int hash, TCord regionCoord)
        {
            int pos = _regions.BinarySearch(default(TReg), Comparer<TReg>.Create((x, y) => x.GetHashCode().CompareTo(hash)));
            if (pos <= -1 || _regions[pos].GetHashCode() != hash)
            {
                if (_regions.Count == _regions.Capacity) RunGc();//try to prevent array grow

                TReg r = _regionConstructor(Info, regionCoord, _dataRoot);
                _regions.Add(r);
                _regions.Sort((x, y) => x.GetHashCode().CompareTo(y.GetHashCode()));
                return r;
            }

            return _regions[pos];
        }

        public void Cleanup()
        {
            _regions.Clear();
        }
    }
}