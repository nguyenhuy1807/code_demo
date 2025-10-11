using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Sockets;
using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Configuration;

public class Book
{
    public int BookId { get; set; }
    public string Title { get; set; } = "";
    public int? PublicationYear { get; set; }
}

class Program
{
    static async Task Main(string[] args)
    {
        var config = new ConfigurationBuilder()
            .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
            .Build();

        int port = int.Parse(config["ServerConfig:Port"]);

        Console.WriteLine("Connecting to server...");
        TcpClient client;
        try
        {
            client = new TcpClient("localhost", port);
            Console.WriteLine("Connected to server.\n");
        }
        catch
        {
            Console.WriteLine("Failed to connect to server. Please start the server first.");
            return;
        }

        using var stream = client.GetStream();
        using var writer = new StreamWriter(stream, Encoding.UTF8) { AutoFlush = true };
        using var reader = new StreamReader(stream, Encoding.UTF8);

        bool running = true;
        while (running)
        {
            Console.WriteLine("====== Library Client ======");
            Console.WriteLine("1. List Books");
            Console.WriteLine("2. Create Book");
            Console.WriteLine("3. Quit");
            Console.Write("Choose an option: ");
            string choice = Console.ReadLine();

            switch (choice)
            {
                case "1":
                    await ListBooksAsync(writer, reader);
                    break;
                case "2":
                    await CreateBookAsync(writer, reader);
                    break;
                case "3":
                    running = false;
                    Console.WriteLine("Goodbye.");
                    break;
                default:
                    Console.WriteLine("Invalid choice.");
                    break;
            }
        }

        client.Close();
    }

    static async Task ListBooksAsync(StreamWriter writer, StreamReader reader)
    {
        try
        {
            await writer.WriteLineAsync("list|");
            string response = await reader.ReadLineAsync();

            if (string.IsNullOrEmpty(response))
            {
                Console.WriteLine("No response from server.");
                return;
            }

            if (response.StartsWith("{\"error\""))
            {
                Console.WriteLine("Server error: " + response);
                return;
            }

            var books = JsonSerializer.Deserialize<List<Book>>(response, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
            Console.WriteLine("\n====== Book List ======");
            if (books?.Count > 0)
            {
                foreach (var book in books)
                    Console.WriteLine($"ID: {book.BookId} | Title: {book.Title} | Year: {book.PublicationYear}");
            }
            else
            {
                Console.WriteLine("No books found.");
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine("Error: " + ex.Message);
        }
    }

    static async Task CreateBookAsync(StreamWriter writer, StreamReader reader)
    {
        Console.Write("Enter title: ");
        string title = Console.ReadLine() ?? "";

        Console.Write("Enter publication year: ");
        int.TryParse(Console.ReadLine(), out int year);

        try
        {
            var newBook = new Book { Title = title, PublicationYear = year };
            string json = JsonSerializer.Serialize(newBook);

            await writer.WriteLineAsync($"create|{json}");
            string response = await reader.ReadLineAsync();

            if (response.StartsWith("{\"error\""))
            {
                Console.WriteLine("Server error: " + response);
                return;
            }

            var result = JsonSerializer.Deserialize<Book>(response, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
            Console.WriteLine($"Book created successfully. ID: {result?.BookId}");
        }
        catch (Exception ex)
        {
            Console.WriteLine("Error: " + ex.Message);
        }
    }
}
