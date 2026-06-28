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
            try
            {
                _db = db;
                _configuration = configuration;
            }
            catch (Exception ex)
            {
                throw new Exception("Failed to initialize ProductController: " + ex.Message, ex);
            }
        }

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

        private bool CanEdit()
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

        // ================= INDEX =================

        public IActionResult Index(string? search = null)
        {
            try
            {
                if (!IsUserLoggedIn())
                {
                    return RedirectToAction("Login", "Account");
                }

                var products = ProductModel.GetAll(_db, search);
                ViewBag.Search = search;
                ViewBag.Categories = CategoryModel.GetAll(_db);

                return View(products);
            }
            catch (SqlException ex)
            {
                TempData["Error"] = "Database error loading products: " + ex.Message;
                return RedirectToAction("Index", "Dashboard");
            }
            catch (Exception ex)
            {
                TempData["Error"] = "Error loading products: " + ex.Message;
                return RedirectToAction("Index", "Dashboard");
            }
        }

        // ================= CREATE GET =================

        [HttpGet]
        public IActionResult Create()
        {
            try
            {
                if (!IsUserLoggedIn())
                {
                    return RedirectToAction("Login", "Account");
                }

                if (!CanEdit())
                {
                    TempData["Error"] = "Access Denied. Only Admin and Super User can add products.";
                    return RedirectToAction("Index");
                }

                ViewBag.Categories = CategoryModel.GetAll(_db);
                return View();
            }
            catch (Exception ex)
            {
                TempData["Error"] = "Error loading create form: " + ex.Message;
                return RedirectToAction("Index");
            }
        }

        // ================= CREATE POST =================

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Create(ProductModel model)
        {
            try
            {
                if (!IsUserLoggedIn())
                {
                    return RedirectToAction("Login", "Account");
                }

                if (!CanEdit())
                {
                    TempData["Error"] = "Access Denied. Only Admin and Super User can add products.";
                    return RedirectToAction("Index");
                }

                if (!ModelState.IsValid)
                {
                    ViewBag.Categories = CategoryModel.GetAll(_db);
                    return View(model);
                }

                string message = model.Insert(_db);

                if (message == "Product added successfully!")
                {
                    TempData["Success"] = message;
                    return RedirectToAction("Index");
                }

                ModelState.AddModelError("", message);
            }
            catch (SqlException ex)
            {
                ModelState.AddModelError("", "Database error: " + ex.Message);
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", "Error: " + ex.Message);
            }

            ViewBag.Categories = CategoryModel.GetAll(_db);
            return View(model);
        }

        // ================= EDIT GET =================

        [HttpGet]
        public IActionResult Edit(int id)
        {
            try
            {
                if (!IsUserLoggedIn())
                {
                    return RedirectToAction("Login", "Account");
                }

                if (!CanEdit())
                {
                    TempData["Error"] = "Access Denied. Only Admin and Super User can edit products.";
                    return RedirectToAction("Index");
                }

                var product = ProductModel.GetById(_db, id);

                if (product == null)
                {
                    TempData["Error"] = "Product not found.";
                    return RedirectToAction("Index");
                }

                ViewBag.Categories = CategoryModel.GetAll(_db);
                return View(product);
            }
            catch (SqlException ex)
            {
                TempData["Error"] = "Database error loading product: " + ex.Message;
                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                TempData["Error"] = "Error loading product: " + ex.Message;
                return RedirectToAction("Index");
            }
        }

        // ================= EDIT POST =================

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Edit(ProductModel model)
        {
            try
            {
                if (!IsUserLoggedIn())
                {
                    return RedirectToAction("Login", "Account");
                }

                if (!CanEdit())
                {
                    TempData["Error"] = "Access Denied. Only Admin and Super User can edit products.";
                    return RedirectToAction("Index");
                }

                if (!ModelState.IsValid)
                {
                    ViewBag.Categories = CategoryModel.GetAll(_db);
                    return View(model);
                }

                string message = model.Update(_db);

                if (message == "Product updated successfully!")
                {
                    TempData["Success"] = message;
                    return RedirectToAction("Index");
                }

                ModelState.AddModelError("", message);
            }
            catch (SqlException ex)
            {
                ModelState.AddModelError("", "Database error: " + ex.Message);
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", "Error: " + ex.Message);
            }

            ViewBag.Categories = CategoryModel.GetAll(_db);
            return View(model);
        }

        // ================= DETAILS =================

        [HttpGet]
        public IActionResult Details(int id)
        {
            try
            {
                if (!IsUserLoggedIn())
                {
                    return RedirectToAction("Login", "Account");
                }

                var product = ProductModel.GetById(_db, id);

                if (product == null)
                {
                    TempData["Error"] = "Product not found.";
                    return RedirectToAction("Index");
                }

                return View(product);
            }
            catch (SqlException ex)
            {
                TempData["Error"] = "Database error loading product details: " + ex.Message;
                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                TempData["Error"] = "Error loading product details: " + ex.Message;
                return RedirectToAction("Index");
            }
        }

        // ================= DELETE =================

        [HttpPost]
        public JsonResult Delete(int id)
        {
            try
            {
                if (!IsUserLoggedIn())
                {
                    return Json(new
                    {
                        success = false,
                        message = "Session expired. Please login again."
                    });
                }

                if (!CanEdit())
                {
                    return Json(new
                    {
                        success = false,
                        message = "Unauthorized. Only Admin and Super User can delete products."
                    });
                }

                string message = ProductModel.Delete(_db, id);

                return Json(new
                {
                    success = message.Contains("successfully"),
                    message = message
                });
            }
            catch (SqlException ex)
            {
                return Json(new
                {
                    success = false,
                    message = "Database error: " + ex.Message
                });
            }
            catch (Exception ex)
            {
                return Json(new
                {
                    success = false,
                    message = "Error: " + ex.Message
                });
            }
        }

        // ================= SEARCH =================

        [HttpGet]
        public JsonResult Search(string? search)
        {
            try
            {
                if (!IsUserLoggedIn())
                {
                    return Json(new
                    {
                        success = false,
                        message = "Unauthorized"
                    });
                }

                var products = ProductModel.GetAll(_db, search);

                return Json(new
                {
                    success = true,
                    data = products
                });
            }
            catch (SqlException ex)
            {
                return Json(new
                {
                    success = false,
                    message = "Database error: " + ex.Message
                });
            }
            catch (Exception ex)
            {
                return Json(new
                {
                    success = false,
                    message = "Error: " + ex.Message
                });
            }
        }

        // ========== EXPORT PRODUCTS TO CSV ==========
        [HttpGet]
        public IActionResult ExportCsv()
        {
            try
            {
                if (!IsUserLoggedIn())
                    return RedirectToAction("Login", "Account");

                var products = new List<ProductModel>();
                string connStr = _configuration.GetConnectionString("DefaultConnection");

                if (string.IsNullOrEmpty(connStr))
                {
                    TempData["Error"] = "Database connection string is missing.";
                    return RedirectToAction("Index");
                }

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

                if (products == null || products.Count == 0)
                {
                    TempData["Error"] = "No products found to export.";
                    return RedirectToAction("Index");
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