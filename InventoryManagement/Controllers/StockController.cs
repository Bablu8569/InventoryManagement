using InventoryManagement.Helpers;
using InventoryManagement.Models;
using InventoryManagement.Repositories;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Data;
using System.Text;

namespace InventoryManagement.Controllers
{
    public class StockController : Controller
    {
        private readonly IConfiguration _configuration;
        private readonly IStockRepository _stockRepository;

        public StockController(
            IConfiguration configuration,
            IStockRepository stockRepository)
        {
            _configuration = configuration;
            _stockRepository = stockRepository;
        }

        // ================= LOGIN CHECK =================

        private bool IsUserLoggedIn()
        {
            return !string.IsNullOrEmpty(HttpContext.Session.GetString("Username"));
        }

        // ================= ROLE CHECK =================

        private bool CanAddTransaction()
        {
            string? role = HttpContext.Session.GetString("Role");
            return role == "1" || role == "3";
        }

        // ================= TRANSACTION HISTORY =================

        //public IActionResult Index(DateTime? fromDate, DateTime? toDate)
        //{
        //    try
        //    {
        //        if (!IsUserLoggedIn())
        //            return RedirectToAction("Login", "Account");

        //        var transactions = _stockRepository.GetStockTransactions(fromDate, toDate);

        //        ViewBag.FromDate = fromDate?.ToString("yyyy-MM-dd");
        //        ViewBag.ToDate = toDate?.ToString("yyyy-MM-dd");

        //        return View(transactions);
        //    }
        //    catch (Exception ex)
        //    {
        //        TempData["Error"] = ex.Message;
        //        return RedirectToAction("Index", "Dashboard");
        //    }
        //}
        public IActionResult Index(DateTime? fromDate, DateTime? toDate, DateTime? date)
        {
            try
            {
                if (!IsUserLoggedIn())
                    return RedirectToAction("Login", "Account");


                // Dashboard Today's Transactions card se aane par
                if (date.HasValue)
                {
                    fromDate = date;
                    toDate = date;
                }


                var transactions = _stockRepository.GetStockTransactions(fromDate, toDate);


                ViewBag.FromDate = fromDate?.ToString("yyyy-MM-dd");
                ViewBag.ToDate = toDate?.ToString("yyyy-MM-dd");


                return View(transactions);
            }
            catch (Exception ex)
            {
                TempData["Error"] = ex.Message;
                return RedirectToAction("Index", "Dashboard");
            }
        }

        // ================= CREATE GET =================

        [HttpGet]
        public IActionResult Create()
        {
            if (!IsUserLoggedIn())
                return RedirectToAction("Login", "Account");

            if (!CanAddTransaction())
            {
                TempData["Error"] = "Access Denied.";
                return RedirectToAction("Index", "Dashboard");
            }

            ViewBag.Products = GetProductList();

            return View();
        }

