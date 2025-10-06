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
            // Load config
            var config = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
                .Build();

            string url = config["BaseUrl"];
            Console.WriteLine($"[ClientJson] Sending requests to {url}");
            Console.WriteLine("Type 'exit' to stop.\n");

            while (true)
            {
                Console.Write("Enter name: ");
                string name = Console.ReadLine()?.Trim();
                if (string.Equals(name, "exit", StringComparison.OrdinalIgnoreCase)) break;

                Console.Write("Enter age: ");
                string ageInput = Console.ReadLine()?.Trim();
                if (string.Equals(ageInput, "exit", StringComparison.OrdinalIgnoreCase)) break;

                if (!int.TryParse(ageInput, out int age))
                {
                    Console.WriteLine("⚠Invalid age, please enter a number.\n");
                    continue;
                }

                var person = new Person { Name = name, Age = age };
                string json = JsonSerializer.Serialize(person);
                byte[] data = Encoding.UTF8.GetBytes(json);

                try
                {
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
                    string result = reader.ReadToEnd();

                    Console.WriteLine($"Response: {result}\n");
                }
                catch (WebException ex)
                {
                    Console.WriteLine($"Error: {ex.Message}\n");
                }
            }

            Console.WriteLine("[ClientJson] Stopped.");
        }

        public class Person
        {
            public string Name { get; set; }
            public int Age { get; set; }
        }
    }
}
