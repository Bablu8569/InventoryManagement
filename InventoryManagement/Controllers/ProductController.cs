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
                    ViewBag.Success = message;

                    // Dropdown reload
                    ViewBag.Categories = CategoryModel.GetAll(_db);

                    // Form clear
                    return View(new ProductModel());
                }
                else
                {
                    ViewBag.Categories = CategoryModel.GetAll(_db);

                    ModelState.AddModelError("", message);
                    return View(model);
                }
            }
            catch (SqlException ex)
            {
                ViewBag.Categories = CategoryModel.GetAll(_db);
                ModelState.AddModelError("", "Database error: " + ex.Message);
            }
            catch (Exception ex)
            {
                ViewBag.Categories = CategoryModel.GetAll(_db);
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

        
    }
}