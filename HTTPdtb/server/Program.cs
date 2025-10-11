using System;
using System.Collections.Generic;
using Microsoft.Data.SqlClient;
using System.Net;
using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Configuration;
using System.Threading.Tasks;

public class Book
{
    public int BookId { get; set; }
    public string Title { get; set; } = "";
    public int? PublicationYear { get; set; }
}

class Program
{
    private static string connectionString = "";
    private static int port;

    static async Task Main(string[] args)
    {
        var config = new ConfigurationBuilder()
            .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
            .Build();

        connectionString = config.GetConnectionString("DefaultConnection");
        port = int.Parse(config["ServerConfig:Port"]);

        var listener = new HttpListener();
        string prefix = $"http://localhost:{port}/";
        listener.Prefixes.Add(prefix);
        listener.Start();

        Console.WriteLine($"HTTP Server started at {prefix}");
        Console.WriteLine("Waiting for client requests...");

        while (true)
        {
            var context = await listener.GetContextAsync();
            _ = Task.Run(() => HandleRequest(context));
        }
    }

    private static async Task HandleRequest(HttpListenerContext context)
    {
        try
        {
            string path = context.Request.Url.AbsolutePath.ToLower();
            string method = context.Request.HttpMethod;

            if (path == "/books" && method == "GET")
            {
                await HandleListBooks(context);
            }
            else if (path == "/books" && method == "POST")
            {
                await HandleCreateBook(context);
            }
            else
            {
                context.Response.StatusCode = 404;
                await WriteResponse(context, new { error = "Not Found" });
            }
        }
        catch (Exception ex)
        {
            await WriteResponse(context, new { error = ex.Message }, 500);
        }
    }

    private static async Task HandleListBooks(HttpListenerContext context)
    {
        try
        {
            var books = new List<Book>();
            using var connection = new SqlConnection(connectionString);
            await connection.OpenAsync();

            string query = "SELECT BookId, Title, PublicationYear FROM Books";
            using var command = new SqlCommand(query, connection);
            using var reader = await command.ExecuteReaderAsync();

            while (await reader.ReadAsync())
            {
                books.Add(new Book
                {
                    BookId = reader.GetInt32(0),
                    Title = reader.GetString(1),
                    PublicationYear = reader.IsDBNull(2) ? (int?)null : reader.GetInt32(2)
                });
            }

            await WriteResponse(context, books);
        }
        catch (Exception ex)
        {
            await WriteResponse(context, new { error = ex.Message }, 500);
        }
    }

    private static async Task HandleCreateBook(HttpListenerContext context)
    {
        try
        {
            using var reader = new StreamReader(context.Request.InputStream, context.Request.ContentEncoding);
            string body = await reader.ReadToEndAsync();

            var newBook = JsonSerializer.Deserialize<Book>(body, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
            if (newBook == null || string.IsNullOrWhiteSpace(newBook.Title))
            {
                await WriteResponse(context, new { error = "Invalid data" }, 400);
                return;
            }

            using var connection = new SqlConnection(connectionString);
            await connection.OpenAsync();

            string query = "INSERT INTO Books (Title, PublicationYear) VALUES (@Title, @PublicationYear); SELECT SCOPE_IDENTITY();";
            using var command = new SqlCommand(query, connection);
            command.Parameters.AddWithValue("@Title", newBook.Title);
            command.Parameters.AddWithValue("@PublicationYear", (object)newBook.PublicationYear ?? DBNull.Value);

            int newBookId = Convert.ToInt32(await command.ExecuteScalarAsync());
            newBook.BookId = newBookId;

            await WriteResponse(context, newBook, 201);
        }
        catch (Exception ex)
        {
            await WriteResponse(context, new { error = ex.Message }, 500);
        }
    }

    private static async Task WriteResponse(HttpListenerContext context, object data, int statusCode = 200)
    {
        context.Response.StatusCode = statusCode;
        context.Response.ContentType = "application/json";
        string json = JsonSerializer.Serialize(data);
        byte[] buffer = Encoding.UTF8.GetBytes(json);
        await context.Response.OutputStream.WriteAsync(buffer, 0, buffer.Length);
        context.Response.Close();
    }
}
