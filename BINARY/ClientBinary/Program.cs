using System;
using System.IO;
using System.Net.Sockets;
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

        // Tạo dữ liệu nhị phân giả lập (ví dụ: 256 bytes)
        byte[] data = new byte[256];
        new Random().NextBytes(data);

        stream.Write(data, 0, data.Length);

        // Nhận phản hồi
        byte[] buffer = new byte[4];
        stream.Read(buffer, 0, buffer.Length);
        int receivedSize = BitConverter.ToInt32(buffer, 0);

        Console.WriteLine($"Server confirmed: {receivedSize} bytes received");

        client.Close();
    }
}
