using System;
using System.IO;
using System.Net;
using System.Net.Sockets;
using Microsoft.Extensions.Configuration;

class ServerBinary
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
        Console.WriteLine($"[ServerBinary] Listening on port {port}...");

        while (true)
        {
            TcpClient client = server.AcceptTcpClient();
            Console.WriteLine("Client connected!");

            NetworkStream stream = client.GetStream();

            // Nhận dữ liệu nhị phân
            byte[] buffer = new byte[1024];
            int byteCount = stream.Read(buffer, 0, buffer.Length);

            Console.WriteLine($"Received {byteCount} bytes");

            // Gửi phản hồi lại
            byte[] response = BitConverter.GetBytes(byteCount);
            stream.Write(response, 0, response.Length);

            client.Close();
        }
    }
}