        // ================= CREATE POST =================

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Create(StockEntryModel model)
        {
            try
            {
                if (!IsUserLoggedIn())
                    return RedirectToAction("Login", "Account");

                if (!CanAddTransaction())
                {
                    TempData["Error"] = "Access Denied.";
                    return RedirectToAction("Index", "Dashboard");
                }

                // Product Validation
                if (model.ProductId <= 0)
                {
                    ModelState.AddModelError("ProductId", "Please select a product.");
                }

                // Quantity Validation
                if (model.Quantity < 1 || model.Quantity > 1000)
                {
                    ModelState.AddModelError("Quantity",
                        "Quantity must be between 1 and 1000.");
                }

                // Transaction Type Validation
                if (string.IsNullOrWhiteSpace(model.TransactionType))
                {
                    ModelState.AddModelError("TransactionType",
                        "Please select transaction type.");
                }

                // Remarks Optional
                if (!string.IsNullOrWhiteSpace(model.Remarks) &&
                    model.Remarks.Length > 500)
                {
                    ModelState.AddModelError("Remarks",
                        "Remarks cannot exceed 500 characters.");
                }

                if (!ModelState.IsValid)
                {
                    ViewBag.Products = GetProductList();
                    return View(model);
                }

                string connStr = _configuration.GetConnectionString("DefaultConnection");

                using (SqlConnection conn = new SqlConnection(connStr))
                {
                    conn.Open();

                    // Stock Check for OUT

                    if (string.Equals(model.TransactionType, "Out",
                        StringComparison.OrdinalIgnoreCase))
                    {
                        using (SqlCommand cmd = new SqlCommand("USP_CheckStock", conn))
                        {
                            cmd.CommandType = CommandType.StoredProcedure;
                            cmd.Parameters.AddWithValue("@ProductId", model.ProductId);

                            int currentStock = Convert.ToInt32(cmd.ExecuteScalar());

                            if (currentStock < model.Quantity)
                            {
                                ModelState.AddModelError("",
                                    $"Insufficient Stock. Available : {currentStock}");

                                ViewBag.Products = GetProductList();
                                return View(model);
                            }
                        }
                    }

                    // Insert Transaction
                    using (SqlCommand cmd = new SqlCommand("USP_InsertStockTransaction", conn))
                    {
                        cmd.CommandType = CommandType.StoredProcedure;

                        cmd.Parameters.AddWithValue("@ProductId", model.ProductId);
                        cmd.Parameters.AddWithValue("@Quantity", model.Quantity);
                        cmd.Parameters.AddWithValue("@TransactionType", model.TransactionType);
                        cmd.Parameters.AddWithValue("@TransactionDate", DateTime.Now);

                        cmd.Parameters.AddWithValue("@Remarks",
                            string.IsNullOrWhiteSpace(model.Remarks)
                            ? DBNull.Value
                            : (object)model.Remarks);

                        cmd.ExecuteNonQuery();
                    }
                }

                TempData["Success"] = "Transaction Saved Successfully.";

                return RedirectToAction(nameof(Index));
            }
            catch (SqlException ex)
            {
                ModelState.AddModelError("", ex.Message);
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", ex.Message);
            }

            ViewBag.Products = GetProductList();
            return View(model);
        }
        // ================= GET PRODUCT LIST =================

        private List<ProductModel> GetProductList()
        {
            var products = new List<ProductModel>();

            try
            {
                string connStr = _configuration.GetConnectionString("DefaultConnection");

                using (SqlConnection conn = new SqlConnection(connStr))
                {
                    conn.Open();

                    using (SqlCommand cmd = new SqlCommand("USP_GetProductsForStock", conn))
                    {
                        cmd.CommandType = CommandType.StoredProcedure;

                        using (SqlDataReader reader = cmd.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                products.Add(new ProductModel
                                {
                                    ProductId = Convert.ToInt32(reader["ProductId"]),
                                    ProductName = reader["ProductName"]?.ToString() ?? "",
                                    Quantity = Convert.ToInt32(reader["Quantity"])
                                });
                            }
                        }
                    }
                }
            }
            catch (Exception)
            {
                // Optional: Log Error
            }

            return products;
        }

        // ================= EXPORT CSV =================

        [HttpGet]
        public IActionResult ExportCsv(DateTime? fromDate, DateTime? toDate)
        {
            try
            {
                if (!IsUserLoggedIn())
                    return RedirectToAction("Login", "Account");

                var transactions = _stockRepository.GetStockTransactions(fromDate, toDate);

                if (transactions == null || transactions.Count == 0)
                {
                    TempData["Error"] = "No transaction found.";
                    return RedirectToAction(nameof(Index));
                }

                string[] headers =
                {
                    "Transaction Id",
                    "Product",
                    "Quantity",
                    "Transaction Type",
                    "Transaction Date",
                    "Remarks"
                };

                string csv = CsvHelper.ConvertToCsv(
                    transactions,
                    headers,
                    item => new string[]
                    {
                        item.TransactionId.ToString(),
                        item.ProductName ?? "",
                        item.Quantity.ToString(),
                        item.TransactionType ?? "",
                        item.TransactionDate.ToString("yyyy-MM-dd HH:mm:ss"),
                        item.Remarks ?? ""
                    });

                byte[] bytes = Encoding.UTF8.GetBytes(csv);

                return File(
                    bytes,
                    "text/csv",
                    $"StockTransactions_{DateTime.Now:yyyyMMddHHmmss}.csv");
            }
            catch (Exception ex)
            {
                TempData["Error"] = ex.Message;
                return RedirectToAction(nameof(Index));
            }
        }
    }
}