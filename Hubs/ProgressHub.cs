using Microsoft.AspNetCore.SignalR;

namespace ProductionManagementSystem.Hubs
{
    public class ProgressHub : Hub
    {
        public async Task UpdateProgress(int orderId, int progress, string status, int minutesCompleted, int totalMinutes)
        {
            await Clients.All.SendAsync(
                "ReceiveProgressUpdate", 
                orderId, 
                progress, 
                status,
                minutesCompleted,
                totalMinutes);
        }
        
    }
}