using InventoryManagement.Helpers;
using InventoryManagement.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;  
using System;
using System.Collections.Generic;
using System.Data;
using System.Text;  
namespace InventoryManagement.Controllers
{
    public class ProductController : Controller
    {
        private readonly DatabaseHelper _db;
        private readonly IConfiguration _configuration;  

        public ProductController(DatabaseHelper db, IConfiguration configuration) 
        {
            _db = db;
            _configuration = configuration;  
        }

        private bool IsUserLoggedIn()
        {
            return !string.IsNullOrEmpty(
                HttpContext.Session.GetString(
                    "Username"
                )
            );
        }

        // ================= INDEX =================

        public IActionResult Index(
            string? search = null
        )
        {
            if (!IsUserLoggedIn())
            {
                return RedirectToAction(
                    "Login",
                    "Account"
                );
            }

            var products =
                ProductModel.GetAll(
                    _db,
                    search
                );

            ViewBag.Search =
                search;

            ViewBag.Categories =
                CategoryModel.GetAll(
                    _db
                );

            return View(
                products
            );
        }

        // ================= CREATE GET =================

        [HttpGet]
        public IActionResult Create()
        {
            if (!IsUserLoggedIn())
            {
                return RedirectToAction(
                    "Login",
                    "Account"
                );
            }

            ViewBag.Categories =
                CategoryModel.GetAll(
                    _db
                );

            return View();
        }

        // ================= CREATE POST =================

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Create(
            ProductModel model
        )
        {
            if (!IsUserLoggedIn())
            {
                return RedirectToAction(
                    "Login",
                    "Account"
                );
            }

            if (!ModelState.IsValid)
            {
                ViewBag.Categories =
                    CategoryModel.GetAll(
                        _db
                    );

                return View(
                    model
                );
            }

            string message =
                model.Insert(
                    _db
                );

            if (message ==
                "Product added successfully!")
            {
                TempData["Success"] =
                    message;

                return RedirectToAction(
                    "Index"
                );
            }

            ModelState.AddModelError(
                "",
                message
            );

            ViewBag.Categories =
                CategoryModel.GetAll(
                    _db
                );

            return View(
                model
            );
        }

        // ================= EDIT GET =================

        [HttpGet]
        public IActionResult Edit(
            int id
        )
        {
            if (!IsUserLoggedIn())
            {
                return RedirectToAction(
                    "Login",
                    "Account"
                );
            }

            var product =
                ProductModel.GetById(
                    _db,
                    id
                );

            if (product == null)
            {
                return RedirectToAction(
                    "Index"
                );
            }

            ViewBag.Categories =
                CategoryModel.GetAll(
                    _db
                );

            return View(
                product
            );
        }

        // ================= EDIT POST =================

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Edit(
            ProductModel model
        )
        {
            if (!IsUserLoggedIn())
            {
                return RedirectToAction(
                    "Login",
                    "Account"
                );
            }

            if (!ModelState.IsValid)
            {
                ViewBag.Categories =
                    CategoryModel.GetAll(
                        _db
                    );

                return View(
                    model
                );
            }

            string message =
                model.Update(
                    _db
                );

            if (message ==
                "Product updated successfully!")
            {
                TempData["Success"] =
                    message;

                return RedirectToAction(
                    "Index"
                );
            }

            ModelState.AddModelError(
                "",
                message
            );

            ViewBag.Categories =
                CategoryModel.GetAll(
                    _db
                );

            return View(
                model
            );
        }

        // ================= DETAILS =================

        [HttpGet]
        public IActionResult Details(
            int id
        )
        {
            if (!IsUserLoggedIn())
            {
                return RedirectToAction(
                    "Login",
                    "Account"
                );
            }

            var product =
                ProductModel.GetById(
                    _db,
                    id
                );

            if (product == null)
            {
                return RedirectToAction(
                    "Index"
                );
            }

            return View(
                product
            );
        }

        // ================= DELETE =================

        [HttpPost]
        public JsonResult Delete(
            int id
        )
        {
            if (!IsUserLoggedIn())
            {
                return Json(
                    new
                    {
                        success = false,

                        message =
                        "Session expired. Please login again."
                    }
                );
            }

            string message =
                ProductModel.Delete(
                    _db,
                    id
                );

            return Json(
                new
                {
                    success =
                        message.Contains(
                            "successfully"
                        ),

                    message =
                        message
                }
            );
        }

        // ================= SEARCH =================

        [HttpGet]
        public JsonResult Search(
            string? search
        )
        {
            if (!IsUserLoggedIn())
            {
                return Json(
                    new
                    {
                        success = false,

                        message =
                        "Unauthorized"
                    }
                );
            }

            var products =
                ProductModel.GetAll(
                    _db,
                    search
                );

            return Json(
                new
                {
                    success = true,

                    data = products
                }
            );
        }

        // ========== ✅ EXPORT PRODUCTS TO CSV ==========
        [HttpGet]
        public IActionResult ExportCsv()
        {
            if (!IsUserLoggedIn())
                return RedirectToAction("Login", "Account");

            var products = new List<ProductModel>();
            string connStr = _configuration.GetConnectionString("DefaultConnection");

            using (SqlConnection conn = new SqlConnection(connStr))
            {
                conn.Open();
                using (SqlCommand cmd = new SqlCommand("USP_GetProducts", conn))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    using (var reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            products.Add(new ProductModel
                            {
                                ProductId = Convert.ToInt32(reader["ProductId"]),
                                ProductName = reader["ProductName"]?.ToString() ?? "",
                                CategoryName = reader["CategoryName"]?.ToString() ?? "",
                                Price = Convert.ToDecimal(reader["Price"]),
                                Quantity = Convert.ToInt32(reader["Quantity"]),
                                CreatedDate = Convert.ToDateTime(reader["CreatedDate"])
                            });
                        }
                    }
                }
            }

            string[] headers = { "ID", "Product Name", "Category", "Price", "Quantity", "Created Date" };

            string csvData = CsvHelper.ConvertToCsv(products, headers, item => new string[]
            {
                item.ProductId.ToString(),
                item.ProductName,
                item.CategoryName ?? "",
                item.Price.ToString("0.00"),
                item.Quantity.ToString(),
                item.CreatedDate.ToString("yyyy-MM-dd")
            });

            byte[] bytes = Encoding.UTF8.GetBytes(csvData);
            return File(bytes, "text/csv", "Products_" + DateTime.Now.ToString("yyyy-MM-dd") + ".csv");
        }
    }
}