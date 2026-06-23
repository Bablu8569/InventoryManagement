using InventoryManagement.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Data;

namespace InventoryManagement.Controllers
{
   
    public class DashboardController : Controller
    {
        private readonly IConfiguration _configuration;

        public DashboardController(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        private bool IsUserLoggedIn() => !string.IsNullOrEmpty(HttpContext.Session.GetString("Username"));
        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Index()
        {
            if (!IsUserLoggedIn()) return RedirectToAction("Login", "Account");
          

            var model = new DashboardModel();
            string connStr = _configuration.GetConnectionString("DefaultConnection");

            using (SqlConnection conn = new SqlConnection(connStr))
            {
                conn.Open();

                // ---- Top stat cards using stored procedures ----
                using (SqlCommand cmd = new SqlCommand("sp_GetTotalCategories", conn))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    model.TotalCategories = (int)cmd.ExecuteScalar();
                }

                using (SqlCommand cmd = new SqlCommand("sp_GetTotalProducts", conn))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    model.TotalProducts = (int)cmd.ExecuteScalar();
                }

                // ---- Chart 3: Low stock items (quantity <= 5) ----


                using (SqlCommand cmd = new SqlCommand("sp_GetLowStockCount", conn))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    model.LowStockCount = (int)cmd.ExecuteScalar();
                }

                using (SqlCommand cmd = new SqlCommand("sp_GetTodayTransactionsCount", conn))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    model.TodayTransactions = (int)cmd.ExecuteScalar();
                }

                // ---- Chart 1: Categories + product counts ----
                using (SqlCommand cmd = new SqlCommand("sp_GetCategoryProductCounts", conn))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    using (var reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            model.CategoryNames.Add(reader["CategoryName"].ToString() ?? "");
                            model.CategoryProductCounts.Add(Convert.ToInt32(reader["ProductCount"]));
                        }
                    }
                }

                // ---- Chart 2: Products – grouped by ProductName ----

                using (SqlCommand cmd = new SqlCommand("sp_GetDashboardProducts", conn))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    using (var reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            model.ProductNames.Add(reader["ProductName"].ToString() ?? "");
                            model.ProductPrices.Add(Convert.ToDecimal(reader["Price"]));
                            model.ProductQuantities.Add(Convert.ToInt32(reader["Quantity"]));
                        }
                    }
                }

                // ---- Chart 3: Low stock items ----
                using (SqlCommand cmd = new SqlCommand("sp_GetLowStockProducts", conn))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    using (var reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            model.LowStockProductNames.Add(reader["ProductName"].ToString() ?? "");
                            model.LowStockQuantities.Add(Convert.ToInt32(reader["Quantity"]));
                        }
                    }
                }

                // ---- Chart 4: Today's transactions summary ----
                using (SqlCommand cmd = new SqlCommand("sp_GetTodayTransactionsSummary", conn))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    using (var reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            model.TransactionProductNames.Add(reader["ProductName"].ToString() ?? "");
                            model.TransactionQuantities.Add(Convert.ToInt32(reader["TotalMoved"]));
                        }
                    }
                }
            }

            return View(model);
        }


    }
}