using System;

namespace InventoryManagement.Models
{
    public class UserModel
    {
        public int UserId { get; set; }
        public string Username { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
        public string ConfirmPassword { get; set; } = string.Empty;
        public string Role { get; set; } = "2";
        public bool IsActive { get; set; } = true;
        public DateTime? CreatedDate { get; set; }
    }
}