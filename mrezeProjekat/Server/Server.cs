using Server.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Reflection.Emit;
using System.Text;
using System.Threading.Tasks;
using Server.Network;
using Server.Services;


namespace Server
{
    internal class Server
    {
        static void Main(string[] args)
        {

           ServerManager manager = new ServerManager();
            manager.RunAdminMenu();


            Console.WriteLine("Pokretanje networka ...");
            TcpServer tcpServer = new TcpServer();
            tcpServer.Start();
            Console.WriteLine("Serveri pokrenuti..");
            UdpListener udp = new UdpListener(udpPort: 50001, tcpPortProvider: () => tcpServer.Port);
            udp.Run();
        }
    }
}
