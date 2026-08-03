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
    public class ProductRepository : IProductRepository
    {
        private readonly DatabaseHelper _dbHelper;

        public ProductRepository(IConfiguration configuration)
        {
            try
            {
                _dbHelper = new DatabaseHelper(configuration);
            }
            catch (Exception ex)
            {
                throw new Exception("Failed to initialize ProductRepository: " + ex.Message, ex);
            }
        }

        public List<ProductModel> GetAllProducts()
        {
            var products = new List<ProductModel>();

            try
            {
                DataTable dt = _dbHelper.ExecuteStoredProcedure("USP_GetProducts");

                if (dt == null || dt.Rows.Count == 0)
                {
                    return products;
                }

                foreach (DataRow row in dt.Rows)
                {
                    try
                    {
                        products.Add(MapToProductModel(row));
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

            return products;
        }

        public ProductModel? GetProductById(int id)
        {
            try
            {
                if (id <= 0)
                {
                    return null;
                }

                var ht = new Hashtable();
                ht.Add("@ProductId", id);

                DataTable dt = _dbHelper.ExecuteStoredProcedure("USP_GetProductById", ht);

                if (dt != null && dt.Rows.Count > 0)
                {
                    return MapToProductModel(dt.Rows[0]);
                }
            }
            catch (SqlException ex)
            {
                throw new Exception($"Database error while fetching product with ID {id}: " + ex.Message, ex);
            }
            catch (Exception ex)
            {
                throw new Exception($"Error fetching product with ID {id}: " + ex.Message, ex);
            }

            return null;
        }

        public (bool Success, string Message) InsertProduct(ProductModel product)
        {
            try
            {
                if (product == null)
                {
                    return (false, "Product cannot be null.");
                }

                if (string.IsNullOrEmpty(product.ProductName))
                {
                    return (false, "Product name cannot be empty.");
                }

                if (product.CategoryId <= 0)
                {
                    return (false, "Invalid category selected.");
                }

                if (product.Price <= 0)
                {
                    return (false, "Price must be greater than 0.");
                }

                if (product.Quantity < 0)
                {
                    return (false, "Quantity cannot be negative.");
                }

                var ht = new Hashtable();
                ht.Add("@ProductName", product.ProductName);
                ht.Add("@CategoryId", product.CategoryId);
                ht.Add("@Price", product.Price);
                ht.Add("@Quantity", product.Quantity);

                DataTable dt = _dbHelper.ExecuteStoredProcedure("USP_InsertProduct", ht);

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
                return (false, "Database error inserting product: " + ex.Message);
            }
            catch (Exception ex)
            {
                return (false, "Error inserting product: " + ex.Message);
            }
        }

        public (bool Success, string Message) UpdateProduct(ProductModel product)
        {
            try
            {
                if (product == null)
                {
                    return (false, "Product cannot be null.");
                }

                if (product.ProductId <= 0)
                {
                    return (false, "Invalid product ID.");
                }

                if (string.IsNullOrEmpty(product.ProductName))
                {
                    return (false, "Product name cannot be empty.");
                }

                if (product.CategoryId <= 0)
                {
                    return (false, "Invalid category selected.");
                }

                if (product.Price <= 0)
                {
                    return (false, "Price must be greater than 0.");
                }

                if (product.Quantity < 0)
                {
                    return (false, "Quantity cannot be negative.");
                }

                var ht = new Hashtable();
                ht.Add("@ProductId", product.ProductId);
                ht.Add("@ProductName", product.ProductName);
                ht.Add("@CategoryId", product.CategoryId);
                ht.Add("@Price", product.Price);
                ht.Add("@Quantity", product.Quantity);

                DataTable dt = _dbHelper.ExecuteStoredProcedure("USP_UpdateProduct", ht);

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
                return (false, "Database error updating product: " + ex.Message);
            }
            catch (Exception ex)
            {
                return (false, "Error updating product: " + ex.Message);
            }
        }

        public (bool Success, string Message) DeleteProduct(int id)
        {
            try
            {
                if (id <= 0)
                {
                    return (false, "Invalid product ID.");
                }

                var ht = new Hashtable();
                ht.Add("@ProductId", id);

                DataTable dt = _dbHelper.ExecuteStoredProcedure("USP_DeleteProduct", ht);

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
                return (false, "Database error deleting product: " + ex.Message);
            }
            catch (Exception ex)
            {
                return (false, "Error deleting product: " + ex.Message);
            }
        }

        public List<ProductModel> SearchProducts(string? productName, int? categoryId)
        {
            var products = new List<ProductModel>();

            try
            {
                var ht = new Hashtable();
                ht.Add("@ProductName", string.IsNullOrEmpty(productName) ? DBNull.Value : productName);
                ht.Add("@CategoryId", categoryId.HasValue ? categoryId.Value : DBNull.Value);

                DataTable dt = _dbHelper.ExecuteStoredProcedure("USP_SearchProducts", ht);

                if (dt == null || dt.Rows.Count == 0)
                {
                    return products;
                }

                foreach (DataRow row in dt.Rows)
                {
                    try
                    {
                        products.Add(MapToProductModel(row));
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"Error processing search result row: {ex.Message}");
                    }
                }
            }
            catch (SqlException ex)
            {
                throw new Exception("Database error while searching products: " + ex.Message, ex);
            }
            catch (Exception ex)
            {
                throw new Exception("Error searching products: " + ex.Message, ex);
            }

            return products;
        }

        private ProductModel MapToProductModel(DataRow row)
        {
            try
            {
                return new ProductModel
                {
                    ProductId = Convert.ToInt32(row["ProductId"]),
                    ProductName = row["ProductName"]?.ToString() ?? string.Empty,
                    CategoryId = Convert.ToInt32(row["CategoryId"]),
                    CategoryName = row["CategoryName"]?.ToString() ?? string.Empty,
                    Price = Convert.ToDecimal(row["Price"]),
                    Quantity = Convert.ToInt32(row["Quantity"]),
                    CreatedDate = Convert.ToDateTime(row["CreatedDate"])
                };
            }
            catch (Exception ex)
            {
                throw new Exception("Error mapping DataRow to ProductModel: " + ex.Message, ex);
            }
        }
    }
}