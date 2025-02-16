using Microsoft.AspNetCore.Mvc;
using MessagingService.Web.Models;
using MessagingService.Web.Services;
using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using System.Text.Json;

namespace MessagingService.Web.Controllers;

[ApiController]
[Route("api/[controller]")]
public class MessageController : ControllerBase
{
    private readonly MessageService _messageService;
    private readonly ILogger<MessageController> _logger;

    public MessageController(MessageService messageService, ILogger<MessageController> logger)
    {
        _messageService = messageService;
        _logger = logger;
    }

    [HttpPost("send")]
    public async Task<IActionResult> SendMessage(Message message)
    {
        if (message is null)
        {
            return BadRequest("Message cannot be null.");
        }

        await _messageService.AddMessageAsync(message);
        var jsonMessage = JsonSerializer.Serialize(message);
        await MessagingService.Web.MWebSocketManager.BroadcastMessage(jsonMessage);

        return Ok();
    }

    [HttpGet("history")]
    public async Task<IActionResult> GetMessageHistory(DateTime startTime, DateTime endTime)
    {
        var messages = await _messageService.GetMessagesAsync(startTime, endTime);
        return Ok(messages);
    }
}