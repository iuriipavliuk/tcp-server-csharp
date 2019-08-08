using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace OneAppTcpCommunication
{
    internal static class Program
    {
        public static void Main(string[] args)
        {
            TcpServerProgram.Start();
            //Task.Delay(1000).Wait();
            //TcpClientProgram.Start();
        }
    }
}