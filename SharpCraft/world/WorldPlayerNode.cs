using System.Collections.Generic;
using OpenTK;
using SharpCraft.entity;
using SharpCraft.item;
using Newtonsoft.Json;
// ReSharper disable InconsistentNaming

#pragma warning disable 618

namespace SharpCraft.world
{
    internal class WorldPlayerNode
    {
        [JsonProperty] private readonly float pitch;
        [JsonProperty] private readonly float yaw;
        [JsonProperty] private readonly float x;
        [JsonProperty] private readonly float y;
        [JsonProperty] private readonly float z;
        [JsonProperty] private readonly Dictionary<int, ItemStackNode> hotbar = new Dictionary<int, ItemStackNode>();
        [JsonProperty] private readonly Dictionary<int, ItemStackNode> inventory = new Dictionary<int, ItemStackNode>();
        [JsonProperty] private readonly float health;

        public WorldPlayerNode()
        {
            
        }

        public WorldPlayerNode(EntityPlayerSp player)
        {
            pitch = SharpCraft.Instance.Camera.Pitch;
            yaw = SharpCraft.Instance.Camera.Yaw;

            x = player.Pos.X;
            y = player.Pos.Y;
            z = player.Pos.Z;

            for (var i = 0; i < player.Hotbar.Length; i++)
            {
                ItemStack stack = player.Hotbar[i];

                if (TryParseStack(stack, out var node))
                    hotbar.Add(i, node);
            }
            for (var i = 0; i < player.Inventory.Length; i++)
            {
                ItemStack stack = player.Inventory[i];

                if (TryParseStack(stack, out var node))
                    inventory.Add(i, node);
            }

            health = player.Health;
        }

        private bool TryParseStack(ItemStack stack, out ItemStackNode node)
        {
            if (stack == null || stack.IsEmpty)
            {
                node = null;
                return false;
            }

            node = new ItemStackNode
            {
                id = stack.Item.UnlocalizedName,
                count = stack.Count,
                meta = stack.Meta
            };

            return true;
        }

        private bool TryParseStack(ItemStackNode node, out ItemStack stack)
        {
            if (node == null)
            {
                stack = null;
                return false;
            }

            Item item = ItemRegistry.GetItem(node.id);

            stack = new ItemStack(item, node.count, node.meta);

            return true;
        }

        public EntityPlayerSp GetPlayer(World world)
        {
            EntityPlayerSp player = new EntityPlayerSp(world, new Vector3(x, y, z));

            SharpCraft.Instance.Camera.Pitch = pitch;
            SharpCraft.Instance.Camera.Yaw = yaw;

            foreach (var pair in hotbar)
            {
                if (TryParseStack(pair.Value, out var stack))
                    player.Hotbar[pair.Key] = stack;
            }

            foreach (var pair in inventory)
            {
                if (TryParseStack(pair.Value, out var stack))
                    player.Inventory[pair.Key] = stack;
            }

            player.Health = health;

            return player;
        }

        private class ItemStackNode
        {
            [JsonProperty]
            public string id;
            [JsonProperty]
            public int count;
            [JsonProperty]
            public short meta;
        }
    }
}