using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using Server.Services;
namespace Server.Network
{
    internal class TcpServer
    {
        private TcpListener _listener;
        private ServerManager _serverManager;

        public int Port { get; private set; }
        public TcpServer(ServerManager serverManager)
        {
            _serverManager = serverManager;
        }


        public void Start()
        {
            _listener = new TcpListener(IPAddress.Any, 0);
            _listener.Start();
            Port = ((IPEndPoint)_listener.LocalEndpoint).Port;

            Console.WriteLine($"TCP server je pokrenut na portu{Port}");
            
        }
        public void AcceptClients() {
            var handler = new ClientHandler(_serverManager);
            Console.WriteLine("TCP: cekam klijente...");
            while (true)
            {
                TcpClient client = _listener.AcceptTcpClient();
                Console.WriteLine("TCP klijent povezan");
                handler.HandleClient(client);
            }
        }
    }
}
