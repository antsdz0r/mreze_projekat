using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using Client.Network;

namespace Client.Services
{
    internal class ClientApp
    {
        private const int UDP_PORT = 50001;
        private readonly IPAddress SERVER_IP = IPAddress.Parse("192.168.1.7");
        int running = 1;
        int ulogovan = 0;
        int? tcpPort = null;
        public void Run()
        {
            while (running == 1)
            {
                if (ulogovan == 0)
                {
                    Console.WriteLine("Unesite vase ime/nadimak za logovanje.");
                    string ime = Console.ReadLine();

                    if (ime.ToLower() == "exit")
                    {
                        running = 0;
                        break;
                    }



                    UdpClientService udp = new UdpClientService(SERVER_IP, UDP_PORT);
                    tcpPort = udp.Login(ime);

                    

                    if (tcpPort == null)
                    {
                        Console.WriteLine("Neuspesna prijava, ili server nije pokrenut, pokusajte ponovo");
                        continue;
                    }

                    Console.WriteLine($"Dobijen TCP port : {tcpPort}");
                    try
                    {
                        TcpClientService tcp = new TcpClientService();
                        tcp.Connect(SERVER_IP, tcpPort.Value);
                        Console.WriteLine("TCP konekcija uspesna");
                        ulogovan = 1;
                    }
                    catch (SocketException)
                    {
                        Console.WriteLine("Neuspesna prijava, server nije pokrenut ili port nije dostupan.");
                        continue;
                    }

                    
                }
                else if(ulogovan == 1)
                {
                    Console.WriteLine("Izaberite server za komunikaciju.//treba da se uradi");
                    Console.WriteLine("Logout za odjavu");
                    string cmd = Console.ReadLine();

                    if (cmd.ToLower() == "logout") 
                    {
                        ulogovan = 0;
                        tcpPort = null;
                        Console.WriteLine("Uspesna odjava.");
                        Console.WriteLine();
                    }
                }
            }
        }
    }
}
