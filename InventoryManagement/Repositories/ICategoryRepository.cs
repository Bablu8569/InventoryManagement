using System.Collections.Generic;
using InventoryManagement.Models;

namespace InventoryManagement.Repositories
{
    public interface ICategoryRepository
    {
        List<CategoryModel> GetAllCategories();
        CategoryModel? GetCategoryById(int id);
        (bool Success, string Message) InsertCategory(CategoryModel category);
        (bool Success, string Message) UpdateCategory(CategoryModel category);
        (bool Success, string Message) DeleteCategory(int id);
    }
}