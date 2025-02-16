using Microsoft.AspNetCore.Mvc;
using MessagingService.Web.Models;
using MessagingService.Web.Services;
using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using System.Text.Json;
using Microsoft.AspNetCore.Http;
using Asp.Versioning;

namespace MessagingService.Web.Controllers;

[ApiController]
[ApiVersion("1.0")] // Version 1.0
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
    public async Task<IActionResult> SendMessage([FromBody] Message message) // Add [FromBody]
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState); // Return validation errors
        }

        await _messageService.AddMessageAsync(message);
        var jsonMessage = JsonSerializer.Serialize(message);
        await MWebSocketManager.BroadcastMessage(jsonMessage); // Use the correct class name

        return Ok();
    }

    [HttpGet("history")]
    public async Task<IActionResult> GetMessageHistory(DateTime startTime, DateTime endTime)
    {
        var messages = await _messageService.GetMessagesAsync(startTime, endTime);
        return Ok(messages);
    }
}