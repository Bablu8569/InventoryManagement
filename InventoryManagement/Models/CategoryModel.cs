using InventoryManagement.Helpers;
using System.Collections;
using System.ComponentModel.DataAnnotations;
using System.Data;

namespace InventoryManagement.Models
{
    public class CategoryModel
    {
        public int CategoryId { get; set; }

        [Required(ErrorMessage = "Category name is required")]
        public string CategoryName { get; set; } = string.Empty;

        public bool IsActive { get; set; } = true;

        public DateTime CreatedDate { get; set; } = DateTime.Now;

        public string Description { get; set; } = string.Empty;



        // ================= GET ALL =================

        public static List<CategoryModel> GetAll(
            DatabaseHelper db,
            string? search = null
        )
        {
            List<CategoryModel> list = new();

            DataTable dt;

            if (string.IsNullOrEmpty(search))
            {
                dt = db.ExecuteStoredProcedure(
                    "sp_GetAllCategories"
                );
            }
            else
            {
                Hashtable ht = new();

                ht.Add("@Search", search);

                dt = db.ExecuteStoredProcedure(
                    "sp_GetCategoriesBySearch",
                    ht
                );
            }

            foreach (DataRow row in dt.Rows)
            {
                list.Add(new CategoryModel
                {
                    CategoryId = Convert.ToInt32(row["CategoryId"]  ),

                    CategoryName =
                        row["CategoryName"]
                        .ToString() ?? "",

                    Description =
                        row["Description"]
                        .ToString() ?? "",

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

            return list;
        }



        // ================= GET BY ID =================

        public static CategoryModel? GetById(
            DatabaseHelper db,
            int id
        )
        {
            Hashtable ht = new();

            ht.Add(
                "@CategoryId",
                id
            );

            DataTable dt =
                db.ExecuteStoredProcedure(
                    "USP_GetCategoryById",
                    ht
                );

            if (dt.Rows.Count == 0)
                return null;

            DataRow row = dt.Rows[0];

            return new CategoryModel
            {
                CategoryId =
                    Convert.ToInt32(
                        row["CategoryId"]
                    ),

                CategoryName =
                    row["CategoryName"]
                    .ToString() ?? "",

                Description =
                    row["Description"]
                    .ToString() ?? "",

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



        // ================= INSERT =================

        public string Insert(
            DatabaseHelper db
        )
        {
            Hashtable ht = new();

            ht.Add(
                "@CategoryName",
                CategoryName
            );

            ht.Add(
                "@Description",
                Description
            );

            DataTable dt =
                db.ExecuteStoredProcedure(
                    "sp_InsertCategory",
                    ht
                );

            return dt.Rows[0]["Message"]
                .ToString() ?? "";
        }



        // ================= UPDATE =================

        public string Update(
            DatabaseHelper db
        )
        {
            Hashtable ht = new();

            ht.Add(
                "@CategoryId",
                CategoryId
            );

            ht.Add(
                "@CategoryName",
                CategoryName
            );

            ht.Add(
                "@Description",
                Description
            );

            ht.Add(
                "@IsActive",
                IsActive
            );

            DataTable dt =
                db.ExecuteStoredProcedure(
                    "sp_UpdateCategory",
                    ht
                );

            return dt.Rows[0]["Message"]
                .ToString() ?? "";
        }



        // ================= DELETE =================

        public static string Delete(
            DatabaseHelper db,
            int id
        )
        {
            Hashtable ht = new();

            ht.Add(
                "@CategoryId",
                id
            );

            DataTable dt =
                db.ExecuteStoredProcedure(
                    "USP_DeleteCategory",
                    ht
                );

            return dt.Rows[0]["Message"]
                .ToString() ?? "";
        }
    }
}