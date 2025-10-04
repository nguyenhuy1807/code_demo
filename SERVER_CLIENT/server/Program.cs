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

            Console.WriteLine($"[ServerJson TCP] listening at {port}...");

            while (true)
            {
                TcpClient client = server.AcceptTcpClient();
                Console.WriteLine("Client connecting successfully!");

                using var stream = client.GetStream();
                using var reader = new StreamReader(stream, Encoding.UTF8);
                using var writer = new StreamWriter(stream, Encoding.UTF8) { AutoFlush = true };

                string receivedJson = reader.ReadLine();
                Console.WriteLine($"Data JSON recieved: {receivedJson}");

                string response = "{\"message\": \"Server recieved JSON successfully!\"}";
                writer.WriteLine(response);

                client.Close();
                Console.WriteLine("Respond to client.\n");
            }
        }
    }
}
