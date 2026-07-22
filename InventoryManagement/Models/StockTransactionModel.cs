using InventoryManagement.Helpers;
using Microsoft.Data.SqlClient;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace InventoryManagement.Models
{
    public class StockTransactionModel
    {


        public int TransactionId { get; set; }
        public int ProductId { get; set; }
        public string ProductName { get; set; } = "";
        public int Quantity { get; set; }
        public string TransactionType { get; set; } = ""; // "In" or "Out"
        public DateTime TransactionDate { get; set; }
        public string Remarks { get; set; } = "";         // ✅ Added to fix CS1061
        public string ReferenceNo { get; set; } = "";     // optional, if needed
        public int UserId { get; set; }                   // optional



        public static List<StockTransactionModel> GetTransactionsByProductId(DatabaseHelper db, int productId)
        {
            List<StockTransactionModel> transactions = new List<StockTransactionModel>();

            Hashtable ht = new Hashtable();
            ht.Add("@ProductId", productId);

            DataTable dt = db.ExecuteStoredProcedure("USP_GetStockTransactionsByProduct", ht);

            foreach (DataRow row in dt.Rows)
            {
                transactions.Add(new StockTransactionModel
                {
                    TransactionId = Convert.ToInt32(row["TransactionId"]),
                    ProductId = Convert.ToInt32(row["ProductId"]),
                    Quantity = Convert.ToInt32(row["Quantity"]),
                    TransactionType = row["TransactionType"]?.ToString() ?? "",
                    TransactionDate = Convert.ToDateTime(row["TransactionDate"]),
                    Remarks = row["Remarks"] == DBNull.Value ? "" : row["Remarks"].ToString()
                });
            }

            return transactions;
        }
    }
}