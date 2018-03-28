namespace SharpCraft.render
{
    internal class EntityRenderer
    {
        public void render(float partialTicks)
        {
            for (int i = 0; i < Game.Instance.World.Entities.Count; i++)
            {
                var entity = Game.Instance.World.Entities[i];

                entity.Render(partialTicks);
            }
        }
    }
}