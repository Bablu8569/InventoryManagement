namespace InventoryManagement.Models
{
    public class DashboardStatsModel
    {
        public int TotalCategories { get; set; }
        public int TotalProducts { get; set; }
        public int LowStockCount { get; set; }
        public int TodayTransactions { get; set; }
    }
}