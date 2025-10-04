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
            string xmlData = "<student><name>Nguyen Huy</name><score>100</score></student>";

            Console.WriteLine("[Client XML] Send data:\n" + xmlData);

            var request = (HttpWebRequest)WebRequest.Create(baseUrl);
            request.Method = "POST";
            request.ContentType = "application/xml";

            using (var writer = new StreamWriter(request.GetRequestStream()))
                writer.Write(xmlData);

            using var response = (HttpWebResponse)request.GetResponse();
            using var reader = new StreamReader(response.GetResponseStream());
            Console.WriteLine("[Client XML] Recieved response:\n" + reader.ReadToEnd());
        }
    }
}
