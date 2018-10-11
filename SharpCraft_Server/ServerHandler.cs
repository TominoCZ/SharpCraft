using System;
using System.Collections.Concurrent;
using System.Net;
using System.Threading;
using InvertedTomato.IO.Messages;
using InvertedTomato.Net.Feather;
using OpenTK;
using SharpCraft_Client;
using SharpCraft_Client.block;
using SharpCraft_Client.item;
using SharpCraft_Client.json;
using SharpCraft_Client.render.shader;
using SharpCraft_Client.util;
using SharpCraft_Client.world;
using SharpCraft_Client.world.chunk;

namespace SharpCraft_Server
{
    public class ServerHandler
    {
        private ConcurrentDictionary<EndPoint, Entity> _entities = new ConcurrentDictionary<EndPoint, Entity>();

        private FeatherTcpServer<GenericMessage> _server;

        private readonly WorldServer _world;

        private TimeSpan TickTime = TimeSpan.FromMilliseconds(50);

        private BlockRegistry _blockRegistry;
        private ItemRegistry _itemRegistry;
        private RecipeRegistry _recipeRegistry;

        public ServerHandler()
        {
            _world = new WorldServer();

            GameRegistry();

            Thread tickThread = new Thread(TickThread);
            tickThread.Start();

            _server = new FeatherTcpServer<GenericMessage>();

            _server.OnClientConnected += (endPoint) =>
            {
                if (!_entities.Keys.Contains(endPoint))
                {
                    var e = new Entity();

                    _entities.TryAdd(endPoint, e);

                    var gm = new GenericMessage();

                    gm.WriteUnsignedInteger(0);
                    gm.WriteGuid(e.ID);

                    _server.SendToAsync(endPoint, gm);
                }

                Console.WriteLine($"{endPoint} connected.");
            };

            _server.OnClientDisconnected += (endPoint, reason) =>
            {
                _entities.TryRemove(endPoint, out var removed);
                Console.WriteLine($"{endPoint} disconnected.");
            };

            _server.OnMessageReceived += (endPoint, message) =>
            {
                var id = message.ReadUnsignedInteger();

                if (_entities.TryGetValue(endPoint, out var entity))
                {
                    if (id == 1)
                    {
                        ProcessPlayerData(entity, message);
                    }
                    if (id == 2)
                    {
                        SendChunkDataTo(endPoint, message);
                    }
                }
            };

            _server.Listen(25566);

            Console.WriteLine("Server now listening for connections on port 25566. Press any key to halt.");
            Console.ReadKey(true);

            _server.Dispose();
        }

        private void GameRegistry()
        {
            _blockRegistry = new BlockRegistry();
            _itemRegistry = new ItemRegistry();
            _recipeRegistry = new RecipeRegistry();

            for (int i = 1; i < 6; i++)
            {
                //SoundEngine.RegisterSound($"block/grass/walk{i}");
            }

            //register materails
            Material.RegisterMaterial(new Material("air", true));
            Material.RegisterMaterial(new Material("tallgrass", true));
            Material.RegisterMaterial(new Material("grass", false));
            Material.RegisterMaterial(new Material("dirt", false));
            Material.RegisterMaterial(new Material("stone", false));
            Material.RegisterMaterial(new Material("wood", false));

            _blockRegistry.Put(new BlockAir());
            _blockRegistry.Put(new BlockStone());
            _blockRegistry.Put(new BlockGrass());
            _blockRegistry.Put(new BlockDirt());
            _blockRegistry.Put(new BlockCobbleStone());
            _blockRegistry.Put(new BlockPlanks());
            _blockRegistry.Put(new BlockBedrock());
            _blockRegistry.Put(new BlockLog());
            _blockRegistry.Put(new BlockLeaves());
            _blockRegistry.Put(new BlockGlass());
            _blockRegistry.Put(new BlockCraftingTable());
            _blockRegistry.Put(new BlockFurnace());
            //_blockRegistry.Put(new BlockSlab());
            _blockRegistry.Put(new BlockRare());
            _blockRegistry.Put(new BlockLadder());
            _blockRegistry.Put(new BlockTallGrass());
            _blockRegistry.Put(new BlockTulipRed());
            _blockRegistry.Put(new BlockTulipOrange());
            _blockRegistry.Put(new BlockTNT());

            //POST - MOD Blocks and Items
            //foreach (ModMain mod in _installedMods)
            //{
            //mod.OnItemsAndBlocksRegistry(new RegistryEventArgs(_blockRegistry, _itemRegistry, _recipeRegistry));
            //}

            foreach (var block in BlockRegistry.AllBlocks())
            {
                _itemRegistry.Put(new ItemBlock(block));
            }

            Item stick = new ItemStick();

            _itemRegistry.Put(new ItemPickaxe("wood"));
            _itemRegistry.Put(new ItemPickaxe("stone"));
            _itemRegistry.Put(new ItemPickaxe("rare"));
            _itemRegistry.Put(stick);

            var log = ItemRegistry.GetItem(BlockRegistry.GetBlock<BlockLog>());
            var wood = ItemRegistry.GetItem(BlockRegistry.GetBlock<BlockPlanks>());
            var cobble = ItemRegistry.GetItem(BlockRegistry.GetBlock<BlockCobbleStone>());
            var rare = ItemRegistry.GetItem(BlockRegistry.GetBlock<BlockRare>());

            Item[] recipe =
            {
                cobble, cobble, cobble,
                null, stick, null,
                null, stick, null
            };
            _recipeRegistry.RegisterRecipe(recipe, ItemRegistry.GetItem("sharpcraft", "pick_stone"));

            recipe = new[]
            {
                rare, rare, rare,
                null, stick, null,
                null, stick, null
            };
            _recipeRegistry.RegisterRecipe(recipe, ItemRegistry.GetItem("sharpcraft", "pick_rare"));

            recipe = new[]
            {
                wood, wood, wood,
                null, stick, null,
                null, stick, null
            };
            _recipeRegistry.RegisterRecipe(recipe, ItemRegistry.GetItem("sharpcraft", "pick_wood"));

            recipe = new[]
            {
                cobble, cobble, cobble,
                cobble, null, cobble,
                cobble, cobble, cobble
            };
            _recipeRegistry.RegisterRecipe(recipe, ItemRegistry.GetItem(BlockRegistry.GetBlock<BlockFurnace>()));

            recipe = new[]
            {
                wood, wood, null,
                wood, wood, null,
                null, null, null
            };
            _recipeRegistry.RegisterRecipe(recipe, ItemRegistry.GetItem(BlockRegistry.GetBlock<BlockCraftingTable>()));

            recipe = new[]
            {
                log, null, null,
                null, null, null,
                null, null, null
            };
            _recipeRegistry.RegisterRecipe(recipe, new ItemStack(wood, 4), true);

            recipe = new[]
            {
                wood, null, null,
                wood, null, null,
                null, null, null
            };
            _recipeRegistry.RegisterRecipe(recipe, new ItemStack(stick, 4));

            recipe = new[]
            {
                wood, wood, wood,
                null, wood, null,
                wood, wood, wood
            };
            _recipeRegistry.RegisterRecipe(recipe, ItemRegistry.GetItem("sharpcraft", "ladder"));

            //foreach (ModMain mod in _installedMods)
            //{
            //mod.OnRecipeRegistry(new RecipeRegistryEventArgs(_recipeRegistry));
            //}

            //LangUtil.LoadLang(SettingsManager.GetString("lang"));//TODO - find a proper placement for this line
        }

