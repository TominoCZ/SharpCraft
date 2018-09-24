using InvertedTomato.IO.Messages;
using InvertedTomato.Net.Feather;
using System;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.Net;
using System.Net.Sockets;
using System.Numerics;
using System.Threading;
using System.Threading.Tasks;

namespace SharpCraft_Server
{
    class Program
    {
        static ConcurrentDictionary<EndPoint, Entity> _entities = new ConcurrentDictionary<EndPoint, Entity>();

        static FeatherTcpServer<GenericMessage> _server;

        static void Main(string[] args)
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

        static TimeSpan TickTime = TimeSpan.FromMilliseconds(50);

        static void TickThread()
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

        static void Tick()
        {
            RequestPlayerData();


        }

        static void RequestPlayerData()
        {
            foreach (var endPoint in _server.RemoteEndPoints)
            {
                var gm = new GenericMessage();

                gm.WriteUnsignedInteger(1);

                _server.SendToAsync(endPoint, gm);
            }
        }

        static void ProcessPlayerData(Entity e, GenericMessage msg)
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
