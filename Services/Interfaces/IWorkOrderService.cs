using ProductionManagementSystem.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ProductionManagementSystem.Services
{
    public interface IWorkOrderService
    {
        Task<List<WorkOrder>> GetWorkOrdersAsync(string? status, string? date);
        Task<(WorkOrder? workOrder, Dictionary<int, decimal>? missingMaterials)> CreateWorkOrderAsync(WorkOrder workOrder);
        Task UpdateWorkOrderProgressAsync(int id, decimal progress);
        Task<WorkOrder?> GetWorkOrderDetailsAsync(int id);
    }
}