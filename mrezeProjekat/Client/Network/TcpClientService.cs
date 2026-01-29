using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net.Sockets;
using System.Net;

namespace Client.Network
{
    internal class TcpClientService
    {
       private TcpClient _client;
        public void Connect(IPAddress serverIP, int port)
        {
            _client = new TcpClient();
            _client.Connect(serverIP, port);
        }
    }
}
