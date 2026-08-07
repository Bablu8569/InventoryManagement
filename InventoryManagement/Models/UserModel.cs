using InventoryManagement.Helpers;
using Microsoft.Data.SqlClient;
using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Data;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.AspNetCore.Http;
using InventoryManagement.Helpers;
namespace InventoryManagement.Models
{
    public class UserModel
    {
        public int UserId { get; set; }

        // ================= BASIC DETAILS =================

        [Required(ErrorMessage = "Username is required.")]
        [StringLength(20, ErrorMessage = "Username cannot be more than 20 characters.")]
        [RegularExpression(@"^[a-zA-Z][a-zA-Z0-9_]*$",
            ErrorMessage = "Username must start with a letter and can contain only letters, numbers, and underscores.")]
        public string Username { get; set; } = string.Empty;

        [Required(ErrorMessage = "Email is required.")]
        [EmailAddress(ErrorMessage = "Enter a valid email address.")]
        public string Email { get; set; } = string.Empty;

        [Required(ErrorMessage = "Password is required.")]
        [RegularExpression(
            @"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[@$!%*?&])[A-Za-z\d@$!%*?&]{8,}$",
            ErrorMessage = "Password must contain at least 8 characters, including uppercase, lowercase, number and special character.")]
        public string Password { get; set; } = string.Empty;

        [Required(ErrorMessage = "Confirm Password is required.")]
        [Compare("Password", ErrorMessage = "Passwords do not match.")]
        public string ConfirmPassword { get; set; } = string.Empty;

        // ================= PERSONAL DETAILS =================

        [Required(ErrorMessage = "Mobile number is required.")]
        [RegularExpression(@"^[6-9]\d{9}$",
            ErrorMessage = "Enter a valid 10-digit mobile number.")]
        public string MobileNo { get; set; } = string.Empty;

        [Required(ErrorMessage = "Date of Birth is required.")]
        [AgeValidation(18)]
        public DateTime? DOB { get; set; }

        [Required(ErrorMessage = "Gender is required.")]
        public string Gender { get; set; } = string.Empty;

        [Required(ErrorMessage = "Marital Status is required.")]
        public string MaritalStatus { get; set; } = string.Empty;

        [Required(ErrorMessage = "Address is required.")]
        [StringLength(250, ErrorMessage = "Address cannot exceed 250 characters.")]
        public string Address { get; set; } = string.Empty;

        public string? Hobbies { get; set; }

        [NotMapped]
        public List<string> SelectedHobbies { get; set; } = new();

        public string? ProfileImage { get; set; }

        [NotMapped]
        public IFormFile? ProfileImageFile { get; set; }

        // ================= SYSTEM FIELDS =================

        public string Role { get; set; } = "2";

        public bool IsActive { get; set; } = true;

        public DateTime? CreatedDate { get; set; } = DateTime.Now;
        // ================= LOGIN =================

        public static (UserModel? User, string Message) ValidateUser(
            string username,
            string password,
            DatabaseHelper db)
        {
            Hashtable htUser = new Hashtable
            {
                { "@UserName", username }
            };

            DataTable dtUser =
                db.ExecuteStoredProcedure("USP_CheckUserName", htUser);

            if (dtUser.Rows.Count == 0)
            {
                return (null, "Username is incorrect.");
            }

            Hashtable htLogin = new Hashtable
            {
                { "@UserName", username },
                { "@Password", password }
            };

            DataTable dt =
                db.ExecuteStoredProcedure("ValidateUser", htLogin);

            if (dt.Rows.Count == 0)
            {
                return (null, "Password is incorrect.");
            }

            DataRow row = dt.Rows[0];

            UserModel user = new UserModel
            {
                UserId = Convert.ToInt32(row["UserId"]),
                Username = row["Username"].ToString() ?? "",
                Email = row["Email"].ToString() ?? "",
                Role = row["Role"].ToString() ?? "2"
            };

            return (user, "Login Successful");
        }

        // ================= CREATE USER =================

        public static (int Result, string Message) CreateUser(

            UserModel model,

            DatabaseHelper db)

