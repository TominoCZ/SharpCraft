namespace SharpCraft_Server
{
    internal class Program
    {
        private static ServerHandler _serverHandler;
        
        static void Main(string[] args)
        {
            _serverHandler = new ServerHandler();
        }
    }
}