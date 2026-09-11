using ECommBackend.DatabaseConns;
using ECommBackend.Models;
using ECommBackend.Repositories.RepoInterfaces;
using Microsoft.EntityFrameworkCore;
using ECommBackend.CustomErrors;

namespace ECommBackend.Repositories
{
    public class CategoriesRepo:ICategory
    {
        private readonly SQLiteConn _conn;
        public CategoriesRepo(SQLiteConn conn)
        {
            conn = _conn;
        }
        public async Task<IQueryable<CategoryModel>?> GetAllCategories(CancellationToken ctx) {
            
            var result = await _conn.Categories.ToListAsync(ctx);
            
            return result.AsQueryable();
        }

        public async Task<CategoryModel?> GetSingleCategory(CancellationToken ctx, Guid _categoryId) {
           var result = await _conn.Categories.FirstOrDefaultAsync(x=>x.CategoryId == _categoryId,ctx);
            if(result == null)
            {
                throw new CategoryNotFound(_categoryId, $"{nameof(_categoryId)} cannot be found in the current context");
            }
            return result;
        }
        public async Task DeleteCategory(CancellationToken ctx, Guid _categoryId) {
            var result = await _conn.Categories.FirstOrDefaultAsync(x => x.CategoryId == _categoryId, ctx);
            if (result == null)
            {
                throw new CategoryNotFound(_categoryId, $"{nameof(_categoryId)} cannot be found in the current context");
            }
            var removed = _conn.Categories.Remove(result);
            await _conn.SaveChangesAsync();
        }
        //public Task CreateCategory(CancellationToken ctx, AdminModel user) { }
        //public Task UpdateAdmin(CancellationToken ctx, AdminModel user);
    }
}
