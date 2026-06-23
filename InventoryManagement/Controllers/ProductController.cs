using InventoryManagement.Helpers;
using InventoryManagement.Models;
using Microsoft.AspNetCore.Mvc;

namespace InventoryManagement.Controllers
{
    public class ProductController : Controller
    {
        private readonly DatabaseHelper _db;

        public ProductController( DatabaseHelper db)
        {
            _db = db;
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
    }
}