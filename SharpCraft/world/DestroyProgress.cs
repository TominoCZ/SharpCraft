using SharpCraft.block;
using SharpCraft.entity;
using System;
using OpenTK;

namespace SharpCraft.world
{
    internal class DestroyProgress
    {
        public EntityPlayerSp Player { get; }
        public BlockPos Pos { get; }

        public bool Destroyed { get; private set; }

        public float PartialProgress
        {
            get
            {
                float partialTicks = SharpCraft.Instance.GetPartialTicksForRender();

                return MathHelper.Clamp((_lastProgress + (Progress - _lastProgress) * partialTicks) / _blockHardness, 0, _blockHardness);
            }
        }

        public float Progress
        {
            get => _progress;

            set
            {
                _lastProgress = _progress;
                _progress = value;

                ProgressChanged();
            }
        }

        private float _progress;
        private float _lastProgress;

        private readonly int _blockHardness;

        public DestroyProgress(BlockPos pos, EntityPlayerSp player) //TODO - once again, change this for multiplayer once its implemented
        {
            Pos = pos;

            BlockState block = player.World.GetBlockState(pos);

            _blockHardness = block.Block.Hardness;
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