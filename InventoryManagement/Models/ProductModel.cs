using InventoryManagement.Helpers;
using Microsoft.Data.SqlClient;
using System;
using System.Collections;
using System.ComponentModel.DataAnnotations;
using System.Data;

namespace InventoryManagement.Models
{
    public class ProductModel
    {
        public int ProductId { get; set; }

        [Required(ErrorMessage = "Product name is required")]
        [RegularExpression(@"^(?=.*[A-Za-z])[A-Za-z0-9\s\-_()]+$",
            ErrorMessage = "Product name must contain at least one letter and only valid characters are allowed.")]
        public string ProductName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Category is required")]
        public int? CategoryId { get; set; }

        public string CategoryName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Price is required")]
        [Range(typeof(decimal), "1", "1000",
            ErrorMessage = "Price must be between 1 and 1000")]
        public decimal? Price { get; set; }

        [Required(ErrorMessage = "Quantity is required")]
        [Range(1, 1000,
            ErrorMessage = "Quantity must be between 1 and 1000")]
        public int? Quantity { get; set; }

        public DateTime CreatedDate { get; set; } = DateTime.Now;

        // ================= GET ALL =================

        public static List<ProductModel> GetAll(
            DatabaseHelper db,
            string? search = null
        )
        {
            var list = new List<ProductModel>();

            try
            {
                if (db == null)
                {
                    throw new Exception("DatabaseHelper cannot be null.");
                }

                DataTable dt = db.ExecuteStoredProcedure("USP_GetProducts");

                if (dt == null || dt.Rows.Count == 0)
                {
                    return list;
                }

                foreach (DataRow row in dt.Rows)
                {
                    try
                    {
                        string productName = row["ProductName"]?.ToString() ?? "";

                        if (!string.IsNullOrEmpty(search))
                        {
                            if (!productName.ToLower().Contains(search.ToLower()))
                            {
                                continue;
                            }
                        }

                        list.Add(new ProductModel
                        {
                            ProductId = Convert.ToInt32(row["ProductId"]),
                            ProductName = productName,
                            CategoryId = Convert.ToInt32(row["CategoryId"]),
                            CategoryName = row["CategoryName"]?.ToString() ?? "",
                            Price = Convert.ToDecimal(row["Price"]),
                            Quantity = Convert.ToInt32(row["Quantity"]),
                            CreatedDate = Convert.ToDateTime(row["CreatedDate"])
                        });
                    }
                    catch (Exception ex)
                    {
                        // Log individual row error but continue processing
                        Console.WriteLine($"Error processing product row: {ex.Message}");
                    }
                }
            }
            catch (SqlException ex)
            {
                throw new Exception("Database error while fetching products: " + ex.Message, ex);
            }
            catch (Exception ex)
            {
                throw new Exception("Error fetching products: " + ex.Message, ex);
            }

            return list;
        }

        // ================= GET BY ID =================

        public static ProductModel? GetById(
            DatabaseHelper db,
            int id
        )
        {
            try
            {
                if (db == null)
                {
                    throw new Exception("DatabaseHelper cannot be null.");
                }

                if (id <= 0)
                {
                    return null;
                }

                var ht = new Hashtable();
                ht.Add("@ProductId", id);

                DataTable dt = db.ExecuteStoredProcedure("USP_GetProductById", ht);

                if (dt == null || dt.Rows.Count == 0)
                {
                    return null;
                }

                DataRow row = dt.Rows[0];

                return new ProductModel
                {
                    ProductId = Convert.ToInt32(row["ProductId"]),
                    ProductName = row["ProductName"]?.ToString() ?? "",
                    CategoryId = Convert.ToInt32(row["CategoryId"]),
                    CategoryName = row["CategoryName"]?.ToString() ?? "",
                    Price = Convert.ToDecimal(row["Price"]),
                    Quantity = Convert.ToInt32(row["Quantity"]),
                    CreatedDate = Convert.ToDateTime(row["CreatedDate"])
                };
            }
            catch (SqlException ex)
            {
                throw new Exception($"Database error while fetching product with ID {id}: " + ex.Message, ex);
            }
            catch (Exception ex)
            {
                throw new Exception($"Error fetching product with ID {id}: " + ex.Message, ex);
            }
        }