        {
            try
            {
                string connStr = db.GetConnection().ConnectionString;

                using SqlConnection conn = new SqlConnection(connStr);

                conn.Open();

                using SqlCommand cmd = new SqlCommand("Create_New_User", conn);

                cmd.CommandType = CommandType.StoredProcedure;

                cmd.Parameters.AddWithValue("@Username", model.Username);
                cmd.Parameters.AddWithValue("@Email", model.Email);
                cmd.Parameters.AddWithValue("@Password", model.Password);
                cmd.Parameters.AddWithValue("@ConfirmPassword", model.ConfirmPassword);

                cmd.Parameters.AddWithValue("@MobileNo", model.MobileNo);

                cmd.Parameters.AddWithValue("@DOB", model.DOB);

                cmd.Parameters.AddWithValue("@Gender", model.Gender);

                cmd.Parameters.AddWithValue("@MaritalStatus", model.MaritalStatus);

                cmd.Parameters.AddWithValue("@Address", model.Address);

                cmd.Parameters.AddWithValue("@Hobbies",
                    (object?)model.Hobbies ?? DBNull.Value);

                cmd.Parameters.AddWithValue("@ProfileImage",
                    (object?)model.ProfileImage ?? DBNull.Value);

                SqlParameter returnParam =
                    new SqlParameter("@ReturnValue", SqlDbType.Int);

                returnParam.Direction = ParameterDirection.Output;

                cmd.Parameters.Add(returnParam);

                cmd.ExecuteNonQuery();

                int result = Convert.ToInt32(returnParam.Value);

                string message = result switch
                {
                    0 => "Account created successfully.",
                    -1 => "Username already exists.",
                    -2 => "Email already exists.",
                    _ => "Unable to create account."
                };

                return (result, message);
            }
            catch (Exception ex)
            {
                return (-99, ex.Message);
            }
        }
        // ================= GET USERS FOR DROPDOWN =================

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

        // ================= GET ALL USERS =================

        public static List<UserModel> GetAllUsers(DatabaseHelper db)
        {
            List<UserModel> users = new List<UserModel>();

            DataTable dt = db.ExecuteStoredProcedure("sp_GetUsersForDropdown");
            foreach (DataRow row in dt.Rows)
            {
                users.Add(new UserModel
                {
                    UserId = Convert.ToInt32(row["UserId"]),
                    Username = row["Username"]?.ToString() ?? string.Empty,
                    Email = row["Email"]?.ToString() ?? string.Empty,

                    MobileNo = row.Table.Columns.Contains("MobileNo")
                        ? row["MobileNo"]?.ToString() ?? string.Empty
                        : string.Empty,

                    DOB = row.Table.Columns.Contains("DOB") &&
                          row["DOB"] != DBNull.Value
                        ? Convert.ToDateTime(row["DOB"])
                        : null,

                    Gender = row.Table.Columns.Contains("Gender")
                        ? row["Gender"]?.ToString() ?? string.Empty
                        : string.Empty,

                    MaritalStatus = row.Table.Columns.Contains("MaritalStatus")
                        ? row["MaritalStatus"]?.ToString() ?? string.Empty
                        : string.Empty,

                    Address = row.Table.Columns.Contains("Address")
                        ? row["Address"]?.ToString() ?? string.Empty
                        : string.Empty,

                    Hobbies = row.Table.Columns.Contains("Hobbies")
                        ? row["Hobbies"]?.ToString()
                        : string.Empty,

                    ProfileImage = row.Table.Columns.Contains("ProfileImage")
                        ? row["ProfileImage"]?.ToString()
                        : string.Empty,

                    Role = row["Role"]?.ToString() ?? "2",

                    IsActive = row.Table.Columns.Contains("IsActive")
                        ? Convert.ToBoolean(row["IsActive"])
                        : true,

                    CreatedDate = row.Table.Columns.Contains("CreatedDate") &&
                                  row["CreatedDate"] != DBNull.Value
                        ? Convert.ToDateTime(row["CreatedDate"])
                        : null
                });
            }

            return users;
        }

        // ================= UPDATE USER ROLE =================

        public static (bool Success, string Message) UpdateUserRole(
            int userId,
            string newRole,
            DatabaseHelper db,
            string? currentUserId = null)
        {
            try
            {
                string connStr = db.GetConnection().ConnectionString;

                using SqlConnection conn = new SqlConnection(connStr);

                conn.Open();

                using (SqlCommand checkCmd = new SqlCommand(
                    "SELECT Role FROM Users WHERE UserId=@UserId", conn))
                {
                    checkCmd.Parameters.AddWithValue("@UserId", userId);

                    object? result = checkCmd.ExecuteScalar();

                    if (result == null)
                    {
                        return (false, "User not found.");
                    }

                    if (result.ToString() == "1")
                    {
                        return (false,
                            "Admin role cannot be modified.");
                    }
                }

                using (SqlCommand cmd =
                    new SqlCommand("update_role", conn))
                {
                    cmd.CommandType = CommandType.StoredProcedure;

                    cmd.Parameters.AddWithValue("@userid", userId);

                    cmd.Parameters.AddWithValue("@role", newRole);

                    cmd.ExecuteNonQuery();
                }

                bool isCurrentUser =
                    currentUserId != null &&
                    currentUserId == userId.ToString();

                return (
                    true,
                    isCurrentUser
                        ? "Role updated successfully. Please login again."
                        : "Role updated successfully."
                );
            }
            catch (Exception ex)
            {
                return (false, ex.Message);
            }
        }

        // ================= USER EXISTS =================

        public static bool UserExists(
            string username,
            string email,
            DatabaseHelper db)
        {
            Hashtable parameters = new Hashtable
            {
                { "@Username", username },
                { "@Email", email }
            };

            object? result =
                db.ExecuteScalar("sp_UserExists", parameters);

            return Convert.ToInt32(result ?? 0) > 0;
        }
    }
}