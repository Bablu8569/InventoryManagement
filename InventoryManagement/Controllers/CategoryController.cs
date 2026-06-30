using InventoryManagement.Helpers;
using InventoryManagement.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Data;
using System.Text;

namespace InventoryManagement.Controllers
{
    public class CategoryController : Controller
    {
        private readonly DatabaseHelper _db;
        private readonly IConfiguration _configuration;

        public CategoryController(DatabaseHelper db, IConfiguration configuration)
        {
            _db = db;
            _configuration = configuration;
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
                    return RedirectToAction("Login", "Account");

                var categories = CategoryModel.GetAll(_db, search);
                ViewBag.Search = search;

                return View(categories);
            }
            catch (SqlException ex)
            {
                TempData["Error"] = "Database error loading categories: " + ex.Message;
                return RedirectToAction("Index", "Dashboard");
            }
            catch (Exception ex)
            {
                TempData["Error"] = "Error loading categories: " + ex.Message;
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
                    return RedirectToAction("Login", "Account");

                if (!CanEdit())
                {
                    TempData["Error"] = "Access Denied. Only Admin and Super User can add categories.";
                    return RedirectToAction("Index");
                }

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
        public IActionResult Create(CategoryModel model)
        {
            try
            {
                if (!IsUserLoggedIn())
                    return RedirectToAction("Login", "Account");

                if (!CanEdit())
                {
                    TempData["Error"] = "Access Denied. Only Admin and Super User can add categories.";
                    return RedirectToAction("Index");
                }

                if (!ModelState.IsValid)
                    return View(model);

                string message = model.Insert(_db);

                if (message == "Category added successfully.")
                {
                    ViewBag.Success = message;

                    // Form clear ho jayega
                    return View(new CategoryModel());
                }
                else
                {
                    ModelState.AddModelError("", message);
                    return View(model);
                }
            }
            catch (SqlException ex)
            {
                ModelState.AddModelError("", "Database error: " + ex.Message);
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", "Error: " + ex.Message);
            }

            return View(model);
        }
        // ================= EDIT GET =================

        [HttpGet]
        public IActionResult Edit(int id)
        {
            try
            {
                if (!IsUserLoggedIn())
                    return RedirectToAction("Login", "Account");

                if (!CanEdit())
                {
                    TempData["Error"] = "Access Denied. Only Admin and Super User can edit categories.";
                    return RedirectToAction("Index");
                }

                var category = CategoryModel.GetById(_db, id);

                if (category == null)
                {
                    TempData["Error"] = "Category not found.";
                    return RedirectToAction("Index");
                }

                return View(category);
            }
            catch (SqlException ex)
            {
                TempData["Error"] = "Database error loading category: " + ex.Message;
                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                TempData["Error"] = "Error loading category: " + ex.Message;
                return RedirectToAction("Index");
            }
        }

        // ================= EDIT POST =================

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Edit(CategoryModel model)
        {
            try
            {
                if (!IsUserLoggedIn())
                    return RedirectToAction("Login", "Account");

                if (!CanEdit())
                {
                    TempData["Error"] = "Access Denied. Only Admin and Super User can edit categories.";
                    return RedirectToAction("Index");
                }

                if (!ModelState.IsValid)
                    return View(model);

                string message = model.Update(_db);

                if (message == "Category updated successfully.")
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

            return View(model);
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
                        message = "Unauthorized. Only Admin and Super User can delete categories."
                    });
                }

                string message = CategoryModel.Delete(_db, id);

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

        // ================= AG GRID =================

        [HttpGet]
        public JsonResult GetCategories()
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

                var categories = CategoryModel.GetAll(_db);

                return Json(new
                {
                    success = true,
                    data = categories
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

        // ========== EXPORT CATEGORIES TO CSV ==========
      
    }
}