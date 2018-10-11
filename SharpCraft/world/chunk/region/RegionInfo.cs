using System;

namespace SharpCraft_Client.world.chunk.region
{
    public class RegionInfo<TCord> where TCord : IRegionCord
    {
        private readonly int[] _dimensionSizes;
        public int ChunkByteSize { get; }
        public int ChunkCount { get; }

        public int DimSize(int i) => _dimensionSizes[i];

        public RegionInfo(int[] dimensionSizes, int chunkByteSize)
        {
            _dimensionSizes = dimensionSizes;
            ChunkByteSize = chunkByteSize;

            ChunkCount = 1;
            for (int i = 0; i < dimensionSizes.Length; i++)
            {
                ChunkCount *= DimSize(i);
            }
        }

        public int CoordHash(TCord coordinate)
        {
            //dimensionSizes is size of a region (16x16 chunks for example)
            if (_dimensionSizes.Length != coordinate.Length) throw new Exception();

            int hash = 0;
            for (int i = 0; i < coordinate.Length; i++)
            {
                int dimensionVal = coordinate[i];
                for (int j = 0; j < i; j++)
                {
                    dimensionVal *= _dimensionSizes[j];
                }

                hash += dimensionVal;
            }

            return hash;
        }
    }
}