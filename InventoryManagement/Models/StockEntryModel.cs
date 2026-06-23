using System.ComponentModel.DataAnnotations;

namespace InventoryManagement.Models
{
    public class StockEntryModel
    {
        [Required(ErrorMessage = "Please select a product")]
        public int ProductId { get; set; }

        [Required(ErrorMessage = "Quantity is required")]
        [Range(1, int.MaxValue, ErrorMessage = "Quantity must be at least 1")]
        public int Quantity { get; set; }

        [Required(ErrorMessage = "Please select transaction type")]
        public string TransactionType { get; set; } = "";

        public string Remarks { get; set; } = "";   // ✅ Added if needed
    }
}