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
            IStockRepository stockRepository
        )
        {
            _configuration = configuration;
            _stockRepository = stockRepository;
        }

        // ========== HELPER METHODS ==========
        private bool IsUserLoggedIn()
        {
            try
            {
                return !string.IsNullOrEmpty(
                    HttpContext.Session.GetString("Username")
                );
            }
            catch
            {
                return false;
            }
        }

        private bool CanAddTransaction()
        {
            try
            {
                var role = HttpContext.Session.GetString("Role");
                return role == "1" || role == "3";
            }
            catch
            {
                return false;
            }
        }

        // ========== TRANSACTION HISTORY ==========
        public IActionResult Index(DateTime? fromDate, DateTime? toDate)
        {
            try
            {
                if (!IsUserLoggedIn())
                    return RedirectToAction("Login", "Account");

                var transactions = _stockRepository.GetStockTransactions(fromDate, toDate);

                ViewBag.FromDate = fromDate?.ToString("yyyy-MM-dd");
                ViewBag.ToDate = toDate?.ToString("yyyy-MM-dd");

                return View(transactions);
            }
            catch (SqlException ex)
            {
                TempData["Error"] = "Database error: " + ex.Message;
                return RedirectToAction("Index", "Dashboard");
            }
            catch (Exception ex)
            {
                TempData["Error"] = "Error loading transactions: " + ex.Message;
                return RedirectToAction("Index", "Dashboard");
            }
        }

        // ========== CREATE TRANSACTION (GET) ==========
        [HttpGet]
        public IActionResult Create()
        {
            try
            {
                if (!IsUserLoggedIn())
                    return RedirectToAction("Login", "Account");

                if (!CanAddTransaction())
                {
                    TempData["Error"] = "Access Denied. Only Admin and Super User can add stock transactions.";
                    return RedirectToAction("Index", "Dashboard");
                }

                var products = GetProductList();
                ViewBag.Products = products;

                return View();
            }
            catch (SqlException ex)
            {
                TempData["Error"] = "Database error: " + ex.Message;
                return RedirectToAction("Index", "Dashboard");
            }
            catch (Exception ex)
            {
                TempData["Error"] = "Error loading products: " + ex.Message;
                return RedirectToAction("Index", "Dashboard");
            }
        }

        // ========== CREATE TRANSACTION (POST) ==========
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
                    TempData["Error"] = "Access Denied. Only Admin and Super User can add stock transactions.";
                    return RedirectToAction("Index", "Dashboard");
                }

                if (!ModelState.IsValid)
                {
                    ViewBag.Products = GetProductList();
                    return View(model);
                }

                string connStr = _configuration.GetConnectionString("DefaultConnection");

                if (string.IsNullOrEmpty(connStr))
                {
                    ModelState.AddModelError("", "Database connection string is missing.");
                    ViewBag.Products = GetProductList();
                    return View(model);
                }

                using (SqlConnection conn = new SqlConnection(connStr))
                {
                    conn.Open();

                    // ✅ Check stock for "Out" transaction
                    if (model.TransactionType == "Out")
                    {
                        try
                        {
                            using (SqlCommand checkCmd = new SqlCommand("USP_CheckStock", conn))
                            {
                                checkCmd.CommandType = CommandType.StoredProcedure;
                                checkCmd.Parameters.AddWithValue("@ProductId", model.ProductId);

                                object result = checkCmd.ExecuteScalar();
                                int currentStock = result != null ? Convert.ToInt32(result) : 0;

                                if (currentStock < model.Quantity)
                                {
                                    ModelState.AddModelError("", "Insufficient stock! Available stock: " + currentStock);
                                    ViewBag.Products = GetProductList();
                                    return View(model);
                                }
                            }
                        }
                        catch (SqlException ex)
                        {
                            ModelState.AddModelError("", "Error checking stock: " + ex.Message);
                            ViewBag.Products = GetProductList();
                            return View(model);
                        }
                    }

                    // ✅ Update product quantity
                    try
                    {
                        using (SqlCommand updateCmd = new SqlCommand("USP_UpdateProductStock", conn))
                        {
                            updateCmd.CommandType = CommandType.StoredProcedure;
                            updateCmd.Parameters.AddWithValue("@ProductId", model.ProductId);
                            updateCmd.Parameters.AddWithValue("@Quantity", model.Quantity);
                            updateCmd.Parameters.AddWithValue("@TransactionType", model.TransactionType);
                            updateCmd.ExecuteNonQuery();
                        }
                    }
                    catch (SqlException ex)
                    {
                        ModelState.AddModelError("", "Error updating stock: " + ex.Message);
                        ViewBag.Products = GetProductList();
                        return View(model);
                    }

                    // ✅ Insert transaction record
                    try
                    {
                        using (SqlCommand insertCmd = new SqlCommand("USP_InsertStockTransaction", conn))
                        {
                            insertCmd.CommandType = CommandType.StoredProcedure;
                            insertCmd.Parameters.AddWithValue("@ProductId", model.ProductId);
                            insertCmd.Parameters.AddWithValue("@Quantity", model.Quantity);
                            insertCmd.Parameters.AddWithValue("@TransactionType", model.TransactionType);
                            insertCmd.Parameters.AddWithValue("@TransactionDate", DateTime.Now);
                            insertCmd.ExecuteNonQuery();
                        }
                    }
                    catch (SqlException ex)
                    {
                        ModelState.AddModelError("", "Error saving transaction: " + ex.Message);
                        ViewBag.Products = GetProductList();
                        return View(model);
                    }
                }

                TempData["Success"] = $"Stock {model.TransactionType} successful! Quantity: {model.Quantity}";
                return RedirectToAction("Index");
            }
            catch (SqlException ex)
            {
                ModelState.AddModelError("", "Database error: " + ex.Message);
                ViewBag.Products = GetProductList();
                return View(model);
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", "Error: " + ex.Message);
                ViewBag.Products = GetProductList();
                return View(model);
            }
        }

        // ========== GET PRODUCT LIST ==========
        private List<ProductModel> GetProductList()
        {
            var products = new List<ProductModel>();

            try
            {
                string connStr = _configuration.GetConnectionString("DefaultConnection");

                if (string.IsNullOrEmpty(connStr))
                {
                    return products;
                }

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
            catch (SqlException ex)
            {
                // Log error (you can add logging here)
                Console.WriteLine("SQL Error in GetProductList: " + ex.Message);
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error in GetProductList: " + ex.Message);
            }

            return products;
        }

        // ========== EXPORT TRANSACTIONS TO CSV ==========
        [HttpGet]
        public IActionResult ExportCsv(DateTime? fromDate, DateTime? toDate)
        {
            try
            {
                if (!IsUserLoggedIn())
                    return RedirectToAction("Login", "Account");

                // ✅ Get transactions with date filter
                var transactions = _stockRepository.GetStockTransactions(fromDate, toDate);

                if (transactions == null || transactions.Count == 0)
                {
                    TempData["Error"] = "No transactions found to export.";
                    return RedirectToAction("Index");
                }

                // ✅ Headers
                string[] headers = {
                    "Transaction ID",
                    "Product",
                    "Quantity",
                    "Type",
                    "Date & Time"
                };

                string csvData = CsvHelper.ConvertToCsv(transactions, headers, item => new string[]
                {
                    item.TransactionId.ToString(),
                    item.ProductName ?? "",
                    item.Quantity.ToString(),
                    item.TransactionType == "In" ? "Stock In" : "Stock Out",
                    item.TransactionDate.ToString("yyyy-MM-dd HH:mm:ss")
                });

                byte[] bytes = Encoding.UTF8.GetBytes(csvData);
                string fileName = "Transactions_" + DateTime.Now.ToString("yyyy-MM-dd") + ".csv";

                return File(bytes, "text/csv", fileName);
            }
            catch (SqlException ex)
            {
                TempData["Error"] = "Database error exporting CSV: " + ex.Message;
                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                TempData["Error"] = "Error exporting CSV: " + ex.Message;
                return RedirectToAction("Index");
            }
        }
    }
}