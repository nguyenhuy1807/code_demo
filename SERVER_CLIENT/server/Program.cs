using System;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Text;
using Microsoft.Extensions.Configuration;

namespace server
{
    class Program
    {
        static void Main()
        {
            var config = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
                .Build();

            int port = int.Parse(config["Port"]);
            TcpListener server = new TcpListener(IPAddress.Any, port);
            server.Start();

            Console.WriteLine($"[ServerJson TCP] Listening on port {port}...\n");

            while (true)
            {
                TcpClient client = server.AcceptTcpClient();
                Console.WriteLine("[ServerJson TCP] Client connected!");

                using NetworkStream stream = client.GetStream();
                using StreamReader reader = new StreamReader(stream, Encoding.UTF8);
                using StreamWriter writer = new StreamWriter(stream, Encoding.UTF8) { AutoFlush = true };

                string line;
                while ((line = reader.ReadLine()) != null)
                {
                    Console.WriteLine($"[ServerJson TCP] Received JSON: {line}");

                    string response = "{\"message\": \"Server received JSON successfully!\"}";
                    writer.WriteLine(response);

                    Console.WriteLine("[ServerJson TCP] Responded to client.\n");
                }

                Console.WriteLine("[ServerJson TCP] Client disconnected.\n");
            }
        }
    }
}
