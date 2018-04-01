using OpenTK;
using SharpCraft.entity;
using System;
using SharpCraft.item;

namespace SharpCraft.world
{
    [Serializable]
    internal class WorldPlayerNode
    {
        public float pitch;
        public float yaw;

        public Vector3 pos;

        public ItemStack[] hotbar;

        public WorldPlayerNode(EntityPlayerSP player)
        {
            pitch = SharpCraft.Instance.Camera.pitch;
            yaw = SharpCraft.Instance.Camera.yaw;
            pos = player.pos;
            hotbar = player.hotbar;
        }
    }
}