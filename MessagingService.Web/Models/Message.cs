namespace MessagingService.Web.Models;

public class Message
{
    public int Id { get; set; }
    public string Text { get; set; } = default!; // Require non-null
    public DateTime Timestamp { get; set; }
    public int ClientOrder { get; set; }
}