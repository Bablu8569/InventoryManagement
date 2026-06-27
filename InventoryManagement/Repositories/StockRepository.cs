using InventoryManagement.Helpers;
using InventoryManagement.Models;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;

namespace InventoryManagement.Repositories
{
    public class StockRepository : IStockRepository
    {
        private readonly DatabaseHelper _dbHelper;

        public StockRepository(IConfiguration configuration)
        {
            try
            {
                _dbHelper = new DatabaseHelper(configuration);
            }
            catch (Exception ex)
            {
                throw new Exception("Failed to initialize StockRepository: " + ex.Message, ex);
            }
        }

        // ========== GET STOCK TRANSACTIONS ==========
        public List<StockTransactionModel> GetStockTransactions(
            DateTime? fromDate,
            DateTime? toDate
        )
        {
            var transactions = new List<StockTransactionModel>();

            try
            {
                var ht = new Hashtable();

                ht.Add("@FromDate", fromDate.HasValue ? (object)fromDate.Value : DBNull.Value);
                ht.Add("@ToDate", toDate.HasValue ? (object)toDate.Value : DBNull.Value);

                DataTable dt = _dbHelper.ExecuteStoredProcedure("USP_GetStockTransactions", ht);

                if (dt == null || dt.Rows.Count == 0)
                {
                    return transactions;
                }

                foreach (DataRow row in dt.Rows)
                {
                    try
                    {
                        transactions.Add(new StockTransactionModel
                        {
                            TransactionId = Convert.ToInt32(row["TransactionId"]),
                            ProductId = Convert.ToInt32(row["ProductId"]),
                            Quantity = Convert.ToInt32(row["Quantity"]),
                            TransactionType = row["TransactionType"]?.ToString() ?? "",
                            TransactionDate = Convert.ToDateTime(row["TransactionDate"]),
                            ProductName = row["ProductName"]?.ToString() ?? "",
                            Remarks = row.Table.Columns.Contains("Remarks") ? row["Remarks"]?.ToString() ?? "" : ""
                        });
                    }
                    catch (Exception ex)
                    {
                        // Log individual row error but continue processing
                        Console.WriteLine($"Error processing transaction row: {ex.Message}");
                    }
                }
            }
            catch (SqlException ex)
            {
                throw new Exception("Database error while fetching stock transactions: " + ex.Message, ex);
            }
            catch (Exception ex)
            {
                throw new Exception("Error fetching stock transactions: " + ex.Message, ex);
            }

            return transactions;
        }

        // ========== INSERT STOCK TRANSACTION ==========
        public (bool Success, string Message) InsertStockTransaction(
            StockTransactionModel model
        )
        {
            try
            {
                if (model == null)
                {
                    return (false, "Transaction model cannot be null.");
                }

                var ht = new Hashtable();

                ht.Add("@ProductId", model.ProductId);
                ht.Add("@TransactionType", model.TransactionType);
                ht.Add("@Quantity", model.Quantity);
                ht.Add("@Remarks", string.IsNullOrEmpty(model.Remarks) ? DBNull.Value : (object)model.Remarks);

                DataTable dt = _dbHelper.ExecuteStoredProcedure("USP_InsertStockTransaction", ht);

                if (dt != null && dt.Rows.Count > 0)
                {
                    try
                    {
                        int result = Convert.ToInt32(dt.Rows[0]["Result"]);
                        string message = dt.Rows[0]["Message"]?.ToString() ?? "Unknown";

                        return (result == 1, message);
                    }
                    catch (Exception ex)
                    {
                        return (false, "Error reading result: " + ex.Message);
                    }
                }

                return (false, "No response from database.");
            }
            catch (SqlException ex)
            {
                return (false, "Database error: " + ex.Message);
            }
            catch (Exception ex)
            {
                return (false, "Error: " + ex.Message);
            }
        }

        // ========== GET STOCK TRANSACTIONS BY PRODUCT ==========
        public List<StockTransactionModel> GetStockTransactionsByProduct(
            int productId
        )
        {
            var transactions = new List<StockTransactionModel>();

            try
            {
                if (productId <= 0)
                {
                    return transactions;
                }

                var ht = new Hashtable();
                ht.Add("@ProductId", productId);

                DataTable dt = _dbHelper.ExecuteStoredProcedure("USP_GetStockTransactionsByProduct", ht);

                if (dt == null || dt.Rows.Count == 0)
                {
                    return transactions;
                }

                foreach (DataRow row in dt.Rows)
                {
                    try
                    {
                        transactions.Add(new StockTransactionModel
                        {
                            TransactionId = Convert.ToInt32(row["TransactionId"]),
                            ProductId = Convert.ToInt32(row["ProductId"]),
                            Quantity = Convert.ToInt32(row["Quantity"]),
                            TransactionType = row["TransactionType"]?.ToString() ?? "",
                            TransactionDate = Convert.ToDateTime(row["TransactionDate"]),
                            ProductName = row["ProductName"]?.ToString() ?? "",
                            Remarks = row.Table.Columns.Contains("Remarks") ? row["Remarks"]?.ToString() ?? "" : ""
                        });
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"Error processing transaction row: {ex.Message}");
                    }
                }
            }
            catch (SqlException ex)
            {
                throw new Exception("Database error while fetching product transactions: " + ex.Message, ex);
            }
            catch (Exception ex)
            {
                throw new Exception("Error fetching product transactions: " + ex.Message, ex);
            }

            return transactions;
        }

        // ========== GET DASHBOARD STATS ==========
        public DashboardModel GetDashboardStats()
        {
            var stats = new DashboardModel();

            try
            {
                DataTable dt = _dbHelper.ExecuteStoredProcedure("USP_GetDashboardStats");

                if (dt != null && dt.Rows.Count > 0)
                {
                    DataRow row = dt.Rows[0];

                    try
                    {
                        stats.TotalCategories = Convert.ToInt32(row["TotalCategories"]);
                        stats.TotalProducts = Convert.ToInt32(row["TotalProducts"]);
                        stats.LowStockCount = Convert.ToInt32(row["LowStockCount"]);
                        stats.TodayTransactions = Convert.ToInt32(row["TodayTransactions"]);
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"Error converting dashboard stats: {ex.Message}");
                    }
                }
            }
            catch (SqlException ex)
            {
                throw new Exception("Database error while fetching dashboard stats: " + ex.Message, ex);
            }
            catch (Exception ex)
            {
                throw new Exception("Error fetching dashboard stats: " + ex.Message, ex);
            }

            return stats;
        }

        // ========== ADD TRANSACTION (HELPER) ==========
        public bool AddTransaction(StockEntryModel model)
        {
            try
            {
                if (model == null)
                {
                    return false;
                }

                var result = InsertStockTransaction(new StockTransactionModel
                {
                    ProductId = model.ProductId,
                    Quantity = model.Quantity,
                    TransactionType = model.TransactionType,
                    TransactionDate = DateTime.Now,
                    Remarks = model.Remarks
                });

                return result.Success;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in AddTransaction: {ex.Message}");
                return false;
            }
        }
    }
}