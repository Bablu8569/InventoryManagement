
namespace InventoryManagement.Models
    {
        public class DashboardModel
        {
            // Top stat cards
            public int TotalCategories { get; set; }
            public int TotalProducts { get; set; }
            public int LowStockCount { get; set; }
            public int TodayTransactions { get; set; }

            // Chart 1: Categories -> Product Count
            public List<string> CategoryNames { get; set; } = new();
            public List<int> CategoryProductCounts { get; set; } = new();

            // Chart 2: Products -> Price Comparison (specific products)
            public List<string> ProductNames { get; set; } = new();
            public List<decimal> ProductPrices { get; set; } = new();

            // Chart 3: Low Stock Items (product name + quantity)
            public List<string> LowStockProductNames { get; set; } = new();
            public List<int> LowStockQuantities { get; set; } = new();

            // Chart 4: Today's Transactions (product name + total quantity moved)
            public List<string> TransactionProductNames { get; set; } = new();
            public List<int> TransactionQuantities { get; set; } = new();
            public List<int> ProductQuantities { get; set; } = new();
    }
    }
    