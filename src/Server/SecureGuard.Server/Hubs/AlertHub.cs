using Microsoft.AspNetCore.SignalR;
using SecureGuard.Shared.DTOs;

namespace SecureGuard.Server.Hubs;

public class AlertHub : Hub
{
    public async Task JoinGroup(string groupName)
    {
        await Groups.AddToGroupAsync(Context.ConnectionId, groupName);
    }

    public async Task LeaveGroup(string groupName)
    {
        await Groups.RemoveFromGroupAsync(Context.ConnectionId, groupName);
    }

    public async Task SendAlert(AlertDto alert)
    {
        await Clients.All.SendAsync("ReceiveAlert", alert);
    }

    public async Task SendAgentStatus(string agentId, bool isOnline)
    {
        await Clients.All.SendAsync("AgentStatusChanged", agentId, isOnline);
    }

    public override async Task OnConnectedAsync()
    {
        await Groups.AddToGroupAsync(Context.ConnectionId, "dashboard");
        await base.OnConnectedAsync();
    }
}
