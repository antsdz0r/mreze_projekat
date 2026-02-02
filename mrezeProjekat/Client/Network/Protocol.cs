using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Client.Network
{
    internal class Protocol
    {
        public static void SendLine(StreamWriter w, string line)
        {
            w.WriteLine(line ?? "");
            w.Flush();
        }

        public static string ReadLineRequired(StreamReader r)
        {
            var line = r.ReadLine();
            if (line == null) throw new IOException("Konekcija prekinuta.");
            return line.Trim();
        }

        public static void SendList(StreamWriter w, IEnumerable<string> items)
        {
            foreach (var it in items)
                w.WriteLine(it);
            w.WriteLine("END");
            w.Flush();
        }

        public static List<string> ReadList(StreamReader r)
        {
            var list = new List<string>();
            while (true)
            {
                var line = ReadLineRequired(r);
                if (line == "END") break;
                if (!string.IsNullOrWhiteSpace(line)) list.Add(line);
            }
            return list;
        }
    }
}
