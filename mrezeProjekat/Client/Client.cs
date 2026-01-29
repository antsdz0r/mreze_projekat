using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using System.Net;
using Client.Services;



namespace Client
{
    internal class Program
    {
        static void Main(string[] args)
        {

            ClientApp app = new ClientApp();
            app.Run();
            /*Console.WriteLine("Unesite ENTER za izlaz.");
            Console.ReadLine();*/
        }
    }
}
