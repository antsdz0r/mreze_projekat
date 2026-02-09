using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using Server.Models;
using Server.Services;

namespace Server.Network
{
    internal class TcpServer
    {
        private TcpListener _listener;
        private readonly ServerManager _serverManager;
        private readonly DecryptionService _decryptionService = new DecryptionService();

        public int Port { get; private set; }

        private enum ClientStage
        {
            AwaitNick,
            AwaitServer,
            AwaitLastExit,
            AwaitChannel,
            Chat
        }

        private sealed class PendingSend
        {
            public byte[] Data { get; }
            public int Offset { get; set; }

            public PendingSend(byte[] data)
            {
                Data = data ?? Array.Empty<byte>();
                Offset = 0;
            }
        }

        private sealed class ClientState
        {
            public Socket Socket { get; }
            public ClientStage Stage { get; set; } = ClientStage.AwaitNick;

            public ClientInfo Info { get; } = new ClientInfo();
            public DateTime? LastExitUtc { get; set; } = null;

            public byte[] Buffer { get; } = new byte[4096];
            public StringBuilder IncomingText { get; } = new StringBuilder();
            public Decoder Utf8Decoder { get; } = Encoding.UTF8.GetDecoder();

           
            public Queue<PendingSend> Outbox { get; } = new Queue<PendingSend>();

            public ClientState(Socket s) => Socket = s;
        }

        private readonly Dictionary<Socket, ClientState> _clients = new Dictionary<Socket, ClientState>();

        public TcpServer(ServerManager serverManager)
        {
            _serverManager = serverManager;
        }

        public void Start()
        {
            Console.WriteLine("=== TcpServer.cs (select polling + outbox + broadcast) ===");
            _listener = new TcpListener(IPAddress.Any, 0);
            _listener.Start();
            Port = ((IPEndPoint)_listener.LocalEndpoint).Port;

            Console.WriteLine($"TCP server je pokrenut na portu {Port}");
        }

