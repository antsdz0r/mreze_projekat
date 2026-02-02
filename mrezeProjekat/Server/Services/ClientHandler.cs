using Server.Models;
using Server.Network;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace Server.Services
{
    public class ClientHandler
    {
        private readonly ServerManager _serverManager;

        public ClientHandler(ServerManager serverManager)
        {
            _serverManager = serverManager;
        }

        public void HandleClient(TcpClient tcpClient)
        {
            var ns = tcpClient.GetStream();
            var r = new StreamReader(ns);
            var w = new StreamWriter(ns) { AutoFlush = true };

            var client = new ClientInfo();

            
            Protocol.SendLine(w, "NICK?");
            client.Nickname = Protocol.ReadLineRequired(r);
            Protocol.SendList(w, _serverManager.GetServerNames());
            client.SelectedServer = Protocol.ReadLineRequired(r);
            Protocol.SendList(w, _serverManager.GetChannelNames(client.SelectedServer));
            client.SelectedChannel = Protocol.ReadLineRequired(r);
            Protocol.SendLine(w, "OK");
            while (true)
            {
                var msg = Protocol.ReadLineRequired(r);
                if (msg.Equals("QUIT", StringComparison.OrdinalIgnoreCase))
                    break;

                var vreme = DateTime.Now.ToString("dd.MM.yyyy HH:mm:ss");

                var kanalObj = _serverManager.GetChannel(client.SelectedServer, client.SelectedChannel);
                if (kanalObj != null)
                {
                    kanalObj.Poruke.Add(new Poruka
                    {
                        Posiljalac = client.Nickname,
                        VremenskiTrenutak = vreme,
                        Sadrzaj = msg
                    });
                }

                Console.WriteLine($"[{vreme}]-{client.SelectedServer}:{client.SelectedChannel}:{msg}-{client.Nickname}");
            }
        }
    }
}
