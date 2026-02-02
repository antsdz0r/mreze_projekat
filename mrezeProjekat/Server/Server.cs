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
            TcpServer tcpServer = new TcpServer(manager);
            tcpServer.Start();
            Console.WriteLine($"TCP server je pokrenut na portu : {tcpServer.Port}");
            Task.Run(() => tcpServer.AcceptClients());
            UdpListener udp = new UdpListener(udpPort: 50001, tcpPortProvider: () => tcpServer.Port);
            udp.Run();
        }
    }
}
