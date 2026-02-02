using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Server.Models
{
    internal class ClientInfo
    {
        public string Nickname { get; set; } = "";
        public string SelectedServer { get; set; } = "";
        public string SelectedChannel { get; set; } = "";
    }
}
