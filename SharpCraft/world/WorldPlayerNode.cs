using OpenTK;
using SharpCraft.entity;
using SharpCraft.item;
using System;
using SharpCraft.block;

namespace SharpCraft.world
{
    [Serializable]
    internal class WorldPlayerNode
    {
        private readonly float pitch;
        private readonly float yaw;

        private readonly Vector3 pos;

        private readonly ItemStackNode[] hotbar;
        private readonly ItemStackNode[] inventory;

        private readonly float healthPercentage;

        public WorldPlayerNode(EntityPlayerSp player)
        {
            pitch = SharpCraft.Instance.Camera.pitch;
            yaw = SharpCraft.Instance.Camera.yaw;
            pos = player.Pos;

            hotbar = new ItemStackNode[player.Hotbar.Length];
            inventory = new ItemStackNode[player.Inventory.Length];

            for (var i = 0; i < player.Hotbar.Length; i++)
            {
                ItemStack stack = player.Hotbar[i];

                if (TryParseStack(player, stack, out var node))
                    hotbar[i] = node;
            }
            for (var i = 0; i < player.Inventory.Length; i++)
            {
                ItemStack stack = player.Inventory[i];

                if (TryParseStack(player, stack, out var node))
                    inventory[i] = node;
            }

            healthPercentage = player.Health;
        }

        private bool TryParseStack(EntityPlayerSp player, ItemStack stack, out ItemStackNode node)
        {
            if (stack == null || stack.IsEmpty)
            {
                node = null;
                return false;
            }

            node = new ItemStackNode();

            if (stack.Item is ItemBlock itemBlock) //TODO - IMPORTANT! at this moment this is always true, will need to add another function for the items or something
            {
                node.IsBlock = true;
                node.LocalItemID = player.World.GetLocalBlockId(itemBlock.Block.UnlocalizedName);
                node.Count = stack.Count;
                node.Meta = stack.Meta;
            }
            else
            {
                //TODO
            }

            return true;
        }

        private bool TryParseStack(World world, ItemStackNode node, out ItemStack stack)
        {
            if (node == null)
            {
                stack = null;
                return false;
            }

            Item item = null;

            if (node.IsBlock)//TODO - IMPORTANT! at this moment this is always true, will need to add another function for the items or something
                item = ItemRegistry.GetItem(BlockRegistry.GetBlock(world.GetLocalBlockName(node.LocalItemID)).UnlocalizedName);

            stack = new ItemStack(item, node.Count, node.Meta);

            return true;
        }

        public EntityPlayerSp GetPlayer(World world)
        {
            EntityPlayerSp player = new EntityPlayerSp(world, pos);
            SharpCraft.Instance.Camera.pitch = pitch;
            SharpCraft.Instance.Camera.yaw = yaw;

            for (int i = 0; i < hotbar.Length; i++)
            {
                if (TryParseStack(world, hotbar[i], out var stack))
                    player.Hotbar[i] = stack;
            }

            for (int i = 0; i < inventory.Length; i++)
            {
                if (TryParseStack(world, inventory[i], out var stack))
                    player.Inventory[i] = stack;
            }

            player.Health = healthPercentage;

            return player;
        }

        [Serializable]
        private class ItemStackNode
        {
            public bool IsBlock;
            public short LocalItemID;
            public int Count;
            public short Meta;
        }
    }
}