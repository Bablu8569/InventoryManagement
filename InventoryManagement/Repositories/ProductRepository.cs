using InventoryManagement.Helpers;
using InventoryManagement.Models;
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
            _dbHelper = new DatabaseHelper(configuration);
        }

        public List<ProductModel> GetAllProducts()
        {
            List<ProductModel> products = new List<ProductModel>();

            DataTable dt =
                _dbHelper.ExecuteStoredProcedure(
                    "USP_GetProducts"
                );

            foreach (DataRow row in dt.Rows)
            {
                products.Add(
                    MapToProductModel(row)
                );
            }

            return products;
        }

        public ProductModel? GetProductById(int id)
        {
            Hashtable ht =
                new Hashtable();

            ht.Add(
                "@ProductId",
                id
            );

            DataTable dt =
                _dbHelper.ExecuteStoredProcedure(
                    "USP_GetProductById",
                    ht
                );

            if (dt.Rows.Count > 0)
            {
                return MapToProductModel(
                    dt.Rows[0]
                );
            }

            return null;
        }

        public (bool Success, string Message)
        InsertProduct(ProductModel product)
        {
            Hashtable ht =
                new Hashtable();

            ht.Add(
                "@ProductName",
                product.ProductName
            );

            ht.Add(
                "@CategoryId",
                product.CategoryId
            );

            ht.Add(
                "@Price",
                product.Price
            );

            ht.Add(
                "@Quantity",
                product.Quantity
            );

            DataTable dt =
                _dbHelper.ExecuteStoredProcedure(
                    "USP_InsertProduct",
                    ht
                );

            if (dt.Rows.Count > 0)
            {
                int result =
                    Convert.ToInt32(
                    dt.Rows[0]["Result"]
                    );

                string message =
                    dt.Rows[0]["Message"]
                    .ToString()
                    ?? "Unknown";

                return (
                    result == 1,
                    message
                );
            }

            return (
                false,
                "Unknown error occurred"
            );
        }

        public (bool Success, string Message)
        UpdateProduct(ProductModel product)
        {
            Hashtable ht =
                new Hashtable();

            ht.Add(
                "@ProductId",
                product.ProductId
            );

            ht.Add(
                "@ProductName",
                product.ProductName
            );

            ht.Add(
                "@CategoryId",
                product.CategoryId
            );

            ht.Add(
                "@Price",
                product.Price
            );

            ht.Add(
                "@Quantity",
                product.Quantity
            );

            DataTable dt =
                _dbHelper.ExecuteStoredProcedure(
                    "USP_UpdateProduct",
                    ht
                );

            if (dt.Rows.Count > 0)
            {
                int result =
                    Convert.ToInt32(
                    dt.Rows[0]["Result"]
                    );

                string message =
                    dt.Rows[0]["Message"]
                    .ToString()
                    ?? "Unknown";

                return (
                    result == 1,
                    message
                );
            }

            return (
                false,
                "Unknown error occurred"
            );
        }

        public (bool Success, string Message)
        DeleteProduct(int id)
        {
            Hashtable ht =
                new Hashtable();

            ht.Add(
                "@ProductId",
                id
            );

            DataTable dt =
                _dbHelper.ExecuteStoredProcedure(
                    "USP_DeleteProduct",
                    ht
                );

            if (dt.Rows.Count > 0)
            {
                int result =
                    Convert.ToInt32(
                    dt.Rows[0]["Result"]
                    );

                string message =
                    dt.Rows[0]["Message"]
                    .ToString()
                    ?? "Unknown";

                return (
                    result == 1,
                    message
                );
            }

            return (
                false,
                "Unknown error occurred"
            );
        }

        public List<ProductModel>
        SearchProducts(
            string? productName,
            int? categoryId
        )
        {
            List<ProductModel> products =
                new List<ProductModel>();

            Hashtable ht =
                new Hashtable();

            ht.Add(
                "@ProductName",

                string.IsNullOrEmpty(
                productName
                )

                ? DBNull.Value

                : productName
            );

            ht.Add(
                "@CategoryId",

                categoryId.HasValue

                ? categoryId.Value

                : DBNull.Value
            );

            DataTable dt =
                _dbHelper.ExecuteStoredProcedure(
                    "USP_SearchProducts",
                    ht
                );

            foreach (DataRow row in dt.Rows)
            {
                products.Add(
                    MapToProductModel(
                    row
                    )
                );
            }

            return products;
        }

        private ProductModel
        MapToProductModel(DataRow row)
        {
            return new ProductModel
            {
                ProductId =
                    Convert.ToInt32(
                    row["ProductId"]
                    ),

                ProductName =
                    row["ProductName"]
                    .ToString()
                    ?? string.Empty,

                CategoryId =
                    Convert.ToInt32(
                    row["CategoryId"]
                    ),

                CategoryName =
                    row["CategoryName"]
                    .ToString()
                    ?? string.Empty,

                Price =
                    Convert.ToDecimal(
                    row["Price"]
                    ),

                Quantity =
                    Convert.ToInt32(
                    row["Quantity"]
                    ),

                CreatedDate =
                    Convert.ToDateTime(
                    row["CreatedDate"]
                    )
            };
        }
    }
}