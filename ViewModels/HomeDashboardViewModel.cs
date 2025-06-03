namespace ProductionManagementSystem.ViewModels
{
    public class HomeDashboardViewModel
    {
        public int ActiveOrders { get; set; }
        public int PendingOrders { get; set; }
        public int LowStockMaterials { get; set; }
        public int ActiveLines { get; set; }
        public int TotalProducts { get; set; }
        public List<WorkOrderViewModel> RecentWorkOrders { get; set; } = new List<WorkOrderViewModel>();
    }
}