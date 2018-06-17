using SharpCraft.entity;

namespace SharpCraft.render
{
    internal class EntityRenderer
    {
        public void Render(float partialTicks)
        {
            for (int i = 0; i < SharpCraft.Instance.World.Entities.Count; i++)
            {
                Entity entity = SharpCraft.Instance.World.Entities[i];

                entity.Render(partialTicks);
            }
        }
    }
}