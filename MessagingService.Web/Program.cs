using System.Net.WebSockets;
using MessagingService.Web.Data;
using MessagingService.Web.Services;
using FluentValidation.AspNetCore;  // Make sure this is included
using MessagingService.Web.Validators;
using Asp.Versioning;
using Microsoft.Extensions.Options;
using Swashbuckle.AspNetCore.SwaggerGen;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddSingleton<MessageService>();  // Make sure this persists!
builder.Services.AddLogging(logging => logging.AddConsole());
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection")!;
builder.Services.AddTransient<IMessageRepository>(provider =>
{
    var logger = provider.GetRequiredService<ILogger<MessageRepository>>();
    var configuration = provider.GetRequiredService<IConfiguration>();
    return new MessageRepository(configuration, logger);
});
builder.Services.AddScoped<MessageService>();
builder.Services.AddControllers()
    .AddFluentValidation(fv =>
    {
        fv.RegisterValidatorsFromAssemblyContaining<MessageValidator>();
    });

builder.Services.AddScoped<MessageService>();

builder.Services.AddApiVersioning(options =>
{
    options.DefaultApiVersion = new ApiVersion(1.0);
    options.AssumeDefaultVersionWhenUnspecified = true;
    options.ReportApiVersions = true;
    options.ApiVersionReader = new UrlSegmentApiVersionReader(); // Версионирование через сегмент URL
    //options.ApiVersionReader = new HeaderApiVersionReader("X-Api-Version"); // Версионирование через заголовок
})
.AddApiExplorer(options =>
{
    options.GroupNameFormat = "'v'VVV";
    options.SubstituteApiVersionInUrl = true;
});

builder.Services.AddTransient<IConfigureOptions<SwaggerGenOptions>, ConfigureSwaggerOptions>();

builder.Services.AddSwaggerGen(options =>
{
    options.OperationFilter<AddApiVersionParameter>(); // adds the version parameter
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();
app.UseAuthorization();
// Enable WebSockets
app.UseWebSockets();

app.Use(async (context, next) =>
{
    if (context.Request.Path == "/ws")
    {
        if (context.WebSockets.IsWebSocketRequest)
        {
            WebSocket webSocket = await context.WebSockets.AcceptWebSocketAsync();
            var socketId = Guid.NewGuid().ToString();
            if (MessagingService.Web.MWebSocketManager.TryAddClient(socketId, webSocket))
            {
                var logger = app.Services.GetRequiredService<ILogger<Program>>();

                try
                {
                    await HandleWebSocket(context, webSocket, logger, socketId);
                }
                finally
                {
                    MessagingService.Web.MWebSocketManager.TryRemoveClient(socketId);
                }
            }
            else
            {
                // Handle the case where adding the client failed (e.g., too many clients)
                context.Response.StatusCode = StatusCodes.Status500InternalServerError;
                await context.Response.WriteAsync("Server is too busy to accept more connections.");
                await webSocket.CloseAsync(System.Net.WebSockets.WebSocketCloseStatus.PolicyViolation, "Server is busy", CancellationToken.None); 
            }
        }
        else
        {
            context.Response.StatusCode = StatusCodes.Status400BadRequest;
        }
    }
    else
    {
        await next();
    }
});

app.MapControllers();

app.Run();

static async Task HandleWebSocket(HttpContext context, WebSocket webSocket, ILogger logger, string socketId)
{
    var buffer = new byte[1024 * 4];
    try
    {
        WebSocketReceiveResult result = await webSocket.ReceiveAsync(new ArraySegment<byte>(buffer), CancellationToken.None);

        while (!result.CloseStatus.HasValue)
        {
            logger.LogInformation("Received message from client");
            result = await webSocket.ReceiveAsync(new ArraySegment<byte>(buffer), CancellationToken.None);
        }

        await webSocket.CloseAsync(result.CloseStatus.Value, result.CloseStatusDescription, CancellationToken.None);
    }
    catch (Exception ex)
    {
        logger.LogError(ex, "WebSocket error");
    }
    finally
    {
        MessagingService.Web.MWebSocketManager.TryRemoveClient(socketId);
        logger.LogInformation("Closed connection");
    }
}