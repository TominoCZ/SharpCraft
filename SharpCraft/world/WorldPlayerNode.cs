using OpenTK;
using SharpCraft.block;
using SharpCraft.entity;
using SharpCraft.item;
using System;
#pragma warning disable 618

namespace SharpCraft.world
{
    [Serializable]
    internal class WorldPlayerNode
    {
        private readonly float _pitch;
        private readonly float _yaw;

        private readonly Vector3 _pos;

        private readonly ItemStackNode[] _hotbar;
        private readonly ItemStackNode[] _inventory;

        private readonly float _healthPercentage;

        public WorldPlayerNode(EntityPlayerSp player)
        {
            _pitch = SharpCraft.Instance.Camera.Pitch;
            _yaw = SharpCraft.Instance.Camera.Yaw;
            _pos = player.Pos;

            _hotbar = new ItemStackNode[player.Hotbar.Length];
            _inventory = new ItemStackNode[player.Inventory.Length];

            for (var i = 0; i < player.Hotbar.Length; i++)
            {
                ItemStack stack = player.Hotbar[i];

                if (TryParseStack(player, stack, out var node))
                    _hotbar[i] = node;
            }
            for (var i = 0; i < player.Inventory.Length; i++)
            {
                ItemStack stack = player.Inventory[i];

                if (TryParseStack(player, stack, out var node))
                    _inventory[i] = node;
            }

            _healthPercentage = player.Health;
        }

        private bool TryParseStack(EntityPlayerSp player, ItemStack stack, out ItemStackNode node)
        {
            if (stack == null || stack.IsEmpty)
            {
                node = null;
                return false;
            }

            node = new ItemStackNode();

            if (stack.Item is ItemBlock itemBlock)
            {
                node.IsBlock = true;
                node.LocalItemId = player.World.GetLocalBlockId(itemBlock.Block.UnlocalizedName);
                node.Count = stack.Count;
                node.Meta = stack.Meta;
            }
            else
            {
                //TODO - saving items
                //node.LocalItemId = player.World.GetLocalItemId(stack.Item.UnlocalizedName); //TODO
                //node.Count = stack.Count;
                //node.Meta = stack.Meta;
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

            if (node.IsBlock)
                item = ItemRegistry.GetItem(BlockRegistry.GetBlock(world.GetLocalBlockName(node.LocalItemId)));
            //else
                //item = ItemRegistry.GetItem(ItemRegistry.GetItem(world.GetLocalItemName(node.LocalItemId))); //TODO

            stack = new ItemStack(item, node.Count, node.Meta);

            return true;
        }

        public EntityPlayerSp GetPlayer(World world)
        {
            EntityPlayerSp player = new EntityPlayerSp(world, _pos);
            SharpCraft.Instance.Camera.Pitch = _pitch;
            SharpCraft.Instance.Camera.Yaw = _yaw;

            for (int i = 0; i < _hotbar.Length; i++)
            {
                if (TryParseStack(world, _hotbar[i], out var stack))
                    player.Hotbar[i] = stack;
            }

            for (int i = 0; i < _inventory.Length; i++)
            {
                if (TryParseStack(world, _inventory[i], out var stack))
                    player.Inventory[i] = stack;
            }

            player.Health = _healthPercentage;

            return player;
        }

        [Serializable]
        private class ItemStackNode
        {
            public bool IsBlock;
            public short LocalItemId;
            public int Count;
            public short Meta;
        }
    }
}