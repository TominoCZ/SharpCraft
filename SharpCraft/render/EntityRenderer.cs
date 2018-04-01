using OpenTK;

namespace SharpCraft.render
{
    internal class EntityRenderer
    {
        public void Render(float partialTicks)
        {
            for (int i = 0; i < SharpCraft.Instance.World.Entities.Count; i++)
            {
                var entity = SharpCraft.Instance.World.Entities[i];

                entity.Render(partialTicks);
            }
        }
    }
}