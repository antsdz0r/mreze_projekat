using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Server.Models
{
    public class Poruka
    {
        public string Posiljalac { get; set; }
        public string VremenskiTrenutak { get; set; }

        public string Sadrzaj { get; set; }
    }
}
