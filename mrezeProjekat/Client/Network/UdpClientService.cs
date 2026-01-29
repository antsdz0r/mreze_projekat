using System;
using System.Collections.Generic;
using System.Data.SqlTypes;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace Client.Network
{
    internal class UdpClientService
    {

        private readonly IPAddress _serverIP;
        private readonly int _udpPort;

        public UdpClientService(IPAddress serverIP, int udpPort)
        {
            _serverIP = serverIP;
            _udpPort = udpPort;
        }

        public int ? Login(string ime)
        {
            Socket clientSocket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);

            clientSocket.ReceiveTimeout = 3000;
            try
            {

                IPEndPoint serverUdpEP = new IPEndPoint(_serverIP, _udpPort);

                string prijava = $"PRIJAVA|{ime}";
                byte[] data = Encoding.UTF8.GetBytes(prijava);
                clientSocket.SendTo(data, serverUdpEP);

                byte[] buffer = new byte[1024];

                EndPoint fromEP = new IPEndPoint(IPAddress.Any, 0);
                int bytes = clientSocket.ReceiveFrom(buffer, ref fromEP);

                string resp = Encoding.UTF8.GetString(buffer, 0, bytes);

                if (resp.StartsWith("TCP|"))
                {
                    return int.Parse(resp.Split('|')[1]);
                }
                return null;
            }
            catch (SocketException)
            {
                Console.WriteLine("Server jos nije pustio liniju za privaju.");
                return null;
            }
            finally
            {
                clientSocket.Close();
            }
        }
    }
}
