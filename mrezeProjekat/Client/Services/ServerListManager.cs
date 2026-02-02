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
        public ServerListManager(string filename = "servers.txt") { _path = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, filename); }

        public List<string> Load()
        {
            if(!File.Exists(_path)) return new List<string>();
            return File.ReadAllLines(_path)
                 .Select(x => x.Trim())
                 .Where(x => !string.IsNullOrWhiteSpace(x))
                 .Distinct(StringComparer.OrdinalIgnoreCase)
                 .ToList();
        }
        public void add(string ServerName)
        {
            if(string.IsNullOrWhiteSpace(ServerName)) return;
            var existing = new HashSet<string>(Load(),StringComparer.OrdinalIgnoreCase);
            if (existing.Contains(ServerName)) return;
            File.AppendAllLines(_path, new[] {ServerName});
        }
    }
}
