using System;
using System.Diagnostics;
using SharpCraft.block;
using SharpCraft.entity;
using SharpCraft.model;

namespace SharpCraft.world
{
    class DestroyProgress
    {
        public EntityPlayerSP Player { get; }
        public BlockPos Pos { get; }

        public bool Destroyed { get; private set; }

        //public float Percentage => (float) _progress / _blockHardness;

        public float PartialProgress
        {
            get
            {
                var partialTicks = SharpCraft.Instance.GetPartialTicksForRender();

                return Math.Clamp((_lastProgress + (Progress - _lastProgress) * partialTicks) / _blockHardness, 0, _blockHardness);
            }
        }

        public int Progress
        {
            get => _progress;

            set
            {
                _lastProgress = _progress;
                _progress = value;

                ProgressChanged();
            }
        }

        private int _progress;
        private int _lastProgress;

        private int _blockHardness;

        public DestroyProgress(BlockPos pos, EntityPlayerSP player) //TODO - once again, change this for multiplayer once its implemented
        {
            Pos = pos;

            var block = player.world.GetBlock(pos);
            var meta = player.world.GetMetadata(pos);

            _blockHardness = 12; //TODO
            //_blockHardness = ModelRegistry.GetBlockState(block, meta).hardness;
            Player = player;
        }

        private void ProgressChanged()
        {
            if (!Destroyed && _progress >= _blockHardness)
            {
                Destroyed = true;

                Player.BreakBlock();
            }
        }
    }
}
