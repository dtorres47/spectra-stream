using Microsoft.AspNetCore.SignalR;
using System.Threading.Tasks;

namespace SpectraStream.Api.Hubs
{
    public class OverlayHub : Hub
    {
        // Called when a new client connects
        public override async Task OnConnectedAsync()
        {
            // You could log or track connections here
            await base.OnConnectedAsync();
        }

        // Called when a client disconnects
        public override async Task OnDisconnectedAsync(Exception? exception)
        {
            // Clean up connection if needed
            await base.OnDisconnectedAsync(exception);
        }

        // Test: broadcast a message to all overlay clients
        public async Task SendEvent(string type, object payload)
        {
            await Clients.All.SendAsync("ReceiveEvent", new
            {
                Type = type,
                Payload = payload
            });
        }
    }
}
