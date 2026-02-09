using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using System.Threading;
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
        private DateTime? lastExitUtc;
        private string lastExitToSend;


        public ClientApp()
        {
            _listManager = new ServerListManager();
            lastExitUtc = _listManager.LoadLastExitUtc();
            lastExitToSend = lastExitUtc == null ? "NONE" : lastExitUtc.Value.ToString("o");
        }

        int running = 1;
        int ulogovan = 0;
        int? tcpPort = null;

        public void Run()
        {
            DateTime? lastExitUtc = _listManager.LoadLastExitUtc();
            string lastExitToSend = lastExitUtc == null ? "NONE" : lastExitUtc.Value.ToString("o");

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
                        var hello = Protocol.ReadLineRequired(_reader);            
                        if (hello == "NICK?")
                        {

                            Protocol.SendLine(_writer, _nickaname);
                        }



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

                   
                    var saved = _listManager.Load();
                    var savedAvailable = saved
                        .Where(s => servers.Any(x => string.Equals(x, s, StringComparison.OrdinalIgnoreCase)))
                        .ToList();

                    if (savedAvailable.Count > 0)
                    {
                        Console.WriteLine("\n----Sacuvani serveri (prečice)----");
                        for (int i = 0; i < savedAvailable.Count; i++)
                            Console.WriteLine($"S{i + 1}. {savedAvailable[i]}");
                        Console.WriteLine("0. Prikaži sve dostupne servere");
                        Console.Write("\nIzaberi server (S-broj / 0 / naziv) : ");
                    }
                    else
                    {
                        Console.WriteLine("\n----Dostupni serveri----");
                        for (int i = 0; i < servers.Count; i++)
                            Console.WriteLine($"{i + 1}. {servers[i]}");
                        Console.Write("\nIzaberi server (broj ili naziv) : ");
                    }

                    string choice = (Console.ReadLine() ?? "").Trim();
                    string serverName = choice;

                   
                    if (savedAvailable.Count > 0 &&
                        choice.Length >= 2 &&
                        (choice[0] == 'S' || choice[0] == 's') &&
                        int.TryParse(choice.Substring(1), out int sidx) &&
                        sidx >= 1 && sidx <= savedAvailable.Count)
                    {
                        serverName = savedAvailable[sidx - 1];
                    }
                    else if (savedAvailable.Count > 0 && choice == "0")
                    {
                        
                        Console.WriteLine("\n----Dostupni serveri----");
                        for (int i = 0; i < servers.Count; i++)
                            Console.WriteLine($"{i + 1}. {servers[i]}");
                        Console.Write("\nIzaberi server (broj ili naziv) : ");
                        choice = (Console.ReadLine() ?? "").Trim();
                        serverName = choice;
                    }

                    if (int.TryParse(serverName, out int idx) && idx >= 1 && idx <= servers.Count)
                        serverName = servers[idx - 1];

                    
                    if (!servers.Any(x => string.Equals(x, serverName, StringComparison.OrdinalIgnoreCase)))
                    {
                        Console.WriteLine("Nepostojeci server. Pokusaj ponovo.");
                        continue;
                    }

                    Protocol.SendLine(_writer, serverName);
                    _listManager.add(serverName);
                    var q = Protocol.ReadLineRequired(_reader);
                    if (q.Equals("LASTEXIT?", StringComparison.OrdinalIgnoreCase))
                        Protocol.SendLine(_writer, lastExitToSend);
                    else
                        Protocol.SendLine(_writer, "NONE");

                    var unread = Protocol.ReadList(_reader);
                    if (unread.Count > 0)
                    {
                        Console.WriteLine("\nNepročitane poruke po kanalima:");
                        foreach (var u in unread)
                        {
                            var parts = u.Split('|');
                            if (parts.Length == 2) Console.WriteLine($"- {parts[0]} ({parts[1]})");
                            else Console.WriteLine($"- {u}");
                        }
                    }


                    var channels = Protocol.ReadList(_reader);
                    if (channels.Count == 0)
                    {
                        Console.WriteLine(" \nIzabrani server nema kanale. \n");
                        Protocol.SendLine(_writer, "QUIT");
                        ulogovan = 0;
                        tcpPort = null;
                        continue;
                    }
                    Console.WriteLine();
                    Console.WriteLine("----Dostupni kanali----");
                    for (int i = 0; i < channels.Count; i++)
                    {
                        Console.WriteLine($"{i + 1}.{channels[i]}");
                    }
                    Console.Write("\nIzaberi kanal(Broj ili naziv) : ");
                    string channelchoice = Console.ReadLine()?.Trim() ?? "";
                    string channelname = channelchoice;
                    if (int.TryParse(channelchoice, out int cidx) && cidx >= 1 && cidx <= channels.Count)
                        channelname = channels[cidx - 1];
                    Protocol.SendLine(_writer, channelname);
                    var history = Protocol.ReadList(_reader);
                    foreach (var h in history)
                        Console.WriteLine(h);
                    var ok = Protocol.ReadLineRequired(_reader);


                    Console.WriteLine("\nSalji poruke u kanal (QUIT za izlaz / LOGOUT za odjavu):");
                    var cts = new CancellationTokenSource();
                    var recvTask = Task.Run(() =>
                    {
                        try
                        {
                            while (!cts.IsCancellationRequested)
                            {
                                var line = _reader.ReadLine();
                                if (line == null) break;
                                line = line.Trim();
                                if (string.IsNullOrEmpty(line)) continue;

                                
                                Console.WriteLine(line);
                            }
                        }
                        catch { /* ignore */ }
                    }, cts.Token);

                    while (true)
                    {
                        string msg = Console.ReadLine() ?? "";
                        if (msg.Equals("LOGOUT", StringComparison.OrdinalIgnoreCase))
                        {
                            Protocol.SendLine(_writer, "QUIT");
                            ulogovan = 0;
                            tcpPort = null;
                            Console.WriteLine("Uspesna odjava \n");
                            _listManager.SaveLastExitUtc(DateTime.UtcNow);
                            cts.Cancel();
                            try { recvTask.Wait(200); } catch { }
                            break;
                        }
                        if (msg.Equals("QUIT", StringComparison.OrdinalIgnoreCase))
                        {
                            Protocol.SendLine(_writer, "QUIT");
                            _listManager.SaveLastExitUtc(DateTime.UtcNow);
                            cts.Cancel();
                            try { recvTask.Wait(200); } catch { }
                            running = 0;
                            break;
                        }
                        string keyword = channelname + _nickaname;

                        string encryption = KeywordCipher.Encrypt(msg, keyword);
                        Protocol.SendLine(_writer, encryption);
                    }
                }
            }
        }
    }
}
