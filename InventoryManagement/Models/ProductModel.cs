using InventoryManagement.Helpers;
using System.Collections;
using System.ComponentModel.DataAnnotations;
using System.Data;

namespace InventoryManagement.Models
{
    public class ProductModel
    {
        public int ProductId { get; set; }

        [Required(ErrorMessage = "Product name is required")]
        public string ProductName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Category is required")]
        public int CategoryId { get; set; }

        public string CategoryName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Price is required")]
        [Range(0.01, double.MaxValue,
            ErrorMessage = "Price must be greater than 0")]
        public decimal Price { get; set; }

        [Required(ErrorMessage = "Quantity is required")]
        [Range(0, int.MaxValue,
            ErrorMessage = "Quantity cannot be negative")]
        public int Quantity { get; set; }

        public DateTime CreatedDate { get; set; } = DateTime.Now;



        // ================= GET ALL =================

        public static List<ProductModel> GetAll(
            DatabaseHelper db,
            string? search = null
        )
        {
            List<ProductModel> list = new();

            DataTable dt =
                db.ExecuteStoredProcedure(
                    "USP_GetProducts"
                );

            foreach (DataRow row in dt.Rows)
            {
                string productName =
                    row["ProductName"]
                    .ToString() ?? "";

                if (!string.IsNullOrEmpty(search))
                {
                    if (!productName
                        .ToLower()
                        .Contains(
                            search.ToLower()
                        ))
                    {
                        continue;
                    }
                }

                list.Add(new ProductModel
                {
                    ProductId =
                        Convert.ToInt32(
                            row["ProductId"]
                        ),

                    ProductName =
                        productName,

                    CategoryId =
                        Convert.ToInt32(
                            row["CategoryId"]
                        ),

                    CategoryName =
                        row["CategoryName"]
                        .ToString() ?? "",

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
                });
            }

            return list;
        }



        // ================= GET BY ID =================

        public static ProductModel? GetById(
            DatabaseHelper db,
            int id
        )
        {
            Hashtable ht = new();

            ht.Add(
                "@ProductId",
                id
            );

            DataTable dt =
                db.ExecuteStoredProcedure(
                    "USP_GetProductById",
                    ht
                );

            if (dt.Rows.Count == 0)
            {
                return null;
            }

            DataRow row =
                dt.Rows[0];

            return new ProductModel
            {
                ProductId =
                    Convert.ToInt32(
                        row["ProductId"]
                    ),

                ProductName =
                    row["ProductName"]
                    .ToString() ?? "",

                CategoryId =
                    Convert.ToInt32(
                        row["CategoryId"]
                    ),

                CategoryName =
                    row["CategoryName"]
                    .ToString() ?? "",

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



        // ================= INSERT =================

        public string Insert(
            DatabaseHelper db
        )
        {
            Hashtable ht =
                new Hashtable();

            ht.Add(
                "@ProductName",
                ProductName
            );

            ht.Add(
                "@CategoryId",
                CategoryId
            );

            ht.Add(
                "@Price",
                Price
            );

            ht.Add(
                "@Quantity",
                Quantity
            );

            DataTable dt =
                db.ExecuteStoredProcedure(
                    "USP_InsertProduct",
                    ht
                );

            return dt.Rows[0]
                ["Message"]
                .ToString() ?? "";
        }



        // ================= UPDATE =================

        public string Update(
            DatabaseHelper db
        )
        {
            Hashtable ht =
                new Hashtable();

            ht.Add(
                "@ProductId",
                ProductId
            );

            ht.Add(
                "@ProductName",
                ProductName
            );

            ht.Add(
                "@CategoryId",
                CategoryId
            );

            ht.Add(
                "@Price",
                Price
            );

            ht.Add(
                "@Quantity",
                Quantity
            );

            DataTable dt =
                db.ExecuteStoredProcedure(
                    "USP_UpdateProduct",
                    ht
                );

            return dt.Rows[0]
                ["Message"]
                .ToString() ?? "";
        }



        // ================= DELETE =================

        public static string Delete(
            DatabaseHelper db,
            int id
        )
        {
            Hashtable ht =
                new Hashtable();

            ht.Add(
                "@ProductId",
                id
            );

            DataTable dt =
                db.ExecuteStoredProcedure(
                    "USP_DeleteProduct",
                    ht
                );

            return dt.Rows[0]
                ["Message"]
                .ToString() ?? "";
        }
    }
}