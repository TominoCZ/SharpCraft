using System;
using System.Collections.Concurrent;
using System.Net;
using System.Threading;
using InvertedTomato.IO.Messages;
using InvertedTomato.Net.Feather;
using OpenTK;
using SharpCraft_Client.world;
using SharpCraft_Client.world.chunk;

namespace SharpCraft_Server
{
    public class ServerHandler
    {
        private ConcurrentDictionary<EndPoint, Entity> _entities = new ConcurrentDictionary<EndPoint, Entity>();

        private FeatherTcpServer<GenericMessage> _server;

        private readonly WorldServer _world;

        public ServerHandler()
        {
            _world = new WorldServer();

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

        private TimeSpan TickTime = TimeSpan.FromMilliseconds(50);

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

            if (!chunk.HasData)
                _world.LoadChunk(chp);

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