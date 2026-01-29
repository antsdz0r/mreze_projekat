using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Server.Models;

namespace Server.Services
{
    internal class ServerManager
    {
        private readonly Dictionary<string, List<Kanal>> _serveri = new Dictionary<string, List<Kanal>>();
        public Dictionary<string, List<Kanal>> GetServers() => _serveri;

        public void RunAdminMenu()
        {
            int running = 1;
            while (running == 1)
            {
                Console.WriteLine("1. Kreiranje novog servera");
                Console.WriteLine("2. Kreiranje kanala u vec postojecem serveru");
                Console.WriteLine("3. Prikaz svih servera sa kanalima");
                Console.WriteLine("4. Pokretanje mreze");

                switch (Console.ReadLine())
                {
                    case "1":
                        {
                            Console.WriteLine("Unesite naziv servera za kreiranje : ");
                            string nazivServera = Console.ReadLine();

                            if (!_serveri.ContainsKey(nazivServera))
                            {
                                _serveri.Add(nazivServera, new List<Kanal>());
                                Console.WriteLine("Server uspesno dodat");
                            }
                            else
                            {
                                Console.WriteLine("Server vec postoji.");
                            }
                            break;
                        }
                    case "2":
                        {
                            Console.WriteLine("Unesite naziv servera na kom zelite da kreirate kanal");
                            string nazivServera = Console.ReadLine();

                            if (_serveri.ContainsKey(nazivServera))
                            {
                                Console.WriteLine("Unesite naziv kanala koji zelite da kreirate");
                                string nazivKanala = Console.ReadLine();
                                bool postoji = _serveri[nazivServera].Any(k => k.Naziv == nazivKanala);
                                if (!postoji)
                                {
                                    _serveri[nazivServera].Add(new Kanal(nazivKanala));
                                    Console.WriteLine("Kanal uspesno dodat");
                                }
                                else
                                {
                                    Console.WriteLine("Kanal sa tim nazivom vec postoji");
                                }
                            }
                            else
                            {
                                Console.WriteLine("Server ne postoji");
                            }
                            break;
                        }

                    case "3":
                        {

                            if (_serveri.Count == 0)
                            {
                                Console.WriteLine("Ne postoji ni jedan server");
                            }

                            else
                            {
                                Console.WriteLine("--Server -> Kanali");
                                foreach (var par in _serveri)
                                {
                                    string srv = par.Key;
                                    var kanali = par.Value;

                                    if (kanali.Count == 0)
                                    {
                                        Console.WriteLine($"{srv} ne sadrzi nijedan kanal");
                                    }
                                    else
                                    {
                                        string lista = string.Join(", ", kanali.Select(k => k.Naziv));
                                        Console.WriteLine($"{srv} - > {lista}");
                                    }
                                }
                            }
                            break;
                        }

                        case "4": {
                            Console.WriteLine("Pokretanje mreze za prijavljivanje");
                            running = 0;
                            break;
                        }
                        default:
                        Console.WriteLine("Pogresna opcija");
                        break;
                }
            }
        }
    
    }
}
