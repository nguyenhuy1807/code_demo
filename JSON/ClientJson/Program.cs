using System;
using System.IO;
using System.Net;
using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Configuration;

namespace ClientJson
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
            string url = config["BaseUrl"];

            var person = new Person { Name = "Nguyen Van A", Age = 21 };
            string json = JsonSerializer.Serialize(person);

            byte[] data = Encoding.UTF8.GetBytes(json);

            var request = (HttpWebRequest)WebRequest.Create(url);
            request.Method = "POST";
            request.ContentType = "application/json";
            request.ContentLength = data.Length;

            using (var stream = request.GetRequestStream())
            {
                stream.Write(data, 0, data.Length);
            }

            var response = (HttpWebResponse)request.GetResponse();
            using var reader = new StreamReader(response.GetResponseStream());
            Console.WriteLine($"[ClientJson] Response: {reader.ReadToEnd()}");
        }

        public class Person
        {
            public string Name { get; set; }
            public int Age { get; set; }
        }
    }
}
