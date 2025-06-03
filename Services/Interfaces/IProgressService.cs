using System;
using System.Threading.Tasks;

namespace ProductionManagementSystem.Services
{
    public interface IProgressService
    {
        Task CheckAndUpdateWorkOrdersAsync();
    }
}