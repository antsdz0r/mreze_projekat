using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using Client.Network;
using System.IO;
using System.Runtime.InteropServices;
namespace Client.Services
{
    internal class ClientApp
    {
        private const int UDP_PORT = 50001;
        private readonly IPAddress SERVER_IP = IPAddress.Parse("127.0.0.1");
        private StreamReader _reader;
        private StreamWriter _writer;
        private TcpClientService _tcp;
        private string _nickaname;
        private readonly ServerListManager _listManager;
        public ClientApp()
        {
            _listManager = new ServerListManager();
        }

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
                        _nickaname = ime;
                        _tcp = new TcpClientService();
                        _tcp.Connect(SERVER_IP, tcpPort.Value);                                        
                        NetworkStream ns = _tcp.GetStream();
                        _reader = new StreamReader(ns, Encoding.UTF8);
                        _writer = new StreamWriter(ns, Encoding.UTF8) { AutoFlush = true };
                        var hello = Protocol.ReadLineRequired(_reader);             //bug fixing
                        if (hello == "NICK?")
                            Protocol.SendLine(_writer, _nickaname);


                        Console.WriteLine("TCP konekcija uspesna");
                        ulogovan = 1;
                    }
                    catch (SocketException)
                    {
                        Console.WriteLine("Neuspesna prijava, server nije pokrenut ili port nije dostupan.");
                        continue;
                    }


                }
                else if (ulogovan == 1)
                {
                    var servers = Protocol.ReadList(_reader);
                    if (servers.Count == 0)
                    {
                        Console.WriteLine("Ne postoji nijedan server, prvo napravi server!");
                        ulogovan = 0;
                        tcpPort = null;
                        continue;
                    }
                    Console.WriteLine("\n Dostupni server :");
                    for (int i = 0; i < servers.Count; i++)

                        Console.WriteLine($"{i + 1}.{servers[i]}");
                    Console.Write("\n Izaberi server (broj ili naziv)");
                    string choice = Console.ReadLine()?.Trim() ?? "";
                    string serverName = choice;

                    if (int.TryParse(choice, out int idx) && idx >= 1 && idx <= servers.Count)
                        serverName = servers[idx - 1];

                    Protocol.SendLine(_writer, serverName);
                    _listManager.add(serverName);
                    var channels = Protocol.ReadList(_reader);

                    if (channels.Count == 0) {
                        Console.WriteLine(" \nIzabrani server nema kanale. \n");
                        Protocol.SendLine(_writer, "QUIT");
                        ulogovan = 0;
                        tcpPort = null;
                        continue;
                    }
                    Console.Write("\n Izaberi kanal(Broj ili naziv)");
                    string channelchoice = Console.ReadLine()?.Trim() ?? "";
                    string channelname = channelchoice;
                    Protocol.SendLine(_writer, channelname);
                    var ok = Protocol.ReadLineRequired(_reader); // ocekuje "ok", HANDSHAKE

                    if (int.TryParse(channelchoice, out int cidx) && cidx >= 1 && cidx <= channels.Count)
                        channelname = channels[cidx - 1];
                    Console.WriteLine("\nSalji poruke u kanal (QUIT za izlaz / LOGOUT za odjavu):");
                    while (true)
                    {
                        string msg = Console.ReadLine() ?? "";
                        if (msg.Equals("LOGOUT", StringComparison.OrdinalIgnoreCase))
                        {
                            Protocol.SendLine(_writer, "QUIT");
                            ulogovan = 0;
                            tcpPort = null;
                            Console.WriteLine("Uspesna odjava \n");
                            break;
                    }
                        if (msg.Equals("QUIT", StringComparison.OrdinalIgnoreCase))
                        {
                            Protocol.SendLine(_writer, "QUIT");
                            running = 0;
                            break;
                        }
                        Protocol.SendLine(_writer, msg);
                    }
                }
            }
        }
    }
}