        private void TickThread()
        {
            while (true)
            {
                var now = DateTime.Now;

                Tick();

                var time = DateTime.Now - now;
                var sub = TickTime - time;

                if (sub.TotalMilliseconds > 0)
                    Thread.Sleep(sub);
            }
        }

        private void Tick()
        {
            foreach (var entity in _entities.Values)
            {
                _world.Update(entity.Pos, 3);
            }

            SendEntities();
        }

        private void ProcessPlayerData(Entity e, GenericMessage msg)
        {
            var x = msg.ReadFloat();
            var y = msg.ReadFloat();
            var z = msg.ReadFloat();

            var pos = new Vector3(x, y, z);

            x = msg.ReadFloat();
            y = msg.ReadFloat();
            z = msg.ReadFloat();

            var dir = new Vector3(x, y, z);

            e.Update(pos, dir);
        }

        private void SendChunkDataTo(EndPoint ep, GenericMessage msg)
        {
            var chp = new ChunkPos((int)msg.ReadSignedInteger(), (int)msg.ReadSignedInteger());
            var chunk = _world.GetChunk(chp);

            if (chunk == null || !chunk.HasData)
            {
                if (!_world.LoadChunk(chp))
                {
                    var chunkData = _world.GenerateChunk(chp, false);

                    _world.PutChunk(chp, chunkData);
                }

                chunk = _world.GetChunk(chp);
            }

            var raw = chunk.GetRaw();

            var m = new GenericMessage();
            m.WriteUnsignedInteger(1);
            m.WriteSignedInteger((uint)chp.x);
            m.WriteSignedInteger((uint)chp.z);

            var data = new byte[raw.Length];

            Buffer.BlockCopy(raw, 0, data, 0, data.Length);

            m.WriteUnsignedInteger((uint)data.Length);
            m.WriteByteArray(data);

            _server.SendToAsync(ep, m);
        }

        private void SendEntities()
        {
            if (_server == null)
                return;

            var msg = new GenericMessage();
            msg.WriteUnsignedInteger(2);
            msg.WriteUnsignedInteger((ulong)_entities.Count);

            foreach (var entity in _entities.Values)
            {
                msg.WriteGuid(entity.ID);

                msg.WriteFloat(entity.Pos.X);
                msg.WriteFloat(entity.Pos.Y);
                msg.WriteFloat(entity.Pos.Z);

                msg.WriteFloat(entity.LookVec.X);
                msg.WriteFloat(entity.LookVec.Y);
                msg.WriteFloat(entity.LookVec.Z);
            }

            foreach (var ep in _server.RemoteEndPoints)
            {
                _server.SendToAsync(ep, msg);
            }
        }
    }
}