using InventoryManagement.Helpers;
using InventoryManagement.Models;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using System;
using System.IO;

namespace InventoryManagement.Controllers
{
    public class AccountController : Controller
    {
        private readonly DatabaseHelper _db;
        private readonly IWebHostEnvironment _environment;

        public AccountController(DatabaseHelper db, IWebHostEnvironment environment)
        {
            _db = db;
            _environment = environment;
        }

        // ================= HELPER METHODS =================

        private bool IsUserLoggedIn()
        {
            return !string.IsNullOrEmpty(
                HttpContext.Session.GetString("Username"));
        }

        private bool IsAdmin()
        {
            return HttpContext.Session.GetString("Role") == "1";
        }

        // ================= LOGIN =================

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
            try
            {
                if (!ModelState.IsValid)
                    return View(model);

                var result = UserModel.ValidateUser(
                    model.Username,
                    model.Password,
                    _db);

                if (result.User != null)
                {
                    HttpContext.Session.SetString(
                        "Username",
                        result.User.Username);

                    HttpContext.Session.SetString(
                        "UserId",
                        result.User.UserId.ToString());

                    HttpContext.Session.SetString(
                        "Role",
                        result.User.Role);

                    HttpContext.Session.SetString(
                        "IsAdmin",
                        (result.User.Role == "1").ToString());

                    TempData["LoginSuccess"] =
                        "Welcome to Dashboard!";

                    return RedirectToAction(
                        "Index",
                        "Dashboard");
                }

                if (result.Message == "Username is incorrect.")
                {
                    ModelState.AddModelError(
                        nameof(model.Username),
                        result.Message);
                }
                else if (result.Message == "Password is incorrect.")
                {
                    ModelState.AddModelError(
                        nameof(model.Password),
                        result.Message);
                }
                else
                {
                    ModelState.AddModelError(
                        "",
                        result.Message);
                }

                return View(model);
            }
            catch (SqlException ex)
            {
                ModelState.AddModelError(
                    "",
                    "Database Error : " + ex.Message);

                return View(model);
            }
            catch (Exception ex)
            {
                ModelState.AddModelError(
                    "",
                    ex.Message);

                return View(model);
            }
        }

        // ================= SIGNUP =================

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
            try
            {
                if (!ModelState.IsValid)
                    return View(model);

                // Upload Profile Image
                if (model.ProfileImageFile != null && model.ProfileImageFile.Length > 0)
                {
                    string uploadFolder = Path.Combine(_environment.WebRootPath, "ProfileImages");

                    if (!Directory.Exists(uploadFolder))
                    {
                        Directory.CreateDirectory(uploadFolder);
                    }

                    string fileName = Guid.NewGuid().ToString() +
                                      Path.GetExtension(model.ProfileImageFile.FileName);

                    string filePath = Path.Combine(uploadFolder, fileName);

                    using (FileStream stream = new FileStream(filePath, FileMode.Create))
                    {
                        model.ProfileImageFile.CopyTo(stream);
                    }

                    model.ProfileImage = fileName;
                }

                // Hobbies
                if (model.SelectedHobbies != null && model.SelectedHobbies.Any())
                {
                    model.Hobbies = string.Join(",", model.SelectedHobbies);
                }

                var result = UserModel.CreateUser(model, _db);

                if (result.Result == 0)
                {
                    TempData["SignupSuccess"] = result.Message;
                    return RedirectToAction("Login");
                }

                ModelState.AddModelError("", result.Message);
                return View(model);
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", ex.Message);
                return View(model);
            }
        }
        // ================= LOGOUT =================

        public IActionResult Logout()
        {
            HttpContext.Session.Clear();

            Response.Cookies.Delete(".AspNetCore.Session");

            return RedirectToAction("Login");
        }

        // ================= INVENTORY ACCESS =================

        [HttpGet]
        public IActionResult InventoryAccess()
        {
            if (!IsUserLoggedIn())
                return RedirectToAction("Login", "Account");

            if (!IsAdmin())
            {
                TempData["Error"] =
                    "Access Denied. Only Admin can manage user roles.";

                return RedirectToAction("Index", "Dashboard");
            }

            return View();
        }

        // ================= GET ALL USERS =================

        [HttpGet]
        public JsonResult GetAllUsers()
        {
            try
            {
                if (!IsAdmin())
                {
                    return Json(new
                    {
                        success = false,
                        message = "Unauthorized."
                    });
                }

                var users = UserModel.GetUsersForDropdown(_db);

                return Json(new
                {
                    success = true,
                    data = users
                });
            }
            catch (Exception ex)
            {
                return Json(new
                {
                    success = false,
                    message = ex.Message
                });
            }
        }

        // ================= UPDATE USER ROLE =================

        [HttpPost]
        public JsonResult UpdateUserRole(
            int userId,
            string newRole)
        {
            try
            {
                if (!IsAdmin())
                {
                    return Json(new
                    {
                        success = false,
                        message = "Unauthorized."
                    });
                }

                string? currentUserId =
                    HttpContext.Session.GetString("UserId");

                var result =
                    UserModel.UpdateUserRole(
                        userId,
                        newRole,
                        _db,
                        currentUserId);

                if (result.Success)
                {
                    bool isCurrentUser =
                        currentUserId != null &&
                        currentUserId == userId.ToString();

                    if (isCurrentUser)
                    {
                        HttpContext.Session.SetString(
                            "Role",
                            newRole);

                        HttpContext.Session.SetString(
                            "IsAdmin",
                            (newRole == "1").ToString());
                    }

                    return Json(new
                    {
                        success = true,
                        message = result.Message,
                        forceLogout = isCurrentUser
                    });
                }

                return Json(new
                {
                    success = false,
                    message = result.Message
                });
            }
            catch (Exception ex)
            {
                return Json(new
                {
                    success = false,
                    message = ex.Message
                });
            }
        }

        // ================= GET USERS =================

        [HttpGet]
        public JsonResult GetUsers()
        {
            try
            {
                if (!IsAdmin())
                {
                    return Json(new
                    {
                        success = false,
                        message = "Unauthorized."
                    });
                }

                var users =
                    UserModel.GetUsersForDropdown(_db);

                return Json(new
                {
                    success = true,
                    data = users
                });
            }
            catch (Exception ex)
            {
                return Json(new
                {
                    success = false,
                    message = ex.Message
                });
            }
        }

        // ================= CHECK USER EXISTS =================

        [HttpGet]
        public JsonResult CheckUserExists(
            string username,
            string email)
        {
            try
            {
                bool exists =
                    UserModel.UserExists(
                        username,
                        email,
                        _db);

                return Json(new
                {
                    success = true,
                    exists = exists
                });
            }
            catch (Exception ex)
            {
                return Json(new
                {
                    success = false,
                    message = ex.Message
                });
            }
        }
    }
}