using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
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
        string baseUrl = $"http://localhost:{port}/books";

        using var client = new HttpClient();

        Console.WriteLine($"Connecting to HTTP server at {baseUrl}");
        bool running = true;

        while (running)
        {
            Console.WriteLine("\n====== Library Client (HTTP) ======");
            Console.WriteLine("1. List Books");
            Console.WriteLine("2. Create Book");
            Console.WriteLine("3. Quit");
            Console.Write("Choose: ");
            string choice = Console.ReadLine();

            switch (choice)
            {
                case "1":
                    await ListBooksAsync(client, baseUrl);
                    break;
                case "2":
                    await CreateBookAsync(client, baseUrl);
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
    }

    static async Task ListBooksAsync(HttpClient client, string baseUrl)
    {
        try
        {
            var response = await client.GetAsync(baseUrl);
            string json = await response.Content.ReadAsStringAsync();

            if (!response.IsSuccessStatusCode)
            {
                Console.WriteLine("Error: " + json);
                return;
            }

            var books = JsonSerializer.Deserialize<List<Book>>(json, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
            Console.WriteLine("\n====== Book List ======");
            if (books?.Count > 0)
            {
                foreach (var b in books)
                    Console.WriteLine($"ID: {b.BookId} | Title: {b.Title} | Year: {b.PublicationYear}");
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

    static async Task CreateBookAsync(HttpClient client, string baseUrl)
    {
        Console.Write("Enter title: ");
        string title = Console.ReadLine() ?? "";
        Console.Write("Enter year: ");
        int.TryParse(Console.ReadLine(), out int year);

        var newBook = new Book { Title = title, PublicationYear = year };
        string json = JsonSerializer.Serialize(newBook);

        try
        {
            var content = new StringContent(json, Encoding.UTF8, "application/json");
            var response = await client.PostAsync(baseUrl, content);
            string responseBody = await response.Content.ReadAsStringAsync();

            if (response.IsSuccessStatusCode)
            {
                var created = JsonSerializer.Deserialize<Book>(responseBody, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
                Console.WriteLine($"Book created successfully. ID: {created?.BookId}");
            }
            else
            {
                Console.WriteLine("Error: " + responseBody);
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine("Error: " + ex.Message);
        }
    }
}