        public void AcceptClients()
        {
            Console.WriteLine("TCP: polling loop...");

            while (true)
            {
                try
                {
                    
                    while (_listener.Pending())
                    {
                        TcpClient tcp = _listener.AcceptTcpClient();
                        Socket s = tcp.Client;

                        s.Blocking = false;

                        var cs = new ClientState(s);
                        _clients[s] = cs;

                        Console.WriteLine("TCP klijent povezan");
                        SendLine(cs, "NICK?");
                    }

                    if (_clients.Count > 0)
                    {
                        var readList = new List<Socket>(_clients.Keys);
                        var writeList = new List<Socket>();

                        foreach (var kv in _clients)
                        {
                            if (kv.Value.Outbox.Count > 0)
                                writeList.Add(kv.Key);
                        }

                       
                        Socket.Select(readList, writeList, null, 1000);

                      
                        foreach (var s in readList)
                        {
                            if (!_clients.TryGetValue(s, out var cs))
                                continue;

                            if (!ReadAndProcess(cs))
                                DropClient(cs);
                        }

                        
                        foreach (var s in writeList)
                        {
                            if (!_clients.TryGetValue(s, out var cs))
                                continue;

                            try
                            {
                                FlushOutbox(cs);
                            }
                            catch
                            {
                                DropClient(cs);
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"[TCP polling] greska: {ex.Message}");
                }

                Thread.Sleep(5);
            }
        }

        private bool ReadAndProcess(ClientState cs)
        {
            try
            {
                while (true)
                {
                    int n;
                    try
                    {
                        n = cs.Socket.Receive(cs.Buffer);
                    }
                    catch (SocketException se)
                    {
                        if (se.SocketErrorCode == SocketError.WouldBlock)
                            break;

                        return false;
                    }

                    if (n == 0) return false;

                    char[] chars = new char[Encoding.UTF8.GetMaxCharCount(n)];
                    int charCount = cs.Utf8Decoder.GetChars(cs.Buffer, 0, n, chars, 0);
                    cs.IncomingText.Append(chars, 0, charCount);

                    while (TryPopLine(cs.IncomingText, out string line))
                    {
                        if (!ProcessLine(cs, line))
                            return false;
                    }
                }

                return true;
            }
            catch
            {
                return false;
            }
        }

        private bool ProcessLine(ClientState cs, string line)
        {
            line = (line ?? "").Trim();

            
            if (cs.Stage == ClientStage.Chat)
            {
                if (line.Equals("QUIT", StringComparison.OrdinalIgnoreCase))
                    return false;

              
                string keyDecryption = (cs.Info.SelectedChannel ?? "") + (cs.Info.Nickname ?? "");
                string decryptedMessage = _decryptionService.Decrypt(line, keyDecryption);

                var vreme = DateTime.Now.ToString("dd.MM.yyyy HH:mm:ss");

                var kanalObj = _serverManager.GetChannel(cs.Info.SelectedServer, cs.Info.SelectedChannel);
                if (kanalObj != null)
                {
                    kanalObj.Poruke.Add(new Poruka
                    {
                        Posiljalac = cs.Info.Nickname,
                        VremenskiTrenutak = vreme,
                        Sadrzaj = decryptedMessage
                    });
                }

                Console.WriteLine($"[{vreme}]-{cs.Info.SelectedServer}:{cs.Info.SelectedChannel}:{decryptedMessage}-{cs.Info.Nickname}");

                
                BroadcastToChannel(cs, vreme, decryptedMessage);

                return true;
            }

           
            switch (cs.Stage)
            {
                case ClientStage.AwaitNick:
                    cs.Info.Nickname = line;
                    SendList(cs, _serverManager.GetServerNames());
                    cs.Stage = ClientStage.AwaitServer;
                    return true;

                case ClientStage.AwaitServer:
                    cs.Info.SelectedServer = line;

                  
                    SendLine(cs, "LASTEXIT?");
                    cs.Stage = ClientStage.AwaitLastExit;
                    return true;

                case ClientStage.AwaitLastExit:
                    if (!line.Equals("NONE", StringComparison.OrdinalIgnoreCase) &&
                        DateTime.TryParse(line, null, System.Globalization.DateTimeStyles.RoundtripKind, out var dt))
                    {
                        cs.LastExitUtc = dt.ToUniversalTime();
                    }
                    else
                    {
                        cs.LastExitUtc = null;
                    }

                    SendUnreadCounts(cs);
                    SendList(cs, _serverManager.GetChannelNames(cs.Info.SelectedServer));
                    cs.Stage = ClientStage.AwaitChannel;
                    return true;

                case ClientStage.AwaitChannel:
                    cs.Info.SelectedChannel = line;
                    SendChannelHistory(cs);
                    SendLine(cs, "ok");
                    cs.Stage = ClientStage.Chat;
                    return true;
            }

            return true;
        }

        private void SendChannelHistory(ClientState cs)
        {
            var kanal = _serverManager.GetChannel(cs.Info.SelectedServer, cs.Info.SelectedChannel);

            if (kanal == null || kanal.Poruke == null || kanal.Poruke.Count == 0)
            {
                SendList(cs, Array.Empty<string>());
                return;
            }

            var lines = new List<string>();
            foreach (var p in kanal.Poruke)
            {
                lines.Add($"[{p.VremenskiTrenutak}]-{p.Posiljalac}: {p.Sadrzaj}");
            }

            SendList(cs, lines);
        }

        private static bool TryPopLine(StringBuilder sb, out string line)
        {
            for (int i = 0; i < sb.Length; i++)
            {
                if (sb[i] == '\n')
                {
                    int len = i;
                    if (len > 0 && sb[len - 1] == '\r') len--;

                    line = sb.ToString(0, len);
                    sb.Remove(0, i + 1);
                    return true;
                }
            }

            line = null;
            return false;
        }

        private static void EnqueueSend(ClientState cs, byte[] data)
        {
            if (cs == null) return;
            cs.Outbox.Enqueue(new PendingSend(data));
        }

        private static void FlushOutbox(ClientState cs)
        {
            while (cs.Outbox.Count > 0)
            {
                var item = cs.Outbox.Peek();
                if (item.Offset >= item.Data.Length)
                {
                    cs.Outbox.Dequeue();
                    continue;
                }

                try
                {
                    int n = cs.Socket.Send(item.Data, item.Offset, item.Data.Length - item.Offset, SocketFlags.None);
                    if (n <= 0) break;

                    item.Offset += n;

                    if (item.Offset >= item.Data.Length)
                        cs.Outbox.Dequeue();
                }
                catch (SocketException se)
                {
                    if (se.SocketErrorCode == SocketError.WouldBlock)
                        break;

                    throw;
                }
            }
        }

        private static void SendLine(ClientState cs, string line)
        {
           
            var data = Encoding.UTF8.GetBytes((line ?? "") + "\n");
            EnqueueSend(cs, data);
        }

        private static void SendList(ClientState cs, IEnumerable<string> items)
        {
            var sb = new StringBuilder();
            foreach (var it in items)
                sb.Append(it ?? "").Append('\n');

            sb.Append("END\n");

            var data = Encoding.UTF8.GetBytes(sb.ToString());
            EnqueueSend(cs, data);
        }

        private void BroadcastToChannel(ClientState sender, string timestamp, string messagePlain)
        {
            
            string line = $"[{timestamp}]-{sender.Info.Nickname}: {messagePlain}";
            var data = Encoding.UTF8.GetBytes(line + "\n");

            foreach (var kv in _clients)
            {
                var cs = kv.Value;
                if (cs == sender) continue;
                if (cs.Stage != ClientStage.Chat) continue;

                if (string.Equals(cs.Info.SelectedServer, sender.Info.SelectedServer, StringComparison.Ordinal) &&
                    string.Equals(cs.Info.SelectedChannel, sender.Info.SelectedChannel, StringComparison.Ordinal))
                {
                    EnqueueSend(cs, data);
                }
            }
        }

        private void DropClient(ClientState cs)
        {
            try
            {
                Console.WriteLine($"TCP klijent {cs.Info.Nickname} diskonektovan");
                _clients.Remove(cs.Socket);

                try { cs.Socket.Shutdown(SocketShutdown.Both); } catch { }
                cs.Socket.Close();
            }
            catch {  }
        }

        private void SendUnreadCounts(ClientState cs)
        {
            if (cs.LastExitUtc == null)
            {
                SendList(cs, Array.Empty<string>());
                return;
            }

            var serverName = cs.Info.SelectedServer;
            if (!_serverManager.GetServers().TryGetValue(serverName, out var kanali) || kanali.Count == 0)
            {
                SendList(cs, Array.Empty<string>());
                return;
            }

            var unreadLines = new List<string>();
            foreach (var k in kanali)
            {
                int cnt = 0;
                foreach (var p in k.Poruke)
                {
                    if (TryParseMsgTime(p.VremenskiTrenutak, out var msgUtc) && msgUtc > cs.LastExitUtc.Value)
                        cnt++;
                }

                if (cnt > 0)
                    unreadLines.Add($"{k.Naziv}|{cnt}");
            }

            SendList(cs, unreadLines);
        }

        private static bool TryParseMsgTime(string s, out DateTime utc)
        {
            utc = default;

            if (DateTime.TryParseExact(
                    s,
                    "dd.MM.yyyy HH:mm:ss",
                    System.Globalization.CultureInfo.InvariantCulture,
                    System.Globalization.DateTimeStyles.AssumeLocal,
                    out var dt))
            {
                utc = dt.ToUniversalTime();
                return true;
            }

            return false;
        }
    }
}
