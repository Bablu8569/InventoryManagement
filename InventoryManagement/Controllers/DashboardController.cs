using InventoryManagement.Helpers;
using InventoryManagement.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;

namespace InventoryManagement.Controllers
{
    public class DashboardController : Controller
    {
        private readonly DatabaseHelper _db;

        public DashboardController(IConfiguration configuration)
        {
            _db = new DatabaseHelper(configuration);
        }

        private bool IsUserLoggedIn()
            => !string.IsNullOrEmpty(HttpContext.Session.GetString("Username"));

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Index()
        {
            if (!IsUserLoggedIn())
                return RedirectToAction("Login", "Account");

            // ✅ Model se data load karo – Controller me koi DB code nahi
            var model = DashboardModel.LoadDashboardData(_db);

            return View(model);
        }
    }
}