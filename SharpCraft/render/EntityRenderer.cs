namespace SharpCraft
{
    internal class EntityRenderer
    {
        public void render(float partialTicks)
        {
            for (int i = 0; i < Game.INSTANCE.world._entities.Count; i++)
            {
                var entity = Game.INSTANCE.world._entities[i];

                entity.Render(partialTicks);
            }
        }
    }
}