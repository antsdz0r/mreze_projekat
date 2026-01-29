using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace Server.Network
{
    internal class UdpListener
    {

        private readonly int _udpPort;
        private readonly Func<int> _tcpPortProvider;

        public UdpListener(int udpPort, Func<int> tcpPortProvider)
        {
            _udpPort = udpPort;
            _tcpPortProvider = tcpPortProvider;
        }

        public void Run()
        {
            Socket udpSocket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
            IPEndPoint udpEP = new IPEndPoint(IPAddress.Any, _udpPort);
            udpSocket.Bind(udpEP);
            Console.WriteLine("Udp prijava aktiva");
            byte[] buffer = new byte[1024];

            while (true) {

                EndPoint senderEp = new IPEndPoint(IPAddress.Any, 0);
                int bytes = udpSocket.ReceiveFrom(buffer, ref senderEp);
                string text  = Encoding.UTF8.GetString(buffer, 0, bytes);

                Console.WriteLine($"[UDP] Primljeno od {senderEp} : {text}");

                if (text.StartsWith("PRIJAVA"))
                {
                    int tcpPort = _tcpPortProvider();
                    string odgovor = $"TCP|{tcpPort}";
                    byte[] resp = Encoding.UTF8.GetBytes( odgovor );

                    udpSocket.SendTo(resp, senderEp);
                    Console.WriteLine($"UDP poslao {senderEp} : {odgovor}");

                }
                else
                {
                    string odgovor = "GRESKA, POSALJI: PRIJAVA";
                    udpSocket.SendTo(Encoding.UTF8.GetBytes(odgovor), senderEp);
                }
            }
        }
    }
}
