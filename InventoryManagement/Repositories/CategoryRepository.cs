using InventoryManagement.Helpers;
using InventoryManagement.Models;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;

namespace InventoryManagement.Repositories
{
    public class CategoryRepository : ICategoryRepository
    {
        private readonly DatabaseHelper _dbHelper;

        public CategoryRepository(IConfiguration configuration)
        {
            try
            {
                _dbHelper = new DatabaseHelper(configuration);
            }
            catch (Exception ex)
            {
                throw new Exception("Failed to initialize CategoryRepository: " + ex.Message, ex);
            }
        }

        // ========== GET ALL CATEGORIES ==========
        public List<CategoryModel> GetAllCategories()
        {
            var categories = new List<CategoryModel>();

            try
            {
                DataTable dt = _dbHelper.ExecuteStoredProcedure("USP_GetCategories");

                if (dt == null || dt.Rows.Count == 0)
                {
                    return categories;
                }

                foreach (DataRow row in dt.Rows)
                {
                    try
                    {
                        categories.Add(new CategoryModel
                        {
                            CategoryId = Convert.ToInt32(row["CategoryId"]),
                            CategoryName = row["CategoryName"]?.ToString() ?? string.Empty,
                            IsActive = Convert.ToBoolean(row["IsActive"]),
                            CreatedDate = Convert.ToDateTime(row["CreatedDate"])
                        });
                    }
                    catch (Exception ex)
                    {
                        // Log individual row error but continue processing
                        Console.WriteLine($"Error processing category row: {ex.Message}");
                    }
                }
            }
            catch (SqlException ex)
            {
                throw new Exception("Database error while fetching categories: " + ex.Message, ex);
            }
            catch (Exception ex)
            {
                throw new Exception("Error fetching categories: " + ex.Message, ex);
            }

            return categories;
        }

        // ========== GET CATEGORY BY ID ==========
        public CategoryModel? GetCategoryById(int id)
        {
            try
            {
                if (id <= 0)
                {
                    return null;
                }

                var ht = new Hashtable();
                ht.Add("@CategoryId", id);

                DataTable dt = _dbHelper.ExecuteStoredProcedure("USP_GetCategoryById", ht);

                if (dt != null && dt.Rows.Count > 0)
                {
                    DataRow row = dt.Rows[0];

                    return new CategoryModel
                    {
                        CategoryId = Convert.ToInt32(row["CategoryId"]),
                        CategoryName = row["CategoryName"]?.ToString() ?? string.Empty,
                        IsActive = Convert.ToBoolean(row["IsActive"]),
                        CreatedDate = Convert.ToDateTime(row["CreatedDate"])
                    };
                }
            }
            catch (SqlException ex)
            {
                throw new Exception($"Database error while fetching category with ID {id}: " + ex.Message, ex);
            }
            catch (Exception ex)
            {
                throw new Exception($"Error fetching category with ID {id}: " + ex.Message, ex);
            }

            return null;
        }

        // ========== INSERT CATEGORY ==========
        public (bool Success, string Message) InsertCategory(CategoryModel category)
        {
            try
            {
                if (category == null)
                {
                    return (false, "Category cannot be null.");
                }

                if (string.IsNullOrEmpty(category.CategoryName))
                {
                    return (false, "Category name cannot be empty.");
                }

                var ht = new Hashtable();
                ht.Add("@CategoryName", category.CategoryName);
                ht.Add("@IsActive", category.IsActive);

                DataTable dt = _dbHelper.ExecuteStoredProcedure("USP_InsertCategory", ht);

                if (dt != null && dt.Rows.Count > 0)
                {
                    try
                    {
                        int result = Convert.ToInt32(dt.Rows[0]["Result"]);
                        string message = dt.Rows[0]["Message"]?.ToString() ?? "Unknown";

                        return (result == 1, message);
                    }
                    catch (Exception ex)
                    {
                        return (false, "Error reading result: " + ex.Message);
                    }
                }

                return (false, "No response from database.");
            }
            catch (SqlException ex)
            {
                return (false, "Database error inserting category: " + ex.Message);
            }
            catch (Exception ex)
            {
                return (false, "Error inserting category: " + ex.Message);
            }
        }

        // ========== UPDATE CATEGORY ==========
        public (bool Success, string Message) UpdateCategory(CategoryModel category)
        {
            try
            {
                if (category == null)
                {
                    return (false, "Category cannot be null.");
                }

                if (category.CategoryId <= 0)
                {
                    return (false, "Invalid category ID.");
                }

                if (string.IsNullOrEmpty(category.CategoryName))
                {
                    return (false, "Category name cannot be empty.");
                }

                var ht = new Hashtable();
                ht.Add("@CategoryId", category.CategoryId);
                ht.Add("@CategoryName", category.CategoryName);
                ht.Add("@IsActive", category.IsActive);

                DataTable dt = _dbHelper.ExecuteStoredProcedure("USP_UpdateCategory", ht);

                if (dt != null && dt.Rows.Count > 0)
                {
                    try
                    {
                        int result = Convert.ToInt32(dt.Rows[0]["Result"]);
                        string message = dt.Rows[0]["Message"]?.ToString() ?? "Unknown";

                        return (result == 1, message);
                    }
                    catch (Exception ex)
                    {
                        return (false, "Error reading result: " + ex.Message);
                    }
                }

                return (false, "No response from database.");
            }
            catch (SqlException ex)
            {
                return (false, "Database error updating category: " + ex.Message);
            }
            catch (Exception ex)
            {
                return (false, "Error updating category: " + ex.Message);
            }
        }

        // ========== DELETE CATEGORY ==========
        public (bool Success, string Message) DeleteCategory(int id)
        {
            try
            {
                if (id <= 0)
                {
                    return (false, "Invalid category ID.");
                }

                var ht = new Hashtable();
                ht.Add("@CategoryId", id);

                DataTable dt = _dbHelper.ExecuteStoredProcedure("USP_DeleteCategory", ht);

                if (dt != null && dt.Rows.Count > 0)
                {
                    try
                    {
                        int result = Convert.ToInt32(dt.Rows[0]["Result"]);
                        string message = dt.Rows[0]["Message"]?.ToString() ?? "Unknown";

                        return (result == 1, message);
                    }
                    catch (Exception ex)
                    {
                        return (false, "Error reading result: " + ex.Message);
                    }
                }

                return (false, "No response from database.");
            }
            catch (SqlException ex)
            {
                return (false, "Database error deleting category: " + ex.Message);
            }
            catch (Exception ex)
            {
                return (false, "Error deleting category: " + ex.Message);
            }
        }
    }
}