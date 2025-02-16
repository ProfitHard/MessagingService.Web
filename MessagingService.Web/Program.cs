using MessagingService.Web.Data;
using MessagingService.Web.Services;
using Microsoft.OpenApi.Models; // For Swagger

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers(); // For MVC

// Configure Logging
builder.Services.AddLogging(loggingBuilder =>
{
    loggingBuilder.AddConsole();
    // Add other logging providers as needed (e.g., file, Seq, etc.)
});

// Configure PostgreSQL (Replace with your connection string)
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection")!;

// Database Context and Repositories
builder.Services.AddTransient<IMessageRepository>(provider =>
{
    var logger = provider.GetRequiredService<ILogger<MessageRepository>>();
    var configuration = provider.GetRequiredService<IConfiguration>();
    return new MessageRepository(configuration, logger);
});

builder.Services.AddScoped<MessageService>();

// Configure Swagger
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "MessagingService API", Version = "v1" });
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles(); // Needed if serving client pages.

app.UseRouting();

app.UseAuthorization();

app.UseSwagger();
app.UseSwaggerUI(c =>
{
    c.SwaggerEndpoint("/swagger/v1/swagger.json", "MessagingService API V1");
});

app.MapControllers();

app.Run();