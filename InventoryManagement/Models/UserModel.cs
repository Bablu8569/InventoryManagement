using InventoryManagement.Helpers;
using Microsoft.Data.SqlClient;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;

namespace InventoryManagement.Models
{
    public class UserModel
    {
        public int UserId { get; set; }
        public string Username { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
        public string ConfirmPassword { get; set; } = string.Empty;
        public string Role { get; set; } = "2";
        public bool IsActive { get; set; } = true;
        public DateTime? CreatedDate { get; set; }

        // ========== VALIDATE USER (LOGIN) ==========
        public static UserModel? ValidateUser(string username, string password, DatabaseHelper db)
        {
            Hashtable parameters = new Hashtable
            {
                { "@UserName", username },
                { "@Password", password }
            };

            DataTable dt = db.ExecuteStoredProcedure("ValidateUser", parameters);

            if (dt.Rows.Count > 0)
            {
                DataRow row = dt.Rows[0];
                return new UserModel
                {
                    UserId = Convert.ToInt32(row["UserId"]),
                    Username = row["Username"]?.ToString() ?? string.Empty,
                    Role = row["Role"]?.ToString() ?? "2"
                };
            }

            return null;
        }

        // ========== CREATE USER (SIGNUP) ==========
        public static (int Result, string Message) CreateUser(
            string username,
            string email,
            string password,
            string confirmPassword,
            DatabaseHelper db)
        {
            Hashtable parameters = new Hashtable
            {
                { "@Username", username },
                { "@Email", email },
                { "@Password", password },
                { "@ConfirmPassword", confirmPassword }
            };

            // Execute stored procedure with output parameter
            // Since DatabaseHelper doesn't have ExecuteNonQueryWithOutput,
            // we need to use a different approach
            try
            {
                string connStr = db.GetConnection().ConnectionString;
                using (SqlConnection conn = new SqlConnection(connStr))
                {
                    conn.Open();
                    using (SqlCommand cmd = new SqlCommand("Create_New_User", conn))
                    {
                        cmd.CommandType = CommandType.StoredProcedure;
                        cmd.Parameters.AddWithValue("@Username", username);
                        cmd.Parameters.AddWithValue("@Email", email);
                        cmd.Parameters.AddWithValue("@Password", password);
                        cmd.Parameters.AddWithValue("@ConfirmPassword", confirmPassword);

                        SqlParameter returnParam = new SqlParameter("@ReturnValue", SqlDbType.Int)
                        {
                            Direction = ParameterDirection.Output
                        };
                        cmd.Parameters.Add(returnParam);

                        cmd.ExecuteNonQuery();

                        int returnValue = Convert.ToInt32(returnParam.Value);
                        string message = returnValue switch
                        {
                            0 => "Account created successfully!",
                            -1 => "Username already taken.",
                            -2 => "Email already registered.",
                            -3 => "Username must be at least 3 characters.",
                            -4 => "Password must be at least 6 characters.",
                            -5 => "Password and Confirm Password do not match.",
                            -6 => "Please enter a valid email address.",
                            _ => "An error occurred. Please try again."
                        };

                        return (returnValue, message);
                    }
                }
            }
            catch (Exception ex)
            {
                return (-7, "Error: " + ex.Message);
            }
        }

        // ========== GET USERS FOR DROPDOWN ==========
        public static List<object> GetUsersForDropdown(DatabaseHelper db)
        {
            List<object> users = new List<object>();

            DataTable dt = db.ExecuteStoredProcedure("sp_GetUsersForDropdown");

            foreach (DataRow row in dt.Rows)
            {
                users.Add(new
                {
                    userId = Convert.ToInt32(row["UserId"]),
                    username = row["Username"]?.ToString() ?? string.Empty,
                    role = row["Role"]?.ToString() ?? "2"
                });
            }

            return users;
        }

        // ========== GET ALL USERS ==========
        public static List<UserModel> GetAllUsers(DatabaseHelper db)
        {
            List<UserModel> users = new List<UserModel>();

            DataTable dt = db.ExecuteStoredProcedure("sp_GetAllUsersForAccess");

            foreach (DataRow row in dt.Rows)
            {
                users.Add(new UserModel
                {
                    UserId = Convert.ToInt32(row["UserId"]),
                    Username = row["Username"]?.ToString() ?? string.Empty,
                    Email = row["Email"]?.ToString() ?? string.Empty,
                    Role = row["Role"]?.ToString() ?? "2",
                    IsActive = Convert.ToBoolean(row["IsActive"]),
                    CreatedDate = Convert.ToDateTime(row["CreatedDate"])
                });
            }

            return users;
        }

        // ========== UPDATE USER ROLE ==========
        public static (bool Success, string Message) UpdateUserRole(
            int userId,
            string newRole,
            DatabaseHelper db,
            string currentUserId = null)
        {
            try
            {
                // Check if user is Admin
                string connStr = db.GetConnection().ConnectionString;
                using (SqlConnection conn = new SqlConnection(connStr))
                {
                    conn.Open();

                    // Check if user is Admin (Role = 1)
                    string checkSql = "SELECT Role FROM Users WHERE UserId = @UserId";
                    using (SqlCommand checkCmd = new SqlCommand(checkSql, conn))
                    {
                        checkCmd.Parameters.AddWithValue("@UserId", userId);
                        string userRole = checkCmd.ExecuteScalar()?.ToString() ?? "2";

                        if (userRole == "1")
                        {
                            return (false, "❌ Cannot update Admin role. Admin cannot be modified.");
                        }
                    }

                    // Update role
                    using (SqlCommand cmd = new SqlCommand("update_role", conn))
                    {
                        cmd.CommandType = CommandType.StoredProcedure;
                        cmd.Parameters.AddWithValue("@userid", userId);
                        cmd.Parameters.AddWithValue("@role", newRole);
                        int rowsAffected = cmd.ExecuteNonQuery();

                        if (rowsAffected > 0)
                        {
                            bool isCurrentUser = currentUserId != null && currentUserId == userId.ToString();
                            return (true, "✅ Role updated successfully!" + (isCurrentUser ? " Your role has been changed." : ""));
                        }
                        else
                        {
                            return (false, "User not found.");
                        }
                    }
                }
            }
            catch (SqlException ex)
            {
                return (false, "Database error: " + ex.Message);
            }
            catch (Exception ex)
            {
                return (false, "Error: " + ex.Message);
            }
        }

        // ========== USER EXISTS ==========
        public static bool UserExists(string username, string email, DatabaseHelper db)
        {
            Hashtable parameters = new Hashtable
            {
                { "@Username", username },
                { "@Email", email }
            };

            object? result = db.ExecuteScalar("sp_UserExists", parameters);
            int count = Convert.ToInt32(result ?? 0);

            return count > 0;
        }
    }
}