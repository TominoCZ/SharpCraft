using OpenTK;
using SharpCraft.entity;
using System;
using SharpCraft.item;

namespace SharpCraft.world
{
    [Serializable]
    internal class WorldPlayerNode
    {
        private float pitch;
        private float yaw;

        private Vector3 pos;

        private ItemStack[] hotbar;
        private ItemStack[] inventory;

        public WorldPlayerNode(EntityPlayerSP player)
        {
            pitch = SharpCraft.Instance.Camera.pitch;
            yaw = SharpCraft.Instance.Camera.yaw;
            pos = player.pos;
            hotbar = player.hotbar;
            inventory = player.inventory;
        }

        public EntityPlayerSP GetPlayer(World world)
        {
            var player = new EntityPlayerSP(world, pos);
            SharpCraft.Instance.Camera.pitch = pitch;
            SharpCraft.Instance.Camera.yaw = yaw;

            for (int i = 0; i < hotbar.Length; i++)
            {
                player.hotbar[i] = hotbar[i];
            }

            for (int i = 0; i < inventory.Length; i++)
            {
                player.inventory[i] = inventory[i];
            }

            return player;
        }
    }
}