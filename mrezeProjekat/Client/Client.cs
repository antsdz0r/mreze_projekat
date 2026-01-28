using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using System.Net;



namespace Client
{
    internal class Program
    {
        static void Main(string[] args)
        {

            const int UDP_PORT = 50001;

            Socket clientSocket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);


            IPAddress serverIp = IPAddress.Parse("192.168.1.7");
            IPEndPoint serverUdpEP = new IPEndPoint(serverIp, UDP_PORT);
            //clientSocket.Bind(destinationEP);

            Console.WriteLine("Unesite vase ime/nadimak");
            string ime = Console.ReadLine();
            //string poruka = "";
            /*while (poruka != "PRIJAVA")
            {
                Console.WriteLine("Unesite PRIJAVA za logovanje.");
                poruka = Console.ReadLine();
            }
            Console.WriteLine("Uspesna prijava!");*/
            string prijava = $"PRIJAVA|{ime}";
            byte[] data = Encoding.UTF8.GetBytes(prijava);
            clientSocket.SendTo(data, serverUdpEP);

            Console.WriteLine("[UDP] Poslato : " + prijava);

            // odgovor od servera za uspesnu prijvau i dostavljanje slobodne tcp socketa za komunikaciju


            byte[] buffer = new byte[1024];
            EndPoint fromEP = new IPEndPoint(IPAddress.Any, 0);
            int bytes = clientSocket.ReceiveFrom(buffer, ref fromEP);
            string resp = Encoding.UTF8.GetString(buffer, 0 , bytes);

            Console.WriteLine("[UDP] odgovor " + resp);

            if (resp.StartsWith("TCP|"))
            {
                string portStr = resp.Split('|')[1];
                int tcpPort = int.Parse(portStr);
                Console.WriteLine($"Povezan na tcp port {tcpPort}");

            }
            else
            {
                Console.WriteLine("Neuspesna prijava.");
            }
            
        }
    }
}
