//using System.ComponentModel.DataAnnotations;

//namespace InventoryManagement.Models
//{
//    public class StockEntryModel
//    {
//        [Required(ErrorMessage = "Please select a product")]

//        public string ProductName { get; set; } = "";
//        public int ProductId { get; set; }

//        [Required(ErrorMessage = "Quantity is required")]
//        [Range(1, int.MaxValue, ErrorMessage = "Quantity must be at least 1")]
//        public int Quantity { get; set; }

//        [Required(ErrorMessage = "Please select transaction type")]
//        public string TransactionType { get; set; } = "";

//        public string Remarks { get; set; } = "";   // ✅ Added if needed
//    }
//}


using System.ComponentModel.DataAnnotations;

namespace InventoryManagement.Models
{
    public class StockEntryModel
    {
        [Required(ErrorMessage = "Please select a product")]
        public int ProductId { get; set; }

        public string ProductName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Quantity is required")]
        [Range(1, 1000, ErrorMessage = "Quantity must be between 1 and 1000")]
        public int Quantity { get; set; }

        [Required(ErrorMessage = "Please select transaction type")]
        public string TransactionType { get; set; } = string.Empty;

        // Optional
        [StringLength(500, ErrorMessage = "Remarks cannot exceed 500 characters.")]
        public string? Remarks { get; set; }
    }
}