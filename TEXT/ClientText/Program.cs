using System;
using System.IO;
using System.Net.Sockets;
using System.Text;
using Microsoft.Extensions.Configuration;

class ClientText
{
    static void Main()
    {
        // Đọc cấu hình từ appsettings.json
        var config = new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
            .Build();

        int port = int.Parse(config["Port"]);

        TcpClient client = new TcpClient("localhost", port);
        NetworkStream stream = client.GetStream();

        string message = "Hello from Client (Text Mode)";
        byte[] data = Encoding.UTF8.GetBytes(message);
        stream.Write(data, 0, data.Length);

        // Nhận phản hồi từ server
        byte[] buffer = new byte[1024];
        int byteCount = stream.Read(buffer, 0, buffer.Length);
        string response = Encoding.UTF8.GetString(buffer, 0, byteCount);

        Console.WriteLine($"Server says: {response}");

        client.Close();
    }
}
