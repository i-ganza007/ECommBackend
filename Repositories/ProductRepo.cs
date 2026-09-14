using ECommBackend.CustomErrors;
using ECommBackend.DatabaseConns;
using ECommBackend.Models;
using ECommBackend.Repositories.RepoInterfaces;
using Microsoft.EntityFrameworkCore;

namespace ECommBackend.Repositories
{
    public class ProductRepo:IProductRepo
    {
        private readonly SQLiteConn _SQLiteConn;
        public ProductRepo(SQLiteConn sqliteConn)
        {
            _SQLiteConn = sqliteConn;
        }


        public async Task<ProductModel?> GetSingleProduct(Guid productId, CancellationToken ctx) {
            var result = await _SQLiteConn.Products.FirstAsync(x=>x.ProductId == productId);
            if (result == null)
            {
                throw new KeyNotFoundException($"{nameof(productId)} doesn't exist");
            }
            return result;
        }
        public async Task<IQueryable<ProductModel>?> GetAllProductsByUser(Guid _userId, CancellationToken ctx) {
            var result = await _SQLiteConn.Products.ToListAsync(ctx);
                
            return result.AsQueryable();

        }

        public async Task DeleteSingleProduct(Guid productId, CancellationToken ctx) {

            var result = await _SQLiteConn.Products.FirstAsync(x => x.ProductId == productId, ctx);
            var result_removed = _SQLiteConn.Products.Remove(result);
            await _SQLiteConn.SaveChangesAsync(ctx);


        }

        public async Task<Guid> CreateProduct(ProductModel newProductModel, CancellationToken ctx) {
            var result = _SQLiteConn.Products.Add(newProductModel);
            await _SQLiteConn.SaveChangesAsync(ctx);
            return newProductModel.ProductId;
        }

        //public Task UpdateProduct(ProductModel newProductModel,CancellationToken ctx);

        public async Task<AdminModel> GetProductOwner(Guid productId, CancellationToken ctx) {
            var result = await _SQLiteConn.Products.FirstOrDefaultAsync(x => x.AdminOwnerId == productId,ctx);
            if(result == null)
            {
                throw new UserNotFoundError(productId,$"{nameof(productId)} doesn't exist");
            }
            return result.Owner;
        
        }







    }
}
