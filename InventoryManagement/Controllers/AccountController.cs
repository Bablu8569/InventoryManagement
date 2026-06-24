using InventoryManagement.Helpers;
using InventoryManagement.Models;
using Microsoft.AspNetCore.Mvc;
using System;

namespace InventoryManagement.Controllers
{
    public class AccountController : Controller
    {
        private readonly DatabaseHelper _db;

        public AccountController(DatabaseHelper db)
        {
            _db = db;
        }

        // ========== HELPER METHODS ==========
        private bool IsUserLoggedIn()
        {
            return !string.IsNullOrEmpty(HttpContext.Session.GetString("Username"));
        }

        private bool IsAdmin()
        {
            return HttpContext.Session.GetString("Role") == "1";
        }

        // ========== LOGIN ==========
        [HttpGet]
        public IActionResult Login()
        {
            if (IsUserLoggedIn())
                return RedirectToAction("Index", "Dashboard");
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Login(LoginModel model)
        {
            if (!ModelState.IsValid)
                return View(model);

            try
            {
                var user = UserModel.ValidateUser(model.Username, model.Password, _db);

                if (user != null)
                {
                    HttpContext.Session.SetString("Username", user.Username);
                    HttpContext.Session.SetString("UserId", user.UserId.ToString());
                    HttpContext.Session.SetString("Role", user.Role);
                    HttpContext.Session.SetString("IsAdmin", (user.Role == "1").ToString());

                    TempData["LoginSuccess"] = "Welcome to Dashboard!";
                    return RedirectToAction("Index", "Dashboard");
                }
                else
                {
                    ModelState.AddModelError(string.Empty, "Invalid username or password.");
                }
            }
            catch (Exception ex)
            {
                ModelState.AddModelError(string.Empty, ex.Message);
            }

            return View(model);
        }

        // ========== SIGNUP ==========
        [HttpGet]
        public IActionResult Signup()
        {
            if (IsUserLoggedIn())
                return RedirectToAction("Index", "Dashboard");
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Signup(UserModel model)
        {
            if (!ModelState.IsValid)
                return View(model);

            var (result, message) = UserModel.CreateUser(
                model.Username,
                model.Email,
                model.Password,
                model.ConfirmPassword,
                _db
            );

            if (result == 0)
            {
                TempData["SignupSuccess"] = message;
                return RedirectToAction("Login");
            }
            else
            {
                ModelState.AddModelError(string.Empty, message);
            }

            return View(model);
        }

        // ========== LOGOUT ==========
        public IActionResult Logout()
        {
            HttpContext.Session.Clear();
            TempData.Clear();
            return RedirectToAction("Login");
        }

        // ========== INVENTORY ACCESS (Admin only) ==========
        [HttpGet]
        public IActionResult InventoryAccess()
        {
            if (!IsUserLoggedIn())
                return RedirectToAction("Login", "Account");

            if (!IsAdmin())
            {
                TempData["Error"] = "Access Denied. Only admin can manage user roles.";
                return RedirectToAction("Index", "Dashboard");
            }

            return View();
        }

        // ========== GET ALL USERS (Admin only) ==========
        [HttpGet]
        public JsonResult GetAllUsers()
        {
            try
            {
                if (!IsAdmin())
                    return Json(new { success = false, message = "Unauthorized." });

                var users = UserModel.GetUsersForDropdown(_db);

                if (users == null || users.Count == 0)
                {
                    return Json(new { success = false, message = "No users found." });
                }

                return Json(new { success = true, data = users });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        // ========== UPDATE USER ROLE (Admin only) ==========
        [HttpPost]
        public JsonResult UpdateUserRole(int userId, string newRole)
        {
            try
            {
                if (!IsAdmin())
                    return Json(new { success = false, message = "Unauthorized – only admin can modify roles." });

                var currentUserId = HttpContext.Session.GetString("UserId");
                var (success, message) = UserModel.UpdateUserRole(userId, newRole, _db, currentUserId);

                if (success)
                {
                    bool isCurrentUser = currentUserId != null && currentUserId == userId.ToString();

                    if (isCurrentUser)
                    {
                        HttpContext.Session.SetString("Role", newRole);
                        HttpContext.Session.SetString("IsAdmin", (newRole == "1").ToString());
                    }

                    return Json(new
                    {
                        success = true,
                        message = message,
                        forceLogout = isCurrentUser
                    });
                }
                else
                {
                    return Json(new { success = false, message = message });
                }
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Error: " + ex.Message });
            }
        }

        // ========== GET USERS (Legacy) ==========
        [HttpGet]
        public JsonResult GetUsers()
        {
            try
            {
                if (!IsAdmin())
                    return Json(new { success = false, message = "Unauthorized." });

                var users = UserModel.GetUsersForDropdown(_db);
                return Json(new { success = true, data = users });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        // ========== USER EXISTS (Legacy) ==========
        [HttpGet]
        public JsonResult CheckUserExists(string username, string email)
        {
            try
            {
                bool exists = UserModel.UserExists(username, email, _db);
                return Json(new { success = true, exists = exists });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }
    }
}