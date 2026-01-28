using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


namespace Server.Models
{
    public class Kanal
    {
        public string Naziv { get; set; }
        public List<Poruka> Poruke { get; set; } = new List<Poruka>();

        public Kanal(string naziv)
        {
            Naziv = naziv;
            Poruke = new List<Poruka>();
        }
    }
}
