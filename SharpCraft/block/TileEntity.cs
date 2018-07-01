using SharpCraft.util;
using SharpCraft.world;

namespace SharpCraft.block
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