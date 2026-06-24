using System.Collections.Generic;
using InventoryManagement.Models;

namespace InventoryManagement.Repositories
{
    public interface IStockRepository
    {
        List<StockTransactionModel> GetStockTransactions(
          DateTime? fromDate,
          DateTime? toDate
      );
        (bool Success, string Message) InsertStockTransaction(StockTransactionModel transaction);
        List<StockTransactionModel> GetStockTransactionsByProduct(int productId);
        DashboardModel GetDashboardStats();
    }
}