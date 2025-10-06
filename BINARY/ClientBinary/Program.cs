using System;
using System.IO;
using System.Net.Sockets;
using System.Text;
using Microsoft.Extensions.Configuration;

class ClientBinary
{
    static void Main()
    {
        var config = new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
            .Build();

        int port = int.Parse(config["Port"]);

        TcpClient client = new TcpClient("localhost", port);
        NetworkStream stream = client.GetStream();

        Console.WriteLine("[ClientBinary] Connected to server.");
        Console.WriteLine("Enter text ('exit' to exit):");

        while (true)
        {
            string input = Console.ReadLine();
            if (string.IsNullOrEmpty(input)) continue;

            byte[] data = Encoding.UTF8.GetBytes(input);
            stream.Write(data, 0, data.Length);

            if (input.Trim().ToLower() == "exit")
            {
                
                break;
            }

            byte[] buffer = new byte[1024];
            int bytesRead = stream.Read(buffer, 0, buffer.Length);
            string response = Encoding.UTF8.GetString(buffer, 0, bytesRead);
            Console.WriteLine($"[ClientBinary] Server: {response}");
        }

        stream.Close();
        client.Close();
    }
}
