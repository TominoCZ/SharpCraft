using System;
using System.Linq;
using OpenTK;
using SharpCraft.util;
using SharpCraft.world;

namespace SharpCraft.entity
{
    internal class EntityItem : Entity
    {
        private ItemStack stack;

        private int entityAge;

        protected EntityItem(World world, Vector3 pos, ItemStack stack) : base(world, pos)
        {
        }

        public override void Update()
        {
            base.Update();

            if (onGround && ++entityAge >= 200)
                SetDead();

            EntityPlayerSP closestPlayer = null;
            float smallestDistance = float.MaxValue;

            foreach (var player in world.Entities.OfType<EntityPlayerSP>()) //TODO change this for multiplayer
            {
                var dist = MathUtil.Distance(player.pos, pos);

                if (dist < smallestDistance && dist <= 2 && !player.HasFullInventory)
                {
                    smallestDistance = dist;
                    closestPlayer = player;
                }
            }

            if (closestPlayer != null)
            {
                closestPlayer.OnPickup(stack);
                SetDead();
            }
        }

        public override void Render(float particalTicks)
        {

        }
    }
}
