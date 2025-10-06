using System;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Text;
using Microsoft.Extensions.Configuration;

class ServerBinary
{
    static void Main()
    {
        // Đọc file cấu hình
        var config = new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
            .Build();

        int port = int.Parse(config["Port"]);

        TcpListener server = new TcpListener(IPAddress.Any, port);
        server.Start();
        Console.WriteLine($"[ServerBinary] Listening on port {port}...");

        while (true)
        {
            TcpClient client = server.AcceptTcpClient();
            Console.WriteLine("[ServerBinary] Client connected!");

            NetworkStream stream = client.GetStream();

            // Vòng lặp nhận dữ liệu liên tục từ client
            while (true)
            {
                try
                {
                    byte[] buffer = new byte[1024];
                    int bytesRead = stream.Read(buffer, 0, buffer.Length);

                    if (bytesRead == 0)
                    {
                        Console.WriteLine("[ServerBinary] Client disconnected.");
                        break;
                    }

                    string message = Encoding.UTF8.GetString(buffer, 0, bytesRead);
                    Console.WriteLine($"[ServerBinary] Received: {message}");

                    if (message.Trim().ToLower() == "exit")
                    {
                        Console.WriteLine("[ServerBinary] Client requested disconnection.");
                        break;
                    }

                    string response = $"Server received: {message.Length} bytes";
                    byte[] responseBytes = Encoding.UTF8.GetBytes(response);
                    stream.Write(responseBytes, 0, responseBytes.Length);
                }
                catch (IOException)
                {
                    Console.WriteLine("[ServerBinary] Connection lost.");
                    break;
                }
            }

            client.Close();
        }
    }
}
