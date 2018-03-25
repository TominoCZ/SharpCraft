﻿using OpenTK;
using System;

namespace SharpCraft
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
            pitch = Camera.INSTANCE.pitch;
            yaw = Camera.INSTANCE.yaw;
            pos = player.pos;
            hotbar = player.hotbar;
        }
    }
}