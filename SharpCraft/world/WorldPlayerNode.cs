using System;
using OpenTK;
using SharpCraft.entity;

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
            pitch = Game.Instance.Camera.pitch;
            yaw = Game.Instance.Camera.yaw;
            pos = player.pos;
            hotbar = player.hotbar;
        }
    }
}