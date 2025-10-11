using System;
using System.Collections.Generic;
using Microsoft.Data.SqlClient;
using System.IO;
using System.Net.Sockets;
using System.Net;
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
    private static string connectionString = "";
    private static TcpListener listener;

    static async Task Main(string[] args)
    {
        var config = new ConfigurationBuilder()
            .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
            .Build();

        connectionString = config.GetConnectionString("DefaultConnection");
        int port = int.Parse(config["ServerConfig:Port"]);

        listener = new TcpListener(IPAddress.Any, port);
        listener.Start();
        Console.WriteLine($"Server started at port {port}. Waiting for client connection...");

        while (true)
        {
            TcpClient client = await listener.AcceptTcpClientAsync();
            Console.WriteLine($"Client connected from {client.Client.RemoteEndPoint}");
            _ = Task.Run(() => HandleClient(client));
        }
    }

    static async Task HandleClient(TcpClient client)
    {
        try
        {
            using var stream = client.GetStream();
            using var reader = new StreamReader(stream, Encoding.UTF8);
            using var writer = new StreamWriter(stream, Encoding.UTF8) { AutoFlush = true };

            string request = await reader.ReadLineAsync();
            if (string.IsNullOrEmpty(request))
                return;

            string[] parts = request.Split('|');
            string command = parts[0].ToLower();
            string data = parts.Length > 1 ? parts[1] : "";

            switch (command)
            {
                case "list":
                    await HandleListBooks(writer);
                    break;
                case "create":
                    await HandleCreateBook(writer, data);
                    break;
                default:
                    await writer.WriteLineAsync(JsonSerializer.Serialize(new { error = "Invalid command" }));
                    break;
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error: {ex.Message}");
        }
        finally
        {
            client.Close();
        }
    }

    static async Task HandleListBooks(StreamWriter writer)
    {
        try
        {
            using var connection = new SqlConnection(connectionString);
            await connection.OpenAsync();

            string query = "SELECT BookId, Title, PublicationYear FROM Books";
            using var command = new SqlCommand(query, connection);
            using var reader = await command.ExecuteReaderAsync();

            var books = new List<Book>();
            while (await reader.ReadAsync())
            {
                books.Add(new Book
                {
                    BookId = reader.GetInt32(0),
                    Title = reader.GetString(1),
                    PublicationYear = reader.IsDBNull(2) ? (int?)null : reader.GetInt32(2)
                });
            }

            string json = JsonSerializer.Serialize(books);
            await writer.WriteLineAsync(json);
        }
        catch (Exception ex)
        {
            await writer.WriteLineAsync(JsonSerializer.Serialize(new { error = ex.Message }));
        }
    }

    static async Task HandleCreateBook(StreamWriter writer, string data)
    {
        try
        {
            var newBook = JsonSerializer.Deserialize<Book>(data, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
            if (newBook != null && !string.IsNullOrWhiteSpace(newBook.Title))
            {
                using var connection = new SqlConnection(connectionString);
                await connection.OpenAsync();

                string query = "INSERT INTO Books (Title, PublicationYear) VALUES (@Title, @PublicationYear); SELECT SCOPE_IDENTITY();";
                using var command = new SqlCommand(query, connection);
                command.Parameters.AddWithValue("@Title", newBook.Title);
                command.Parameters.AddWithValue("@PublicationYear", (object)newBook.PublicationYear ?? DBNull.Value);

                int newBookId = Convert.ToInt32(await command.ExecuteScalarAsync());
                newBook.BookId = newBookId;
                await writer.WriteLineAsync(JsonSerializer.Serialize(newBook));
            }
            else
            {
                await writer.WriteLineAsync(JsonSerializer.Serialize(new { error = "Invalid book data" }));
            }
        }
        catch (Exception ex)
        {
            await writer.WriteLineAsync(JsonSerializer.Serialize(new { error = ex.Message }));
        }
    }
}
