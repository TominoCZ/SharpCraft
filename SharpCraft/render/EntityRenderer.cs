using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SharpCraft
{
    class EntityRenderer
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