        // ================= INSERT =================

        public string Insert(DatabaseHelper db)
        {
            try
            {
                if (db == null)
                {
                    throw new Exception("DatabaseHelper cannot be null.");
                }

                if (string.IsNullOrWhiteSpace(ProductName))
                {
                    return "Product name is required.";
                }

                if (!CategoryId.HasValue || CategoryId.Value <= 0)
                {
                    return "Category is required.";
                }

                if (!Price.HasValue)
                {
                    return "Price is required.";
                }

                if (Price.Value < 1 || Price.Value > 1000)
                {
                    return "Price must be between 1 and 1000.";
                }

                if (!Quantity.HasValue)
                {
                    return "Quantity is required.";
                }

                if (Quantity.Value < 1 || Quantity.Value > 1000)
                {
                    return "Quantity must be between 1 and 1000.";
                }

                var ht = new Hashtable();
                ht.Add("@ProductName", ProductName.Trim());
                ht.Add("@CategoryId", CategoryId.Value);
                ht.Add("@Price", Price.Value);
                ht.Add("@Quantity", Quantity.Value);

                DataTable dt = db.ExecuteStoredProcedure("USP_InsertProduct", ht);

                if (dt == null || dt.Rows.Count == 0)
                {
                    return "No response from database.";
                }

                return dt.Rows[0]["Message"]?.ToString() ?? "Unknown error occurred.";
            }
            catch (SqlException ex)
            {
                return "Database error: " + ex.Message;
            }
            catch (Exception ex)
            {
                return "Error: " + ex.Message;
            }
        }

        // ================= UPDATE =================

        public string Update(DatabaseHelper db)
        {
            try
            {
                if (db == null)
                {
                    throw new Exception("DatabaseHelper cannot be null.");
                }

                if (ProductId <= 0)
                {
                    return "Invalid product ID.";
                }

                if (string.IsNullOrWhiteSpace(ProductName))
                {
                    return "Product name is required.";
                }

                if (!CategoryId.HasValue || CategoryId.Value <= 0)
                {
                    return "Category is required.";
                }

                if (!Price.HasValue)
                {
                    return "Price is required.";
                }

                if (Price.Value < 1 || Price.Value > 1000)
                {
                    return "Price must be between 1 and 1000.";
                }

                if (!Quantity.HasValue)
                {
                    return "Quantity is required.";
                }

                if (Quantity.Value < 1 || Quantity.Value > 1000)
                {
                    return "Quantity must be between 1 and 1000.";
                }

                var ht = new Hashtable();
                ht.Add("@ProductId", ProductId);
                ht.Add("@ProductName", ProductName.Trim());
                ht.Add("@CategoryId", CategoryId.Value);
                ht.Add("@Price", Price.Value);
                ht.Add("@Quantity", Quantity.Value);

                DataTable dt = db.ExecuteStoredProcedure("USP_UpdateProduct", ht);

                if (dt == null || dt.Rows.Count == 0)
                {
                    return "No response from database.";
                }

                return dt.Rows[0]["Message"]?.ToString() ?? "Unknown error occurred.";
            }
            catch (SqlException ex)
            {
                return "Database error: " + ex.Message;
            }
            catch (Exception ex)
            {
                return "Error: " + ex.Message;
            }
        }
        // ================= DELETE =================

        public static string Delete(DatabaseHelper db, int id)
        {
            try
            {
                if (db == null)
                {
                    throw new Exception("DatabaseHelper cannot be null.");
                }

                if (id <= 0)
                {
                    return "Invalid product ID.";
                }

                var ht = new Hashtable();
                ht.Add("@ProductId", id);

                DataTable dt = db.ExecuteStoredProcedure("USP_DeleteProduct", ht);

                if (dt == null || dt.Rows.Count == 0)
                {
                    return "No response from database.";
                }

                return dt.Rows[0]["Message"]?.ToString() ?? "Unknown error occurred.";
            }
            catch (SqlException ex)
            {
                return "Database error: " + ex.Message;
            }
            catch (Exception ex)
            {
                return "Error: " + ex.Message;
            }
        }
    }
}