using Microsoft.AspNetCore.Mvc;
using MessagingService.Web.Models;
using MessagingService.Web.Services;
using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using System.Text.Json;
using Microsoft.AspNetCore.Http;
using Asp.Versioning;
using System.Collections.Generic;

namespace MessagingService.Web.Controllers;

[ApiController]
[ApiVersion("1.0")] // Version 1.0
[Route("api/[controller]")]
public class MessageController(MessageService messageService, ILogger<MessageController> logger) : ControllerBase
{
    private readonly MessageService _messageService = messageService;
    private readonly ILogger<MessageController> _logger = logger;

    [HttpPost("send")]
    public async Task<IActionResult> SendMessage([FromBody] Message message) // Add [FromBody]
    {
        _logger.LogInformation("Received SendMessage request. Message: {Message}", JsonSerializer.Serialize(message)); // Логируем полученное сообщение

        if (!ModelState.IsValid)
        {
            _logger.LogWarning("Invalid model state. Errors: {ModelStateErrors}", ModelState.Values
                .SelectMany(v => v.Errors)
                .Select(e => e.ErrorMessage));
            return BadRequest(ModelState); // Return validation errors
        }

        try
        {
            await _messageService.AddMessageAsync(message);
            var jsonMessage = JsonSerializer.Serialize(message);
            await MWebSocketManager.BroadcastMessage(jsonMessage); // Use the correct class name

            _logger.LogInformation("Message sent successfully. Id: {MessageId}", message.Id);  // Логируем успешную отправку

            return Ok();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while sending message. Id: {MessageId}", message.Id); // Логируем ошибку
            return StatusCode(500, "Internal Server Error");
        }
    }

    [HttpGet("history")]
    public IAsyncEnumerable<Message> GetMessageHistory(DateTime startTime, DateTime endTime)
    {
        _logger.LogInformation("Received GetMessageHistory request. StartTime: {StartTime}, EndTime: {EndTime}", startTime, endTime);  // Логируем получение запроса истории

        return _messageService.GetMessagesAsync(startTime, endTime);
    }
}