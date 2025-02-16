using Microsoft.AspNetCore.Mvc;
using MessagingService.Web.Models;
using MessagingService.Web.Services;
using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

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
        if (string.IsNullOrEmpty(message.Text) || message.Text.Length > 128)
        {
            return BadRequest("Message text must be between 1 and 128 characters.");
        }

        try
        {
            await _messageService.AddMessageAsync(message);
            return Ok();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error sending message: {ErrorMessage}", ex.Message);
            return StatusCode(500, "Internal server error");
        }
    }

    [HttpGet("history")]
    public async Task<IActionResult> GetMessageHistory(DateTime startTime, DateTime endTime)
    {
        try
        {
            var messages = await _messageService.GetMessagesAsync(startTime, endTime);
            return Ok(messages);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting message history: {ErrorMessage}", ex.Message);
            return StatusCode(500, "Internal server error");
        }
    }
}