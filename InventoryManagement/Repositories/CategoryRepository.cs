using InventoryManagement.Helpers;
using InventoryManagement.Models;
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

        public CategoryRepository(
            IConfiguration configuration
        )
        {
            _dbHelper =
                new DatabaseHelper(
                configuration
                );
        }

        public List<CategoryModel>
        GetAllCategories()
        {
            List<CategoryModel>
            categories =
            new List<CategoryModel>();

            DataTable dt =
            _dbHelper
            .ExecuteStoredProcedure(
            "USP_GetCategories"
            );

            foreach (
            DataRow row
            in dt.Rows
            )
            {
                categories.Add(
                new CategoryModel
                {
                    CategoryId =
                    Convert.ToInt32(
                    row["CategoryId"]
                    ),

                    CategoryName =
                    row["CategoryName"]
                    .ToString()
                    ?? string.Empty,

                    IsActive =
                    Convert.ToBoolean(
                    row["IsActive"]
                    ),

                    CreatedDate =
                    Convert.ToDateTime(
                    row["CreatedDate"]
                    )
                });
            }

            return categories;
        }

        public CategoryModel?
        GetCategoryById(
        int id
        )
        {
            Hashtable ht =
            new Hashtable();

            ht.Add(
            "@CategoryId",
            id
            );

            DataTable dt =
            _dbHelper
            .ExecuteStoredProcedure(
            "USP_GetCategoryById",
            ht
            );

            if (dt.Rows.Count > 0)
            {
                DataRow row =
                dt.Rows[0];

                return new CategoryModel
                {
                    CategoryId =
                    Convert.ToInt32(
                    row["CategoryId"]
                    ),

                    CategoryName =
                    row["CategoryName"]
                    .ToString()
                    ?? string.Empty,

                    IsActive =
                    Convert.ToBoolean(
                    row["IsActive"]
                    ),

                    CreatedDate =
                    Convert.ToDateTime(
                    row["CreatedDate"]
                    )
                };
            }

            return null;
        }

        public (bool Success, string Message)
        InsertCategory(
        CategoryModel category
        )
        {
            Hashtable ht =
            new Hashtable();

            ht.Add(
            "@CategoryName",
            category.CategoryName
            );

            ht.Add(
            "@IsActive",
            category.IsActive
            );

            DataTable dt =
            _dbHelper
            .ExecuteStoredProcedure(
            "USP_InsertCategory",
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
        UpdateCategory(
        CategoryModel category
        )
        {
            Hashtable ht =
            new Hashtable();

            ht.Add(
            "@CategoryId",
            category.CategoryId
            );

            ht.Add(
            "@CategoryName",
            category.CategoryName
            );

            ht.Add(
            "@IsActive",
            category.IsActive
            );

            DataTable dt =
            _dbHelper
            .ExecuteStoredProcedure(
            "USP_UpdateCategory",
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
        DeleteCategory(
        int id
        )
        {
            Hashtable ht =
            new Hashtable();

            ht.Add(
            "@CategoryId",
            id
            );

            DataTable dt =
            _dbHelper
            .ExecuteStoredProcedure(
            "USP_DeleteCategory",
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
    }
}