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
            // Đọc file config
            var config = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
                .Build();

            string baseUrl = config["BaseUrl"];
            Console.WriteLine($"[Server JSON] is running at {baseUrl}");

            // Khởi tạo server
            HttpListener listener = new HttpListener();
            listener.Prefixes.Add(baseUrl);
            listener.Start();
            Console.WriteLine("Listening request...");

            while (true)
            {
                var context = listener.GetContext();
                using var reader = new StreamReader(context.Request.InputStream, context.Request.ContentEncoding);
                string data = reader.ReadToEnd();

                Console.WriteLine($"Recieve JSON: {data}");

                // Phản hồi
                string response = "{\"message\":\"Server recieved JSON successfully!\"}";
                byte[] buffer = Encoding.UTF8.GetBytes(response);
                context.Response.ContentType = "application/json";
                context.Response.OutputStream.Write(buffer, 0, buffer.Length);
                context.Response.OutputStream.Close();

                Console.WriteLine("Responded JSON.\n");
            }
        }
    }
}
