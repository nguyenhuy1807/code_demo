using System;
using System.IO;
using System.Net;
using System.Text;
using Microsoft.Extensions.Configuration;

namespace ServerXML
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
            Console.WriteLine($"[Server XML] is running at {baseUrl}");

            HttpListener listener = new HttpListener();
            listener.Prefixes.Add(baseUrl);
            listener.Start();
            Console.WriteLine("Listening request...");

            while (true)
            {
                var context = listener.GetContext();
                using var reader = new StreamReader(context.Request.InputStream, context.Request.ContentEncoding);
                string xmlData = reader.ReadToEnd();

                Console.WriteLine($"Recieved XML:\n{xmlData}");

                string responseXml = "<response><message>Server recieved XML successfully!</message></response>";
                byte[] buffer = Encoding.UTF8.GetBytes(responseXml);
                context.Response.ContentType = "application/xml";
                context.Response.OutputStream.Write(buffer, 0, buffer.Length);
                context.Response.OutputStream.Close();

                Console.WriteLine("Responded XML.\n");
            }
        }
    }
}
