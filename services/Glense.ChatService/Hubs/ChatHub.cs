using Microsoft.AspNetCore.SignalR;

namespace Glense.ChatService.Hubs
{
    public class ChatHub : Hub
    {
        // Simple server methods for demo purposes
        public async Task SendMessageToChat(string chatId, string user, string message)
        {
            // broadcast to a group representing the chat
            await Clients.Group(chatId).SendAsync("ReceiveMessage", chatId, user, message);
        }

        public Task JoinChat(string chatId)
        {
            return Groups.AddToGroupAsync(Context.ConnectionId, chatId);
        }

        public Task LeaveChat(string chatId)
        {
            return Groups.RemoveFromGroupAsync(Context.ConnectionId, chatId);
        }
    }
}
