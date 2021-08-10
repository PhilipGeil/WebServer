using System;
using System.Net;
using System.Net.Sockets;
using System.Text;

namespace WebServer
{
    class Program
    {
        static void Main(string[] args)
        {
            Server s = new Server();
            s.Start(IPAddress.Parse("127.0.0.1"), 8080, 65000, @"C:\Users\phil2643\source\repos\WebServer\WebServer\public");
            Console.ReadLine();
        }
    }
}
