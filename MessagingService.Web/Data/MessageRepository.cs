using MessagingService.Web.Models;
using Npgsql;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace MessagingService.Web.Data;

public class MessageRepository(IConfiguration configuration, ILogger<MessageRepository> logger) : IMessageRepository
{
    private readonly string _connectionString = configuration.GetConnectionString("DefaultConnection")!;
    private readonly ILogger<MessageRepository> _logger = logger;

    public async Task<int> AddMessageAsync(Message message)
    {
        try
        {
            await using var connection = new NpgsqlConnection(_connectionString);
            await connection.OpenAsync();

            const string sql = "INSERT INTO messages (text, timestamp, client_order) VALUES (@Text, @Timestamp, @ClientOrder) RETURNING id;";
            await using var cmd = new NpgsqlCommand(sql, connection);
            cmd.Parameters.AddWithValue("Text", message.Text);
            cmd.Parameters.AddWithValue("Timestamp", message.Timestamp);
            cmd.Parameters.AddWithValue("ClientOrder", message.ClientOrder);

            var id = await cmd.ExecuteScalarAsync();
            _logger.LogInformation("Message added with ID: {Id}", id);
            return Convert.ToInt32(id);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error adding message: {ErrorMessage}", ex.Message);
            throw;
        }
    }

    public async Task<IAsyncEnumerable<Message>> GetMessagesAsync(DateTime startTime, DateTime endTime)
    {
        var messages = new List<Message>();
        try
        {
            await using var connection = new NpgsqlConnection(_connectionString);
            await connection.OpenAsync();

            const string sql = "SELECT id, text, timestamp, client_order FROM messages WHERE timestamp BETWEEN @StartTime AND @EndTime ORDER BY timestamp;";
            await using var cmd = new NpgsqlCommand(sql, connection);
            cmd.Parameters.AddWithValue("StartTime", startTime);
            cmd.Parameters.AddWithValue("EndTime", endTime);

            await using var reader = await cmd.ExecuteReaderAsync();
            while (await reader.ReadAsync())
            {
                var message = new Message
                {
                    Id = reader.GetInt32(0),
                    Text = reader.GetString(1),
                    Timestamp = reader.GetDateTime(2),
                    ClientOrder = reader.GetInt32(3)
                };
                messages.Add(message);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting messages: {ErrorMessage}", ex.Message);
            throw;
        }
        return (IAsyncEnumerable<Message>)messages;
    }

    Task IMessageRepository.AddMessageAsync(Message message)
    {
        throw new NotImplementedException();
    }

    IAsyncEnumerable<Message> IMessageRepository.GetMessagesAsync(DateTime startTime, DateTime endTime)
    {
        throw new NotImplementedException();
    }
}