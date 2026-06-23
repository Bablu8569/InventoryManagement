using InventoryManagement.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using System;
using System.Data;
using System.Collections.Generic;

namespace InventoryManagement.Controllers
{
    public class AccountController : Controller
    {
        private readonly IConfiguration _configuration;

        public AccountController(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        // ========== LOGIN ==========
        [HttpGet]
        public IActionResult Login()
        {
            if (!string.IsNullOrEmpty(HttpContext.Session.GetString("Username")))
                return RedirectToAction("Index", "Dashboard");
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Login(LoginModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            try
            {
                var user = ValidateUser(model.Username, model.Password);

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

        // ========== VALIDATE USER ==========
        public UserModel ValidateUser(string username, string password)
        {
            UserModel user = null;
            string connectionString = _configuration.GetConnectionString("DefaultConnection");

            using (SqlConnection con = new SqlConnection(connectionString))
            {
                using (SqlCommand cmd = new SqlCommand("ValidateUser", con))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@UserName", username);
                    cmd.Parameters.AddWithValue("@Password", password);
                    con.Open();
                    SqlDataReader reader = cmd.ExecuteReader();
                    if (reader.Read())
                    {
                        user = new UserModel
                        {
                            UserId = Convert.ToInt32(reader["UserId"]),
                            Username = reader["Username"]?.ToString() ?? string.Empty,
                            Role = reader["Role"]?.ToString() ?? "2"
                        };
                    }
                }
            }
            return user;
        }

        // ========== SIGNUP ==========
        [HttpGet]
        public IActionResult Signup()
        {
            if (!string.IsNullOrEmpty(HttpContext.Session.GetString("Username")))
                return RedirectToAction("Index", "Dashboard");
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Signup(UserModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            string connStr = _configuration.GetConnectionString("DefaultConnection");
            using (SqlConnection conn = new SqlConnection(connStr))
            {
                conn.Open();
                using (SqlCommand cmd = new SqlCommand("Create_New_User", conn))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@Username", model.Username);
                    cmd.Parameters.AddWithValue("@Email", model.Email);
                    cmd.Parameters.AddWithValue("@Password", model.Password);
                    cmd.Parameters.AddWithValue("@ConfirmPassword", model.ConfirmPassword);

                    SqlParameter returnParam = new SqlParameter("@ReturnValue", SqlDbType.Int)
                    {
                        Direction = ParameterDirection.Output
                    };
                    cmd.Parameters.Add(returnParam);

                    cmd.ExecuteNonQuery();

                    int result = Convert.ToInt32(returnParam.Value);

                    switch (result)
                    {
                        case 0:
                            TempData["SignupSuccess"] = "Account created successfully! Please login.";
                            return RedirectToAction("Login");

                        case -1:
                            ModelState.AddModelError("Username", "Username already taken. Please choose another.");
                            break;

                        case -2:
                            ModelState.AddModelError("Email", "Email already registered. Please use another email.");
                            break;

                        case -3:
                            ModelState.AddModelError("Username", "Username must be at least 3 characters long.");
                            break;

                        case -4:
                            ModelState.AddModelError("Password", "Password must be at least 6 characters long.");
                            break;

                        case -5:
                            ModelState.AddModelError("ConfirmPassword", "Password and Confirm Password do not match.");
                            break;

                        case -6:
                            ModelState.AddModelError("Email", "Please enter a valid email address.");
                            break;

                        default:
                            ModelState.AddModelError(string.Empty, "An error occurred. Please try again.");
                            break;
                    }
                }
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

        // ========== USER EXISTS ==========
        private bool UserExists(string username, string email)
        {
            string connStr = _configuration.GetConnectionString("DefaultConnection");
            using (SqlConnection conn = new SqlConnection(connStr))
            {
                SqlCommand cmd = new SqlCommand("sp_UserExists", conn);
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@Username", username);
                cmd.Parameters.AddWithValue("@Email", email);
                conn.Open();
                int count = Convert.ToInt32(cmd.ExecuteScalar());
                return count > 0;
            }
        }

        // ========== INSERT USER (Legacy) ==========
        private int InsertUser(string username, string email, string password)
        {
            string connStr = _configuration.GetConnectionString("DefaultConnection");
            using (SqlConnection conn = new SqlConnection(connStr))
            {
                SqlCommand cmd = new SqlCommand("Create_New_User", conn);
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@Username", username);
                cmd.Parameters.AddWithValue("@Email", email);
                cmd.Parameters.AddWithValue("@Password", password);
                cmd.Parameters.AddWithValue("@ConfirmPassword", password);
                SqlParameter output = new SqlParameter("@ReturnValue", SqlDbType.Int);
                output.Direction = ParameterDirection.Output;
                cmd.Parameters.Add(output);
                conn.Open();
                cmd.ExecuteNonQuery();
                return Convert.ToInt32(output.Value);
            }
        }

        // ========== GET USERS (Legacy) ==========
        [HttpGet]
        public JsonResult GetUsers()
        {
            try
            {
                var users = new List<object>();
                string connStr = _configuration.GetConnectionString("DefaultConnection");
                using (SqlConnection conn = new SqlConnection(connStr))
                {
                    conn.Open();
                    string sql = "SELECT UserId, Username, Role FROM Users WHERE IsActive = 1 ORDER BY Username";
                    using (SqlCommand cmd = new SqlCommand(sql, conn))
                    using (var reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            users.Add(new
                            {
                                userId = Convert.ToInt32(reader["UserId"]),
                                username = reader["Username"]?.ToString() ?? string.Empty,
                                role = reader["Role"]?.ToString() ?? "2"
                            });
                        }
                    }
                }
                return Json(new { success = true, data = users });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        // ========== INVENTORY ACCESS ==========
        [HttpGet]
        public IActionResult InventoryAccess()
        {
            var role = HttpContext.Session.GetString("Role");
            if (role != "1")
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
                var role = HttpContext.Session.GetString("Role");
                if (role != "1")
                    return Json(new { success = false, message = "Unauthorized." });

                var users = new List<object>();
                string connStr = _configuration.GetConnectionString("DefaultConnection");
                using (SqlConnection conn = new SqlConnection(connStr))
                {
                    conn.Open();
                    string sql = "SELECT UserId, Username, Role FROM Users WHERE IsActive = 1 ORDER BY Username";
                    using (SqlCommand cmd = new SqlCommand(sql, conn))
                    using (var reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            users.Add(new
                            {
                                userId = Convert.ToInt32(reader["UserId"]),
                                username = reader["Username"]?.ToString() ?? string.Empty,
                                role = reader["Role"]?.ToString() ?? "2"
                            });
                        }
                    }
                }
                return Json(new { success = true, data = users });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        // ========== UPDATE USER ROLE (Admin only) ==========
        // ========== UPDATE USER ROLE (Admin only - Role = 1) ==========
        [HttpPost]
        public JsonResult UpdateUserRole(int userId, string newRole)
        {
            try
            {
                // ✅ Sirf Admin (Role = 1) ko role update karne ki permission
                var currentRole = HttpContext.Session.GetString("Role");
                if (currentRole != "1")
                    return Json(new { success = false, message = "Unauthorized – only admin can modify roles." });

                string connStr = _configuration.GetConnectionString("DefaultConnection");
                using (SqlConnection conn = new SqlConnection(connStr))
                {
                    conn.Open();

                    // ✅ Check karo ki user Admin (1) to nahi hai
                    string checkSql = "SELECT Role FROM Users WHERE UserId = @UserId";
                    using (SqlCommand checkCmd = new SqlCommand(checkSql, conn))
                    {
                        checkCmd.Parameters.AddWithValue("@UserId", userId);
                        string userRole = checkCmd.ExecuteScalar()?.ToString() ?? "2";

                        // ✅ Agar user Admin (1) hai to update mat karo
                        if (userRole == "1")
                        {
                            return Json(new
                            {
                                success = false,
                                message = "❌ Cannot update Admin role. Admin cannot be modified."
                            });
                        }
                    }

                    // ✅ Admin nahi hai to update karo
                    using (SqlCommand cmd = new SqlCommand("update_role", conn))
                    {
                        cmd.CommandType = CommandType.StoredProcedure;
                        cmd.Parameters.AddWithValue("@userid", userId);
                        cmd.Parameters.AddWithValue("@role", newRole);
                        int rowsAffected = cmd.ExecuteNonQuery();

                        if (rowsAffected > 0)
                        {
                            var currentUserId = HttpContext.Session.GetString("UserId");
                            bool isCurrentUser = currentUserId != null && currentUserId == userId.ToString();

                            if (isCurrentUser)
                            {
                                HttpContext.Session.SetString("Role", newRole);
                                HttpContext.Session.SetString("IsAdmin", (newRole == "1").ToString());
                            }

                            return Json(new
                            {
                                success = true,
                                message = "✅ Role updated successfully!" + (isCurrentUser ? " Your role has been changed." : ""),
                                forceLogout = isCurrentUser
                            });
                        }
                        else
                            return Json(new { success = false, message = "User not found." });
                    }
                }
            }
            catch (SqlException ex)
            {
                return Json(new { success = false, message = "Database error: " + ex.Message });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Error: " + ex.Message });
            }
        }
    }
}