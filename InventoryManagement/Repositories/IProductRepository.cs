using System.Collections.Generic;
using InventoryManagement.Models;

namespace InventoryManagement.Repositories
{
    public interface IProductRepository
    {
        List<ProductModel> GetAllProducts();
        ProductModel? GetProductById(int id);
        (bool Success, string Message) InsertProduct(ProductModel product);
        (bool Success, string Message) UpdateProduct(ProductModel product);
        (bool Success, string Message) DeleteProduct(int id);
        List<ProductModel> SearchProducts(string? productName, int? categoryId);
    }
}