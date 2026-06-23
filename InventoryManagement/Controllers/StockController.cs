using InventoryManagement.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Data;

namespace InventoryManagement.Controllers
{
    public class StockController : Controller
    {
        private readonly IConfiguration _configuration;

        public StockController(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        // ========== HELPER METHODS ==========
        private bool IsUserLoggedIn()
        {
            return !string.IsNullOrEmpty(HttpContext.Session.GetString("Username"));
        }

        private bool CanAddTransaction()
        {
            var role = HttpContext.Session.GetString("Role");
            return role == "1" || role == "3"; // Admin (1) or Super User (3)
        }

        // ========== TRANSACTION HISTORY ==========
        public IActionResult Index(DateTime? fromDate, DateTime? toDate)
        {
            if (!IsUserLoggedIn())
                return RedirectToAction("Login", "Account");

            var transactions = new List<StockTransactionModel>();
            string connStr = _configuration.GetConnectionString("DefaultConnection");

            using (SqlConnection conn = new SqlConnection(connStr))
            {
                conn.Open();
                using (SqlCommand cmd = new SqlCommand("USP_GetStockTransactions", conn))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@FromDate", (object)fromDate ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("@ToDate", (object)toDate ?? DBNull.Value);

                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            transactions.Add(new StockTransactionModel
                            {
                                TransactionId = Convert.ToInt32(reader["TransactionId"]),
                                ProductId = Convert.ToInt32(reader["ProductId"]),
                                Quantity = Convert.ToInt32(reader["Quantity"]),
                                TransactionType = reader["TransactionType"].ToString() ?? "",
                                TransactionDate = Convert.ToDateTime(reader["TransactionDate"]),
                                ProductName = reader["ProductName"].ToString() ?? ""
                            });
                        }
                    }
                }
            }

            ViewBag.FromDate = fromDate?.ToString("yyyy-MM-dd");
            ViewBag.ToDate = toDate?.ToString("yyyy-MM-dd");
            return View(transactions);
        }

        // ========== CREATE TRANSACTION (GET) ==========
        [HttpGet]
        public IActionResult Create()
        {
            if (!IsUserLoggedIn())
                return RedirectToAction("Login", "Account");

            // ✅ Admin (1) ya Super User (3) ko access
            if (!CanAddTransaction())
            {
                TempData["Error"] = "Access Denied. Only Admin and Super User can add stock transactions.";
                return RedirectToAction("Index", "Dashboard");
            }

            var products = GetProductList();
            ViewBag.Products = products;
            return View();
        }

        // ========== CREATE TRANSACTION (POST) ==========
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Create(StockEntryModel model)
        {
            if (!IsUserLoggedIn())
                return RedirectToAction("Login", "Account");

            // ✅ Admin (1) ya Super User (3) ko access
            if (!CanAddTransaction())
            {
                TempData["Error"] = "Access Denied. Only Admin and Super User can add stock transactions.";
                return RedirectToAction("Index", "Dashboard");
            }

            if (ModelState.IsValid)
            {
                try
                {
                    string connStr = _configuration.GetConnectionString("DefaultConnection");
                    using (SqlConnection conn = new SqlConnection(connStr))
                    {
                        conn.Open();

                        // ✅ Check stock for "Out" transaction
                        if (model.TransactionType == "Out")
                        {
                            string checkSql = "SELECT Quantity FROM Products WHERE ProductId = @ProductId";
                            SqlCommand checkCmd = new SqlCommand(checkSql, conn);
                            checkCmd.Parameters.AddWithValue("@ProductId", model.ProductId);
                            int currentStock = (int)checkCmd.ExecuteScalar();

                            if (currentStock < model.Quantity)
                            {
                                ModelState.AddModelError("", "Insufficient stock! Available stock: " + currentStock);
                                ViewBag.Products = GetProductList();
                                return View(model);
                            }
                        }

                        // ✅ Update product quantity
                        string updateSql = (model.TransactionType == "In")
                            ? "UPDATE Products SET Quantity = Quantity + @Qty WHERE ProductId = @ProductId"
                            : "UPDATE Products SET Quantity = Quantity - @Qty WHERE ProductId = @ProductId";

                        SqlCommand updateCmd = new SqlCommand(updateSql, conn);
                        updateCmd.Parameters.AddWithValue("@Qty", model.Quantity);
                        updateCmd.Parameters.AddWithValue("@ProductId", model.ProductId);
                        updateCmd.ExecuteNonQuery();

                        // ✅ Insert transaction record
                        string insertSql = @"
                            INSERT INTO StockTransactions (ProductId, Quantity, TransactionType, TransactionDate)
                            VALUES (@ProductId, @Qty, @Type, @Date)";
                        SqlCommand insertCmd = new SqlCommand(insertSql, conn);
                        insertCmd.Parameters.AddWithValue("@ProductId", model.ProductId);
                        insertCmd.Parameters.AddWithValue("@Qty", model.Quantity);
                        insertCmd.Parameters.AddWithValue("@Type", model.TransactionType);
                        insertCmd.Parameters.AddWithValue("@Date", DateTime.Now);
                        insertCmd.ExecuteNonQuery();
                    }

                    TempData["Success"] = $"Stock {model.TransactionType} successful! Quantity: {model.Quantity}";
                    return RedirectToAction("Index");
                }
                catch (SqlException ex)
                {
                    ModelState.AddModelError("", "Database error: " + ex.Message);
                }
                catch (Exception ex)
                {
                    ModelState.AddModelError("", "Error: " + ex.Message);
                }
            }

            ViewBag.Products = GetProductList();
            return View(model);
        }

        // ========== GET PRODUCT LIST ==========
        private List<ProductModel> GetProductList()
        {
            var products = new List<ProductModel>();
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
                                ProductName = reader["ProductName"].ToString() ?? "",
                                Quantity = Convert.ToInt32(reader["Quantity"])
                            });
                        }
                    }
                }
            }
            return products;
        }
    }
}