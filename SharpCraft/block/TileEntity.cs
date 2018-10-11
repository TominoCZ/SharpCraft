using SharpCraft_Client.util;
using SharpCraft_Client.world;

namespace SharpCraft_Client.block
{
    public class TileEntity
    {
        protected World World { get; }

        protected TileEntity(World world)
        {
            World = world;
        }

        public virtual void Update()
        {
        }

        public virtual void Render(float partialTicks)
        {
        }

        public virtual void ReadData(ByteBufferReader bbr)
        {
        }

        public virtual void WriteData(ByteBufferWriter bbw)
        {
        }

        public virtual void OnDestroyed(World world, BlockPos pos)
        {
        }
    }
}