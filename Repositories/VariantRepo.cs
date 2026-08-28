using ECommBackend.DatabaseConns;
using ECommBackend.Models;
using ECommBackend.Repositories.RepoInterfaces;
using Microsoft.EntityFrameworkCore;

namespace ECommBackend.Repositories
{
    public class VariantRepo: IVariantRepo
    {
        private readonly SQLiteConn _SQLiteConn;
        public VariantRepo(SQLiteConn sqliteConn)
        {
            _SQLiteConn = sqliteConn;
        }
        public async Task<VariantModel> GetSingleVariant(Guid _variantId, CancellationToken ctx) {

            var result = await _SQLiteConn.Variants.FirstOrDefaultAsync(x => x.VariantId == _variantId);
            if (result == null) {
                throw new KeyNotFoundException($"{nameof(_variantId)} doesn't exist as variant ");
            }
            return result;
        
        }
        public async Task<IEnumerable<VariantModel>?> GetAllVariantsForProduct(Guid _productId, CancellationToken ctx) {
            var result = await _SQLiteConn.Variants.ToListAsync(ctx);
            return result;
        }

        public async Task CreateVariantForProduct(Guid _productId, VariantModel createVariantModel, CancellationToken ctx) {
           var result = await _SQLiteConn.Variants.FirstOrDefaultAsync(x=>x.ProductModelId == _productId);
            if (result == null)
            {
                throw new KeyNotFoundException($"{nameof(_productId)} doesn't exist as variant ");
            }
            var result_add = await _SQLiteConn.Variants.AddAsync(createVariantModel, ctx);
            await _SQLiteConn.SaveChangesAsync(ctx);
        }

        public async Task UpdateSingleVariant(Guid _variantId, CancellationToken ctx) { }

        public async Task DeleteSingleVariant(Guid _variantId, CancellationToken ctx) {
            var result = await _SQLiteConn.Variants.FirstOrDefaultAsync(x => x.VariantId == _variantId);
            if (result == null)
            {
                throw new KeyNotFoundException($"{nameof(_variantId)} doesn't exist as variant ");
            }
            var removed = _SQLiteConn.Variants.Remove(result);
            await _SQLiteConn.SaveChangesAsync(ctx);
        }

        public async Task DeleteAllVariantsForProduct(Guid _productId, CancellationToken ctx) {
            var check = await _SQLiteConn.Products.FindAsync(x => x.ProductId == _productId);
            if (check == null)
            {
                throw new KeyNotFoundException($"{_productId} doesn't exist ");
            }
            //var variants_where =_SQLiteConn.Variants.Where(x => x.ProductModelId == _productId);
            //var result_first = await Task.FromResult<IEnumerable<VariantModel>>(_SQLiteConn.Variants.Where(x => x.ProductModelId == _productId));
            var result_first = await _SQLiteConn.Variants.Where(x => x.ProductModelId == _productId).ToListAsync(ctx);

            _SQLiteConn.Variants.RemoveRange(result_first);

            await _SQLiteConn.SaveChangesAsync(ctx);
        }
    }
}
