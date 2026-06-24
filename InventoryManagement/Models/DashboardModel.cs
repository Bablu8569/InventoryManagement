using InventoryManagement.Helpers;
using Microsoft.Data.SqlClient;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;

namespace InventoryManagement.Models
{
    public class DashboardModel
    {
        // ================= TOP STAT CARDS =================
        public int TotalCategories { get; set; }
        public int TotalProducts { get; set; }
        public int LowStockCount { get; set; }
        public int TodayTransactions { get; set; }

        // ================= CHART 1: CATEGORY WISE =================
        public List<string> CategoryNames { get; set; } = new();
        public List<int> CategoryProductCounts { get; set; } = new();

        // ================= CHART 2: PRODUCT PRICE / STOCK =================
        public List<string> ProductNames { get; set; } = new();
        public List<decimal> ProductPrices { get; set; } = new();
        public List<int> ProductQuantities { get; set; } = new();

        // ================= CHART 3: LOW STOCK =================
        public List<string> LowStockProductNames { get; set; } = new();
        public List<int> LowStockQuantities { get; set; } = new();

        // ================= CHART 4: TRANSACTIONS =================
        public List<string> TransactionProductNames { get; set; } = new();
        public List<int> TransactionQuantities { get; set; } = new();

        // ========== LOAD DASHBOARD DATA ==========
        public static DashboardModel LoadDashboardData(DatabaseHelper db)
        {
            var model = new DashboardModel();

            // ================= TOP CARDS =================
            model.TotalCategories = Convert.ToInt32(db.ExecuteScalar("sp_GetTotalCategories") ?? 0);
            model.TotalProducts = Convert.ToInt32(db.ExecuteScalar("sp_GetTotalProducts") ?? 0);
            model.LowStockCount = Convert.ToInt32(db.ExecuteScalar("sp_GetLowStockCount") ?? 0);
            model.TodayTransactions = Convert.ToInt32(db.ExecuteScalar("sp_GetTodayTransactionsCount") ?? 0);

            // ================= CATEGORY CHART =================
            DataTable dtCategory = db.ExecuteStoredProcedure("sp_GetCategoryProductCounts");

            foreach (DataRow row in dtCategory.Rows)
            {
                model.CategoryNames.Add(row["CategoryName"]?.ToString() ?? "");
                model.CategoryProductCounts.Add(Convert.ToInt32(row["ProductCount"]));
            }

            // ================= PRODUCT CHART =================
            DataTable dtProducts = db.ExecuteStoredProcedure("sp_GetDashboardProducts");

            foreach (DataRow row in dtProducts.Rows)
            {
                model.ProductNames.Add(row["ProductName"]?.ToString() ?? "");
                model.ProductPrices.Add(Convert.ToDecimal(row["Price"]));
                model.ProductQuantities.Add(Convert.ToInt32(row["Quantity"]));
            }

            // ================= LOW STOCK CHART =================
            DataTable dtLowStock = db.ExecuteStoredProcedure("sp_GetLowStockProducts");

            foreach (DataRow row in dtLowStock.Rows)
            {
                model.LowStockProductNames.Add(row["ProductName"]?.ToString() ?? "");
                model.LowStockQuantities.Add(Convert.ToInt32(row["Quantity"]));
            }

            // ================= TRANSACTIONS CHART =================
            DataTable dtTrans = db.ExecuteStoredProcedure("sp_GetTodayTransactionsSummary");

            foreach (DataRow row in dtTrans.Rows)
            {
                model.TransactionProductNames.Add(row["ProductName"]?.ToString() ?? "");
                model.TransactionQuantities.Add(Convert.ToInt32(row["TotalMoved"]));
            }

            return model;
        }
    }
}