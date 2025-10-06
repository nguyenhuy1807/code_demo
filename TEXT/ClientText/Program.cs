using System;
using System.IO;
using System.Net.Sockets;
using System.Text;
using Microsoft.Extensions.Configuration;

class ClientText
{
    static void Main()
    {
        // Read config
        var config = new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
            .Build();

        int port = int.Parse(config["Port"]);

        TcpClient client = new TcpClient("localhost", port);
        NetworkStream stream = client.GetStream();
        Console.WriteLine($"[ClientText] Connected to server on port {port}");
        Console.WriteLine("Type your messages below. Type 'exit' to quit.\n");

        while (true)
        {
            Console.Write("You: ");
            string message = Console.ReadLine()?.Trim();

            if (string.IsNullOrEmpty(message))
                continue;

            if (message.Equals("exit", StringComparison.OrdinalIgnoreCase))
            {
                Console.WriteLine("[ClientText] Closing connection...");
                break;
            }

            // Send message to server
            byte[] data = Encoding.UTF8.GetBytes(message);
            stream.Write(data, 0, data.Length);

            // Receive response
            byte[] buffer = new byte[1024];
            int bytesRead = stream.Read(buffer, 0, buffer.Length);
            string response = Encoding.UTF8.GetString(buffer, 0, bytesRead);

            Console.WriteLine($"Server: {response}\n");
        }

        stream.Close();
        client.Close();
        Console.WriteLine("[ClientText] Disconnected.");
    }
}
