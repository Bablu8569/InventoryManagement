using InventoryManagement.Models;
using InventoryManagement.Repositories;
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
            return !string.IsNullOrEmpty(
                HttpContext.Session.GetString("Username")
            );
        }

        private bool CanAddTransaction()
        {
            var role =
                HttpContext.Session.GetString("Role");

            return role == "1" || role == "3"; // Admin (1) or Super User (3)
        }

        // ========== TRANSACTION HISTORY ==========
        public IActionResult Index(
            DateTime? fromDate,
            DateTime? toDate
        )
        {
            if (!IsUserLoggedIn())
                return RedirectToAction(
                    "Login",
                    "Account"
                );

            var transactions =
                _stockRepository.GetStockTransactions(
                    fromDate,
                    toDate
                );

            ViewBag.FromDate =
                fromDate?.ToString("yyyy-MM-dd");

            ViewBag.ToDate =
                toDate?.ToString("yyyy-MM-dd");

            return View(transactions);
        }

        // ========== CREATE TRANSACTION (GET) ==========
        [HttpGet]
        public IActionResult Create()
        {
            if (!IsUserLoggedIn())
                return RedirectToAction(
                    "Login",
                    "Account"
                );

            // ✅ Admin (1) ya Super User (3) ko access
            if (!CanAddTransaction())
            {
                TempData["Error"] =
                    "Access Denied. Only Admin and Super User can add stock transactions.";

                return RedirectToAction(
                    "Index",
                    "Dashboard"
                );
            }

            var products = GetProductList();
            ViewBag.Products = products;

            return View();
        }

        // ========== CREATE TRANSACTION (POST) ==========
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Create(
            StockEntryModel model
        )
        {
            if (!IsUserLoggedIn())
                return RedirectToAction(
                    "Login",
                    "Account"
                );

            // ✅ Admin (1) ya Super User (3) ko access
            if (!CanAddTransaction())
            {
                TempData["Error"] =
                    "Access Denied. Only Admin and Super User can add stock transactions.";

                return RedirectToAction(
                    "Index",
                    "Dashboard"
                );
            }

            if (ModelState.IsValid)
            {
                try
                {
                    string connStr =
                        _configuration.GetConnectionString(
                            "DefaultConnection"
                        );

                    using (
                        SqlConnection conn =
                            new SqlConnection(connStr)
                    )
                    {
                        conn.Open();

                        // ✅ Check stock for "Out" transaction (Stored Procedure)
                        if (model.TransactionType == "Out")
                        {
                            using (
                                SqlCommand checkCmd =
                                    new SqlCommand(
                                        "USP_CheckStock",
                                        conn
                                    )
                            )
                            {
                                checkCmd.CommandType =
                                    CommandType.StoredProcedure;

                                checkCmd.Parameters.AddWithValue(
                                    "@ProductId",
                                    model.ProductId
                                );

                                int currentStock =
                                    (int)checkCmd.ExecuteScalar();

                                if (currentStock < model.Quantity)
                                {
                                    ModelState.AddModelError(
                                        "",
                                        "Insufficient stock! Available stock: "
                                        + currentStock
                                    );

                                    ViewBag.Products =
                                        GetProductList();

                                    return View(model);
                                }
                            }
                        }

                        // ✅ Update product quantity (Stored Procedure)
                        using (
                            SqlCommand updateCmd =
                                new SqlCommand(
                                    "USP_UpdateProductStock",
                                    conn
                                )
                        )
                        {
                            updateCmd.CommandType =
                                CommandType.StoredProcedure;

                            updateCmd.Parameters.AddWithValue(
                                "@ProductId",
                                model.ProductId
                            );

                            updateCmd.Parameters.AddWithValue(
                                "@Quantity",
                                model.Quantity
                            );

                            updateCmd.Parameters.AddWithValue(
                                "@TransactionType",
                                model.TransactionType
                            );

                            updateCmd.ExecuteNonQuery();
                        }

                        // ✅ Insert transaction record (Stored Procedure)
                        using (
                            SqlCommand insertCmd =
                                new SqlCommand(
                                    "USP_InsertStockTransaction",
                                    conn
                                )
                        )
                        {
                            insertCmd.CommandType =
                                CommandType.StoredProcedure;

                            insertCmd.Parameters.AddWithValue(
                                "@ProductId",
                                model.ProductId
                            );

                            insertCmd.Parameters.AddWithValue(
                                "@Quantity",
                                model.Quantity
                            );

                            insertCmd.Parameters.AddWithValue(
                                "@TransactionType",
                                model.TransactionType
                            );

                            insertCmd.Parameters.AddWithValue(
                                "@TransactionDate",
                                DateTime.Now
                            );

                            insertCmd.ExecuteNonQuery();
                        }
                    }

                    TempData["Success"] =
                        $"Stock {model.TransactionType} successful! Quantity: {model.Quantity}";

                    return RedirectToAction("Index");
                }
                catch (SqlException ex)
                {
                    ModelState.AddModelError(
                        "",
                        "Database error: " + ex.Message
                    );
                }
                catch (Exception ex)
                {
                    ModelState.AddModelError(
                        "",
                        "Error: " + ex.Message
                    );
                }
            }

            ViewBag.Products = GetProductList();
            return View(model);
        }

        // ========== GET PRODUCT LIST (Stored Procedure) ==========
        private List<ProductModel> GetProductList()
        {
            var products = new List<ProductModel>();

            string connStr =
                _configuration.GetConnectionString(
                    "DefaultConnection"
                );

            using (
                SqlConnection conn =
                    new SqlConnection(connStr)
            )
            {
                conn.Open();

                using (
                    SqlCommand cmd =
                        new SqlCommand(
                            "USP_GetProductsForStock",
                            conn
                        )
                )
                {
                    cmd.CommandType =
                        CommandType.StoredProcedure;

                    using (
                        SqlDataReader reader =
                            cmd.ExecuteReader()
                    )
                    {
                        while (reader.Read())
                        {
                            products.Add(
                                new ProductModel
                                {
                                    ProductId =
                                        Convert.ToInt32(
                                        reader["ProductId"]
                                        ),

                                    ProductName =
                                        reader["ProductName"]
                                        .ToString()
                                        ?? "",

                                    Quantity =
                                        Convert.ToInt32(
                                        reader["Quantity"]
                                        )
                                }
                            );
                        }
                    }
                }
            }

            return products;
        }
    }
}