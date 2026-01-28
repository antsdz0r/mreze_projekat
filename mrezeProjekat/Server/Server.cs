using Server.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Reflection.Emit;
using System.Text;
using System.Threading.Tasks;


namespace Server
{
    internal class Server
    {
        static void Main(string[] args)
        {

            Dictionary<string, List<Kanal>> serveri = new Dictionary<string, List<Kanal>>();

            int running = 1;
            while (running == 1)
            {
                Console.WriteLine("1. Kreiranje novog servera.");
                Console.WriteLine("2. Kreiranje kanala u vec postojecem serveru.");
                Console.WriteLine("3. Prikaz svih servera i kanala.");
                Console.WriteLine("4. Pokretanje servera.");

                switch (Console.ReadLine())
                {
                    case "1":
                        {
                            Console.WriteLine("Unesite ime servera.");
                            string nazivServera = Console.ReadLine();

                            if (!serveri.ContainsKey(nazivServera))
                            {
                                serveri.Add(nazivServera, new List<Kanal>());
                                Console.WriteLine("Server dodat");
                            }
                            else
                            {
                                Console.WriteLine("Server vec postoji.");
                            }
                            break;
                        }


                    case "2":
                        {
                            Console.WriteLine("Unesite naziv servera u kom zelite da dodate kanal.");
                            string imeServera = Console.ReadLine();
                            if (serveri.ContainsKey(imeServera))
                            {

                                Console.WriteLine("Unesite naziv kanala koji zelite da dodate u server : ");
                                string nazivKanala = Console.ReadLine();
                                bool postoji = serveri[imeServera].Any(k => k.Naziv == nazivKanala);
                                if (!postoji)
                                {
                                    Kanal noviKanal = new Kanal(nazivKanala);
                                    serveri[imeServera].Add(noviKanal);
                                    Console.WriteLine("Kanal uspesno dodat.");
                                }
                                else
                                {
                                    Console.WriteLine("Kanal vec postoji.");
                                }
                            }
                            else
                            {
                                Console.WriteLine("Server sa tim imenom ne postoji");
                            }
                            break;
                        }

                    case "3":
                        {
                            if (serveri.Count == 0)
                            {
                                Console.WriteLine("Ne postoji nijedan server.");
                            }
                            else
                            {
                                Console.WriteLine("--Server - > kanali --");
                                foreach (var par in serveri)
                                {
                                    string imeServera = par.Key;
                                    List<Kanal> kanali = par.Value;
                                    if(kanali.Count == 0)
                                    {
                                        Console.WriteLine($"{imeServera} -> (nema kanala)");
                                    }
                                    else
                                    {
                                        string listaKanala = string.Join(", ", kanali.Select(k => k.Naziv));
                                        Console.WriteLine($"{imeServera} -> {listaKanala}");
                                    }
                                }
                            }
                            break;
                        }
                        case "4": {

                            Console.WriteLine("Pokretanje networka.");
                            running = 0;
                            break;
                        }

                }
            }


            TcpListener tcpListener = new TcpListener(IPAddress.Any, 0);
            tcpListener.Start();
            int tcpPort = ((IPEndPoint)tcpListener.LocalEndpoint).Port;
            Console.WriteLine($"TCP listener pokrenut na instanci : {tcpPort}");

            Socket udpSocket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
            IPEndPoint udpEP = new IPEndPoint(IPAddress.Any, 50001);
            udpSocket.Bind(udpEP);
            Console.WriteLine($"Server je pokrenut sada se mozete prijaviti. {udpEP}");
            EndPoint posiljaocEP = new IPEndPoint(IPAddress.Any, 0);

            byte[] buffer = new byte[4096];

            while (true)
            {
                EndPoint senderEP = new IPEndPoint(IPAddress.Any, 0);
                int bytes = udpSocket.ReceiveFrom(buffer, ref senderEP);
                string text = Encoding.UTF8.GetString(buffer,0, bytes);

                Console.WriteLine($"[UDP] Primljeno od {senderEP} : {text}");

                if (text.StartsWith("PRIJAVA"))
                {
                    string odgovor = $"TCP|{tcpPort}";
                    byte[] resp = Encoding.UTF8.GetBytes( odgovor );
                    udpSocket.SendTo(resp, senderEP);
                    Console.WriteLine($"[UDP] poslao {senderEP} : {odgovor}");
                }
                else
                {
                    string odgovor = "GRESKA| Posalji : PRIJAVA";
                    byte[] resp = Encoding .UTF8.GetBytes( odgovor );
                    udpSocket.SendTo(resp, senderEP);
                }
            }
        }
    }
}
