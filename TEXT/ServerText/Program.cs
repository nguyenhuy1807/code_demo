using System;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Text;
using Microsoft.Extensions.Configuration;

class ServerText
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
        Console.WriteLine($"[ServerText] Listening on port {port}...");

        while (true)
        {
            TcpClient client = server.AcceptTcpClient();
            Console.WriteLine("[ServerText] Client connected!");

            NetworkStream stream = client.GetStream();
            byte[] buffer = new byte[1024];

            while (true)
            {
                try
                {
                    int bytesRead = stream.Read(buffer, 0, buffer.Length);
                    if (bytesRead == 0)
                    {
                        Console.WriteLine("[ServerText] Client disconnected.\n");
                        break;
                    }

                    string message = Encoding.UTF8.GetString(buffer, 0, bytesRead);
                    Console.WriteLine($"Received: {message}");

                    if (message.Trim().ToLower() == "exit")
                    {
                        Console.WriteLine("[ServerText] Client requested disconnection.\n");
                        break;
                    }

                    // Send response
                    string response = $"Server received: {message.Length} characters";
                    byte[] responseData = Encoding.UTF8.GetBytes(response);
                    stream.Write(responseData, 0, responseData.Length);
                    Console.WriteLine("Response sent.\n");
                }
                catch (IOException)
                {
                    Console.WriteLine("[ServerText] Connection lost.\n");
                    break;
                }
            }

            client.Close();
        }
    }
}
