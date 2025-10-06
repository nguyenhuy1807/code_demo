using System;
using System.IO;
using System.Net;
using System.Text;
using Microsoft.Extensions.Configuration;

namespace ServerJson
{
    class Program
    {
        static void Main()
        {
            var config = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
                .Build();

            string baseUrl = config["BaseUrl"];
            Console.WriteLine($"[ServerJson] Running at {baseUrl}");

            HttpListener listener = new HttpListener();
            listener.Prefixes.Add(baseUrl);
            listener.Start();

            while (true)
            {
                var context = listener.GetContext();
                using var reader = new StreamReader(context.Request.InputStream, context.Request.ContentEncoding);
                string data = reader.ReadToEnd();

                Console.WriteLine($"Received JSON: {data}");

                string response = "{\"message\":\"Server received JSON successfully!\"}";
                byte[] buffer = Encoding.UTF8.GetBytes(response);
                context.Response.ContentType = "application/json";
                context.Response.OutputStream.Write(buffer, 0, buffer.Length);
                context.Response.OutputStream.Close();

                Console.WriteLine("Response sent.\n");
            }
        }
    }
}
