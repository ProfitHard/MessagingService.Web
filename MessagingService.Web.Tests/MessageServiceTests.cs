using Xunit;
using MessagingService.Web.Services;
using MessagingService.Web.Data;
using MessagingService.Web.Models;
using Microsoft.Extensions.Logging;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MessagingService.Web.Tests;

public class MessageServiceTests
{
    [Fact]
    public async Task GetMessagesAsync_ReturnsMessagesWithinTimeRange()
    {
        // Arrange
        var mockRepository = new Mock<IMessageRepository>();
        var logger = Mock.Of<ILogger<MessageService>>();

        // Создаем список тестовых сообщений
        var messages = new List<Message>
        {
            new() { Id = 1, Text = "Message 1", Timestamp = DateTime.Now.AddDays(-1) },
            new() { Id = 2, Text = "Message 2", Timestamp = DateTime.Now },
            new() { Id = 3, Text = "Message 3", Timestamp = DateTime.Now.AddDays(1) }
        }.ToAsyncEnumerable();

        mockRepository.Setup(repo => repo.GetMessagesAsync(It.IsAny<DateTime>(), It.IsAny<DateTime>()))
            .Returns(messages);

        var messageService = new MessageService(mockRepository.Object, logger);

        // Act
        var startTime = DateTime.Now.AddDays(-2);
        var endTime = DateTime.Now.AddDays(2);
        var result = await messageService.GetMessagesAsync(startTime, endTime).ToListAsync();

        // Assert
        Assert.Equal(3, result.Count); // Проверяем, что возвращено 3 сообщения
        Assert.Contains(result, m => m.Id == 1); // Проверяем, что Id == 1 есть в результатах
        Assert.Contains(result, m => m.Id == 2); // Проверяем, что Id == 2 есть в результатах
        Assert.Contains(result, m => m.Id == 3); // Проверяем, что Id == 3 есть в результатах
    }

    [Fact]
    public async Task AddMessageAsync_CallsRepositoryAddMessageAsync()
    {
        // Arrange
        var mockRepository = new Mock<IMessageRepository>();
        var logger = Mock.Of<ILogger<MessageService>>();
        var messageService = new MessageService(mockRepository.Object, logger);

        var message = new Message { Id = 0, Text = "Test Message", Timestamp = DateTime.Now }; // Id = 0 для нового сообщения

        // Act
        await messageService.AddMessageAsync(message);

        // Assert
        mockRepository.Verify(repo => repo.AddMessageAsync(It.Is<Message>(m => m.Text == "Test Message")), Times.Once); // Проверяем, что AddMessageAsync был вызван один раз с правильным сообщением
    }
}