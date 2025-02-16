using System.Net.WebSockets;
using System.Collections.Concurrent;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace MessagingService.Web;

public static class MWebSocketManager
{
    private static readonly ConcurrentDictionary<string, WebSocket> ConnectedClients = new();

    public static async Task BroadcastMessage(string message)
    {
        byte[] buffer = Encoding.UTF8.GetBytes(message);
        foreach (var (key, client) in ConnectedClients)
        {
            if (client.State == WebSocketState.Open)
            {
                try
                {
                    await client.SendAsync(new ArraySegment<byte>(buffer, 0, buffer.Length), WebSocketMessageType.Text, true, CancellationToken.None);
                }
                catch
                {
                    TryRemoveClient(key); // Remove the client if sending fails
                }
            }
        }
    }

    public static bool TryAddClient(string socketId, WebSocket webSocket)
    {
        return ConnectedClients.TryAdd(socketId, webSocket);
    }

    public static bool TryRemoveClient(string socketId)
    {
        return ConnectedClients.TryRemove(socketId, out _);
    }
}