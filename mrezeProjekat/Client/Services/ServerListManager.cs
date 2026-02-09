using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Client.Services
{
    internal class ServerListManager

    {
        private readonly string _path;

        private const string LastExitPrefix = "LAST_EXIT=";

        public ServerListManager(string filename = "servers.txt") { _path = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, filename); }

        public List<string> Load()
        {
            if (!File.Exists(_path)) return new List<string>();

            
            var result = new List<string>();
            var seen = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

            foreach (var raw in File.ReadAllLines(_path))
            {
                var line = (raw ?? string.Empty).Trim();
                if (string.IsNullOrWhiteSpace(line)) continue;
                if (line.StartsWith(LastExitPrefix, StringComparison.OrdinalIgnoreCase)) continue;

                if (seen.Add(line))
                    result.Add(line);
            }

            return result;
        }
        public DateTime? LoadLastExitUtc()
        {
            if (!File.Exists(_path)) return null;

            var first = File.ReadLines(_path).FirstOrDefault();
            if (first == null) return null;

            if (!first.StartsWith(LastExitPrefix, StringComparison.OrdinalIgnoreCase))
                return null;

            var val = first.Substring(LastExitPrefix.Length).Trim();
            if (DateTime.TryParse(val, null, System.Globalization.DateTimeStyles.RoundtripKind, out var dt))
                return dt.ToUniversalTime();

            return null;
        }

        public void SaveLastExitUtc(DateTime utcNow)
        {
            var servers = Load();
            var lines = new List<string> { LastExitPrefix + utcNow.ToString("o") };
            lines.AddRange(servers);
            File.WriteAllLines(_path, lines);
        }

        public void add(string ServerName)
        {
            if (string.IsNullOrWhiteSpace(ServerName)) return;
            var existing = new HashSet<string>(Load(), StringComparer.OrdinalIgnoreCase);
            if (existing.Contains(ServerName)) return;

            existing.Add(ServerName);

            var lines = new List<string>();
            DateTime? lastExit = LoadLastExitUtc();
            if (lastExit != null) lines.Add(LastExitPrefix + lastExit.Value.ToString("o"));

           
            var ordered = Load();
            ordered.Add(ServerName);

            lines.AddRange(ordered);

           
            File.WriteAllLines(_path, lines);
        }
    }
}
