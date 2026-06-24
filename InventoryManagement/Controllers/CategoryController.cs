using InventoryManagement.Helpers;
using InventoryManagement.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace InventoryManagement.Controllers
{
    public class CategoryController : Controller
    {
        private readonly DatabaseHelper _db;

        public CategoryController(DatabaseHelper db)
        {
            _db = db;
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
    }
}