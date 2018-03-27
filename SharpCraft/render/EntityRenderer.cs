namespace SharpCraft
{
    internal class EntityRenderer
    {
        public void render(float partialTicks)
        {
            for (int i = 0; i < Game.INSTANCE.world.Entities.Count; i++)
            {
                var entity = Game.INSTANCE.world.Entities[i];

                entity.Render(partialTicks);
            }
        }
    }
}