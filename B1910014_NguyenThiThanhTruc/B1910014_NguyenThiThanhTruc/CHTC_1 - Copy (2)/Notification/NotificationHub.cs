using Microsoft.AspNetCore.SignalR;
namespace CHTC_1.Notification
{
    public class NotificationHub : Hub
    {
        public async Task SendMessage(int user,string message)
        {
            await Clients.All.SendAsync("SendMessage", user ,message);
        }

        public async Task SendNotification(string user,string message)
        {
            await Clients.All.SendAsync("SendNotification", user,message);
        }
    }
}
