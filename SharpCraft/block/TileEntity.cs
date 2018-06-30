using SharpCraft.world;

namespace SharpCraft.block
{
    public class TileEntity
    {
        protected TileEntity()
        {

        }

        public virtual void Update()
        {

        }

        public virtual void Render(float partialTicks)
        {

        } 

        public virtual void OnReadData()
        {

        }
        
        public virtual void OnWriteData()
        {

        }
        
        public virtual void OnDestroyed(World world, BlockPos pos)
        {

        }
    }
}