using ECommBackend.Models;

namespace ECommBackend.Repositories.RepoInterfaces
{
    public interface ICategory
    {
        public Task<IQueryable<CategoryModel>?> GetAllCategories(CancellationToken ctx);

        public Task<CategoryModel?> GetSingleCategory(CancellationToken ctx, Guid _adminId);
        public Task DeleteCategory(CancellationToken ctx, Guid _adminId);

        public Task<Guid> CreateCategory(CancellationToken ctx, CategoryModel newCategory);
    }
}
