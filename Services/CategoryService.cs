using ECommBackend.DTOs;
using ECommBackend.DTOs.MapToDomain;
using ECommBackend.Models;
using ECommBackend.Repositories.RepoInterfaces;

namespace ECommBackend.Services
{
    public class CategoryService
    {
        private readonly ICategory _category;
        public CategoryService(ICategory category)
        {
            _category = category;
        }

        public async Task<IQueryable<CategoryDTO>?> GetAllCategories(CancellationToken ctx) {
          var result = await _category.GetAllCategories(ctx);
          return result?.Select(x => x.ModelToRecordDTO());
        }

        public async Task<CategoryDTO?> GetSingleCategory(Guid _categoryId, CancellationToken ctx) {
          var result = await _category.GetSingleCategory(ctx, _categoryId);
          return result?.ModelToRecordDTO();
        }

        public async Task<Guid> CreateCategory(CategoryModel newCategory, CancellationToken ctx) {
          return await _category.CreateCategory(ctx, newCategory);
        }

        public async Task DeleteCategory(Guid _categoryId, CancellationToken ctx) {
          await _category.DeleteCategory(ctx, _categoryId);
        }
    }
}
