using InvertedTomato.IO.Messages;
using InvertedTomato.Net.Feather;
using System;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Numerics;
using System.Threading;
using System.Threading.Tasks;

namespace SharpCraft_Server
{
    class Program
    {
        private static ServerHandler _serverHandler;

        static void Main(string[] args)
        {
            _serverHandler = new ServerHandler();
        }
    }

    class ServerHandler
    {
        ConcurrentDictionary<EndPoint, Entity> _entities = new ConcurrentDictionary<EndPoint, Entity>();

        FeatherTcpServer<GenericMessage> _server;

        public ServerHandler()
        {
            Thread tickThread = new Thread(TickThread);
            tickThread.Start();

            _server = new FeatherTcpServer<GenericMessage>();

            _server.OnClientConnected += (endPoint) =>
            {
                bool add = false;

                foreach (var pair in _entities)
                {
                    if (pair.Key != endPoint)
                    {
                        add = true;
                        break;
                    }
                }

                if (add)
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
                }
            };

            _server.Listen(12345);

            Console.WriteLine("Server now listening for connections on port 12345. Press any key to halt.");
            Console.ReadKey(true);

            _server.Dispose();
        }

        TimeSpan TickTime = TimeSpan.FromMilliseconds(50);

        void TickThread()
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

        void Tick()
        {
            RequestPlayerData();

            SendEntities();
        }

        void RequestPlayerData()
        {
            foreach (var endPoint in _server.RemoteEndPoints)
            {
                var gm = new GenericMessage();

                gm.WriteUnsignedInteger(1);

                _server.SendToAsync(endPoint, gm);
            }
        }

        void ProcessPlayerData(Entity e, GenericMessage msg)
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

        void SendEntities()
        {
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

    class Entity
    {
        public Guid ID = Guid.NewGuid();

        public Vector3 Pos;
        public Vector3 LookVec;

        public void Update(Vector3 pos, Vector3 dir)
        {
            Pos = pos;

            LookVec = dir;
        }
    }

    /*class TCPHandler
    {
        private ConcurrentQueue<> _queue = new ConcurrentQueue<>();

        private BlockingCollection<TcpClient> _connections = new BlockingCollection<TcpClient>();

        private TcpListener _listener;

        private Thread _acceptClientThread;
        private Thread _acceptMessageThread;
        private Thread _processMessageThread;

        public TCPHandler(int port)
        {
            _listener = new TcpListener(IPAddress.Any, port);

            _acceptClientThread = new Thread(AcceptConnection);

            Task.Run(async () =>
            {
                while (true)
                {
                    AcceptMessage();

                    Task.Delay(1);
                }
            });

            _processMessageThread = new Thread(AcceptConnection);
        }

        private void AcceptConnection()
        {
            while (true)
            {
                var client = _listener.AcceptTcpClient();

                _connections.Add(client);
            }
        }

        private void AcceptMessage()
        {

        }
    }*/
}
