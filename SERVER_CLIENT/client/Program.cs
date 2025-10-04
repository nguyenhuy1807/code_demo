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
            // Đọc config
            var config = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
                .Build();

            int port = int.Parse(config["Port"]);

            // Chuẩn bị dữ liệu JSON
            var person = new Person { Name = "Nguyen Van A", Age = 21 };
            string json = JsonSerializer.Serialize(person);

            // Kết nối TCP đến server
            TcpClient client = new TcpClient("localhost", port);
            using var stream = client.GetStream();
            using var writer = new StreamWriter(stream, Encoding.UTF8) { AutoFlush = true };
            using var reader = new StreamReader(stream, Encoding.UTF8);

            // Gửi JSON
            writer.WriteLine(json);
            Console.WriteLine($"[ClientJson TCP] send JSON: {json}");

            // Nhận phản hồi
            string response = reader.ReadLine();
            Console.WriteLine($"Recieved from server: {response}");

            client.Close();
        }

        public class Person
        {
            public string Name { get; set; }
            public int Age { get; set; }
        }
    }
}
