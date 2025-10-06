using System;
using System.IO;
using System.Net;
using System.Text;
using Microsoft.Extensions.Configuration;

namespace ClientXML
{
    class Program
    {
        static void Main()
        {
            var config = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json")
                .Build();

            string baseUrl = config["BaseUrl"];
            Console.WriteLine("[Client XML] Connected to " + baseUrl);
            Console.WriteLine("Type 'exit' at any time to quit.\n");

            while (true)
            {
                Console.Write("[Client XML] Enter student name: ");
                string name = Console.ReadLine();
                if (string.Equals(name, "exit", StringComparison.OrdinalIgnoreCase)) break;

                Console.Write("[Client XML] Enter student score: ");
                string score = Console.ReadLine();
                if (string.Equals(score, "exit", StringComparison.OrdinalIgnoreCase)) break;

                // Build XML
                string xmlData = $"<student><name>{name}</name><score>{score}</score></student>";
              

                try
                {
                    var request = (HttpWebRequest)WebRequest.Create(baseUrl);
                    request.Method = "POST";
                    request.ContentType = "application/xml";

                    using (var writer = new StreamWriter(request.GetRequestStream()))
                        writer.Write(xmlData);

                    using var response = (HttpWebResponse)request.GetResponse();
                    using var reader = new StreamReader(response.GetResponseStream());
                    Console.WriteLine("[Client XML] Server replied:\n" + reader.ReadToEnd() + "\n");
                }
                catch (Exception ex)
                {
                    Console.WriteLine("[Client XML] Error: " + ex.Message + "\n");
                }
            }

            Console.WriteLine("[Client XML] Disconnected.");
        }
    }
}
