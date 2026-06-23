using System;

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
    }
}