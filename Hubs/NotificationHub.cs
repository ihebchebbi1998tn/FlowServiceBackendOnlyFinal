using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using System.Security.Claims;

namespace ConsolidatedApi.Hubs
{
    [Authorize]
    public class NotificationHub : Hub
    {
        public async Task JoinUserGroup(string userId)
        {
            await Groups.AddToGroupAsync(Context.ConnectionId, $"user_{userId}");
        }

        public async Task LeaveUserGroup(string userId)
        {
            await Groups.RemoveFromGroupAsync(Context.ConnectionId, $"user_{userId}");
        }

        public async Task JoinOrganizationGroup(string organizationId)
        {
            await Groups.AddToGroupAsync(Context.ConnectionId, $"org_{organizationId}");
        }

        public async Task LeaveOrganizationGroup(string organizationId)
        {
            await Groups.RemoveFromGroupAsync(Context.ConnectionId, $"org_{organizationId}");
        }

        public async Task SendNotificationToUser(string userId, object notification)
        {
            await Clients.Group($"user_{userId}").SendAsync("ReceiveNotification", notification);
        }

        public async Task SendNotificationToOrganization(string organizationId, object notification)
        {
            await Clients.Group($"org_{organizationId}").SendAsync("ReceiveNotification", notification);
        }

        public override async Task OnConnectedAsync()
        {
            var userId = Context.User?.FindFirstValue(ClaimTypes.NameIdentifier);
            if (!string.IsNullOrEmpty(userId))
            {
                await Groups.AddToGroupAsync(Context.ConnectionId, $"user_{userId}");
            }
            await base.OnConnectedAsync();
        }

        public override async Task OnDisconnectedAsync(Exception? exception)
        {
            var userId = Context.User?.FindFirstValue(ClaimTypes.NameIdentifier);
            if (!string.IsNullOrEmpty(userId))
            {
                await Groups.RemoveFromGroupAsync(Context.ConnectionId, $"user_{userId}");
            }
            await base.OnDisconnectedAsync(exception);
        }
    }
}