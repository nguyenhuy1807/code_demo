using System;
using System.IO;
using System.Net.Sockets;
using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Configuration;

namespace client
{
    class Program
    {
        static void Main()
        {
            // Load config
            var config = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
                .Build();

            int port = int.Parse(config["Port"]);

            Console.WriteLine($"[ClientJson TCP] Connecting to port {port}...");
            using TcpClient client = new TcpClient("localhost", port);
            using NetworkStream stream = client.GetStream();
            using StreamWriter writer = new StreamWriter(stream, Encoding.UTF8) { AutoFlush = true };
            using StreamReader reader = new StreamReader(stream, Encoding.UTF8);

            Console.WriteLine("[ClientJson TCP] Connected!");
            Console.WriteLine("Type 'exit' to quit.\n");

            while (true)
            {
                Console.Write("Enter name: ");
                string name = Console.ReadLine();
                if (string.Equals(name, "exit", StringComparison.OrdinalIgnoreCase))
                    break;

                Console.Write("Enter age: ");
                string ageInput = Console.ReadLine();
                if (string.Equals(ageInput, "exit", StringComparison.OrdinalIgnoreCase))
                    break;

                if (!int.TryParse(ageInput, out int age))
                {
                    Console.WriteLine("Invalid age. Try again.\n");
                    continue;
                }

                var person = new Person { Name = name, Age = age };
                string json = JsonSerializer.Serialize(person);

                writer.WriteLine(json);
                

                string response = reader.ReadLine();
                Console.WriteLine($"[ClientJson TCP] Received: {response}\n");
            }

            Console.WriteLine("[ClientJson TCP] Disconnected.");
        }

        public class Person
        {
            public string Name { get; set; }
            public int Age { get; set; }
        }
    }
}
