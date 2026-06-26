using InventoryManagement.Helpers;
using InventoryManagement.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;  // ✅ Add this
using System;
using System.Collections.Generic;
using System.Data;
using System.Text;  // ✅ Add for Encoding

namespace InventoryManagement.Controllers
{
    public class CategoryController : Controller
    {
        private readonly DatabaseHelper _db;
        private readonly IConfiguration _configuration;  // ✅ Add this

        public CategoryController(DatabaseHelper db, IConfiguration configuration)  // ✅ Add parameter
        {
            _db = db;
            _configuration = configuration;  // ✅ Initialize
        }

        private bool IsUserLoggedIn()
        {
            return !string.IsNullOrEmpty(
                HttpContext.Session.GetString("Username")
            );
        }

        // ================= INDEX =================

        public IActionResult Index(string? search = null)
        {
            if (!IsUserLoggedIn())
                return RedirectToAction("Login", "Account");

            var categories = CategoryModel.GetAll(_db, search);

            ViewBag.Search = search;

            return View(categories);
        }

        // ================= CREATE GET =================

        [HttpGet]
        public IActionResult Create()
        {
            if (!IsUserLoggedIn())
                return RedirectToAction("Login", "Account");
            return View();
        }

        // ================= CREATE POST =================

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Create(CategoryModel model)
        {
            if (!IsUserLoggedIn())
                return RedirectToAction("Login", "Account");

            if (!ModelState.IsValid)
                return View(model);

            string message = model.Insert(_db);
            if (message == "Category added successfully.")
            {
                TempData["Success"] = message;
                return RedirectToAction("Index");

            }

            ModelState.AddModelError("", message);

            return View(model);
        }

        // ================= EDIT GET =================

        [HttpGet]
        public IActionResult Edit(int id)
        {
            if (!IsUserLoggedIn())
                return RedirectToAction("Login", "Account");

            var category = CategoryModel.GetById(_db, id);

            if (category == null)
                return RedirectToAction("Index");

            return View(category);
        }

        // ================= EDIT POST =================

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Edit(CategoryModel model)

        {
            if (!IsUserLoggedIn())
                return RedirectToAction("Login", "Account");

            if (!ModelState.IsValid)
                return View(model);

            string message = model.Update(_db);


            if (message == "Category updated successfully.")
            {
                TempData["Success"] = message;


                return RedirectToAction("Index");


            }

            ModelState.AddModelError("", message);
            return View(model);
        }

        // ================= DELETE =================

        [HttpPost]
        public JsonResult Delete(int id)
        {
            if (!IsUserLoggedIn())
            {
                return Json(new
                {

                    message = "Session expired. Please login again."

                });
            }

            string message =
                CategoryModel.Delete(
                    _db,
                    id
                );

            return Json(new
            {
                success =
                    message.Contains(
                        "successfully"
                    ),

                message = message
            });
        }

        // ================= AG GRID =================

        [HttpGet]
        public JsonResult GetCategories()
        {
            if (!IsUserLoggedIn())
            {
                return Json(new
                {
                    success = false,

                    message =
                    "Unauthorized"
                });
            }

            var categories =
                CategoryModel.GetAll(
                    _db
                );

            return Json(new
            {
                success = true,

                data = categories
            });
        }

        // ========== EXPORT CATEGORIES TO CSV ==========
        [HttpGet]
        public IActionResult ExportCsv()
        {
            if (!IsUserLoggedIn())
                return RedirectToAction("Login", "Account");

            var categories = new List<CategoryModel>();
            string connStr = _configuration.GetConnectionString("DefaultConnection");  // ✅ Now works

            using (SqlConnection conn = new SqlConnection(connStr))
            {
                conn.Open();
                using (SqlCommand cmd = new SqlCommand("sp_GetAllCategories", conn))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    using (var reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            categories.Add(new CategoryModel
                            {
                                CategoryId = Convert.ToInt32(reader["CategoryId"]),
                                CategoryName = reader["CategoryName"]?.ToString() ?? "",
                                Description = reader["Description"]?.ToString() ?? "",
                                IsActive = Convert.ToBoolean(reader["IsActive"]),
                                CreatedDate = Convert.ToDateTime(reader["CreatedDate"])
                            });
                        }
                    }
                }
            }

            // ✅ Headers as per your table
            string[] headers = {
                "Category Name",
                "Status",
                "Created Date"
            };

            string csvData = CsvHelper.ConvertToCsv(categories, headers, item => new string[]
            {
                item.CategoryName,
                item.IsActive ? "Active" : "Inactive",
                item.CreatedDate.ToString("dd-MM-yyyy")
            });

            byte[] bytes = System.Text.Encoding.UTF8.GetBytes(csvData);
            return File(bytes, "text/csv", "Categories_" + DateTime.Now.ToString("yyyy-MM-dd") + ".csv");
        }
    }
}