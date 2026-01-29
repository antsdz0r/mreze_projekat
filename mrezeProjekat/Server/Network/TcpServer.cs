using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace Server.Network
{
    internal class TcpServer
    {
        private TcpListener _listener;

        public int Port { get; private set; }
        
        public void Start()
        {
            _listener = new TcpListener(IPAddress.Any, 0);
            _listener.Start();
            Port = ((IPEndPoint)_listener.LocalEndpoint).Port;
        }
    }
}
