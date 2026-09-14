using ECommBackend.Models;
namespace ECommBackend.DTOs.MapToDomain
{
    public static class CategoryMapToDomain
    {
        public static CategoryDTO ModelToRecordDTO(this CategoryModel categoryModel)
        {
            return new CategoryDTO(categoryModel.CategoryId, categoryModel.CategoryName, categoryModel.CategoryDescription, categoryModel.ParentCategoryId, categoryModel.ParentCategory.ModelToRecordDTO(), categoryModel.ChildCategories.Select(x => x.ModelToRecordDTO()), categoryModel.Products.Select(x => x.ModelToRecordDTO()));
                
        }
    }
}
