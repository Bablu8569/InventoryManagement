using InventoryManagement.Helpers;
using InventoryManagement.Models;
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
            _dbHelper = new DatabaseHelper(configuration);
        }

        // Get transactions with optional date filter
        public List<StockTransactionModel> GetStockTransactions(
            DateTime? fromDate,
            DateTime? toDate
        )
        {
            List<StockTransactionModel> transactions =
                new List<StockTransactionModel>();

            Hashtable ht =
                new Hashtable();

            ht.Add(
                "@FromDate",

                fromDate.HasValue

                ? fromDate.Value

                : DBNull.Value
            );

            ht.Add(
                "@ToDate",

                toDate.HasValue

                ? toDate.Value

                : DBNull.Value
            );

            DataTable dt =
                _dbHelper.ExecuteStoredProcedure(
                    "USP_GetStockTransactions",
                    ht
                );

            foreach (DataRow row in dt.Rows)
            {
                transactions.Add(
                    new StockTransactionModel
                    {
                        TransactionId =
                            Convert.ToInt32(
                            row["TransactionId"]
                            ),

                        ProductId =
                            Convert.ToInt32(
                            row["ProductId"]
                            ),

                        Quantity =
                            Convert.ToInt32(
                            row["Quantity"]
                            ),

                        TransactionType =
                            row["TransactionType"]
                            .ToString()
                            ?? "",

                        TransactionDate =
                            Convert.ToDateTime(
                            row["TransactionDate"]
                            ),

                        ProductName =
                            row["ProductName"]
                            .ToString()
                            ?? "",

                        Remarks =
                            row.Table.Columns.Contains("Remarks")

                            ? row["Remarks"]
                            .ToString()
                            ?? ""

                            : ""
                    }
                );
            }

            return transactions;
        }

        // Insert Transaction
        public (bool Success, string Message)
        InsertStockTransaction(
            StockTransactionModel model
        )
        {
            Hashtable ht =
                new Hashtable();

            ht.Add(
                "@ProductId",
                model.ProductId
            );

            ht.Add(
                "@TransactionType",
                model.TransactionType
            );

            ht.Add(
                "@Quantity",
                model.Quantity
            );

            ht.Add(
                "@Remarks",

                string.IsNullOrEmpty(
                model.Remarks
                )

                ? DBNull.Value

                : model.Remarks
            );

            DataTable dt =
                _dbHelper.ExecuteStoredProcedure(
                    "USP_InsertStockTransaction",
                    ht
                );

            if (dt.Rows.Count > 0)
            {
                int result =
                    Convert.ToInt32(
                    dt.Rows[0]["Result"]
                    );

                string message =
                    dt.Rows[0]["Message"]
                    .ToString()
                    ?? "Unknown";

                return (
                    result == 1,
                    message
                );
            }

            return (
                false,
                "Unknown error occurred"
            );
        }

        // Product wise transactions
        public List<StockTransactionModel>
        GetStockTransactionsByProduct(
            int productId
        )
        {
            List<StockTransactionModel>
            transactions =
            new List<StockTransactionModel>();

            Hashtable ht =
            new Hashtable();

            ht.Add(
            "@ProductId",
            productId
            );

            DataTable dt =
            _dbHelper.ExecuteStoredProcedure(
            "USP_GetStockTransactionsByProduct",
            ht
            );

            foreach (DataRow row in dt.Rows)
            {
                transactions.Add(
                new StockTransactionModel
                {
                    TransactionId =
                    Convert.ToInt32(
                    row["TransactionId"]
                    ),

                    ProductId =
                    Convert.ToInt32(
                    row["ProductId"]
                    ),

                    Quantity =
                    Convert.ToInt32(
                    row["Quantity"]
                    ),

                    TransactionType =
                    row["TransactionType"]
                    .ToString()
                    ?? "",

                    TransactionDate =
                    Convert.ToDateTime(
                    row["TransactionDate"]
                    ),

                    ProductName =
                    row["ProductName"]
                    .ToString()
                    ?? "",

                    Remarks =
                    row.Table.Columns.Contains(
                    "Remarks"
                    )

                    ? row["Remarks"]
                    .ToString()
                    ?? ""

                    : ""
                });
            }

            return transactions;
        }

        // Dashboard Stats
        public DashboardModel
        GetDashboardStats()
        {
            DashboardModel stats =
                new DashboardModel();

            DataTable dt =
                _dbHelper.ExecuteStoredProcedure(
                "USP_GetDashboardStats"
                );

            if (dt.Rows.Count > 0)
            {
                DataRow row =
                    dt.Rows[0];

                stats.TotalCategories =
                    Convert.ToInt32(
                    row["TotalCategories"]
                    );

                stats.TotalProducts =
                    Convert.ToInt32(
                    row["TotalProducts"]
                    );

                stats.LowStockCount =
                    Convert.ToInt32(
                    row["LowStockCount"]
                    );

                stats.TodayTransactions =
                    Convert.ToInt32(
                    row["TodayTransactions"]
                    );
            }

            return stats;
        }

        public bool AddTransaction(
            StockEntryModel model
        )
        {
            var result =
            InsertStockTransaction(
            new StockTransactionModel
            {
                ProductId =
                model.ProductId,

                Quantity =
                model.Quantity,

                TransactionType =
                model.TransactionType,

                TransactionDate =
                DateTime.Now,

                Remarks =
                model.Remarks
            });

            return result.Success;
        }
    }
}