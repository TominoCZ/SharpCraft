using System;
using System.Linq;
using InvertedTomato.IO.Messages;
using InvertedTomato.Net.Feather;
using OpenTK;
using SharpCraft_Client.entity;
using SharpCraft_Client.world;
using SharpCraft_Client.world.chunk;

namespace SharpCraft_Client
{
    internal class ServerHander
    {
        private FeatherTcpClient<GenericMessage> client;

        private Guid ClientID;

        public bool Connect(string ip, int port)
        {
            client = new FeatherTcpClient<GenericMessage>();

            client.OnDisconnected += (endPoint) =>
            {
                Console.WriteLine($"{endPoint} disconnected.");
            };

            client.OnMessageReceived += OnReceived;

            try
            {
                client.Connect(ip, port);
            }
            catch
            {
                return false;
            }

            Console.WriteLine("Connected to server");
            return true;
        }

        private void OnReceived(GenericMessage msg)
        {
            var id = msg.ReadUnsignedInteger();

            if (id == 0)
            {
                ClientID = msg.ReadGuid();
            }

            if (id == 1)
            {
                if (SharpCraft.Instance.World is WorldClientServer w)
                {
                    var x = msg.ReadSignedInteger();
                    var z = msg.ReadSignedInteger();

                    var chp = new ChunkPos((int)x, (int)z);

                    var count = msg.ReadUnsignedInteger();

                    var data = msg.ReadByteArray((int)count);

                    var raw = new short[Chunk.ChunkSize, Chunk.ChunkHeight, Chunk.ChunkSize];

                    Buffer.BlockCopy(data, 0, raw, 0, (int)count);

                    w.PutChunk(chp, raw);
                }
            }
            if (id == 2)
            {
                int count = (int)msg.ReadUnsignedInteger();

                for (int i = 0; i < count; i++)
                {
                    var guid = msg.ReadGuid();

                    var x = msg.ReadFloat();
                    var y = msg.ReadFloat();
                    var z = msg.ReadFloat();

                    var pos = new Vector3(x, y, z);

                    x = msg.ReadFloat();
                    y = msg.ReadFloat();
                    z = msg.ReadFloat();

                    var dir = new Vector3(x, y, z);

                    if (guid == ClientID || SharpCraft.Instance.World == null)
                        continue;

                    //TODO - update entities in WorldMP

                    var entity = (EntityMp)SharpCraft.Instance.World.Entities.SingleOrDefault(e => e is EntityMp emp && emp.ID == guid);

                    if (entity == null)
                    {
                        entity = new EntityMp(SharpCraft.Instance.World, pos, guid);
                        SharpCraft.Instance.World.AddEntity(entity);
                    }

                    entity.PosServer = pos;
                    entity.Dir = dir;
                }
            }
        }

        public void RequestChunk(ChunkPos pos)
        {
            var m = new GenericMessage();
            m.WriteUnsignedInteger(2);
            
            m.WriteSignedInteger(pos.x);
            m.WriteSignedInteger(pos.z);

            client.Send(m);
        }

        public void Tick()
        {
            if (client == null)
                return;

            if (SharpCraft.Instance.Player != null)
            {
                var m = new GenericMessage();
                m.WriteUnsignedInteger(1);

                var pos = SharpCraft.Instance.Player.Pos;
                var dir = SharpCraft.Instance.Camera.GetLookVec();

                m.WriteFloat(pos.X);
                m.WriteFloat(pos.Y);
                m.WriteFloat(pos.Z);

                m.WriteFloat(dir.X);
                m.WriteFloat(dir.Y);
                m.WriteFloat(dir.Z);

                client?.Send(m);
            }
        }
    }
}