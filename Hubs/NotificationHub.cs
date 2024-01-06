using Microsoft.AspNetCore.SignalR;
using ESD_EDI_BE.Services.Common;

namespace ESD_EDI_BE.Hubs
{
    public class NotificationHub : Hub
    {
        private readonly string _botUser;

        public NotificationHub()
        {
            _botUser = "Chat Bot";
        }
        public async Task JoinRoom(NotificationService notificationService)
        {
            await Groups.AddToGroupAsync(Context.ConnectionId, notificationService.UserId.ToString());
            await Clients.Groups(notificationService.UserId.ToString()).SendAsync("ReceiveMessage", _botUser, $"");
        }
    }
}
