using ECommBackend.CustomErrors;
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
                throw new VariantNotFoundError(_variantId,$"{nameof(_variantId)} doesn't exist as variant ");
            }
            return result;
        
        }
        public async Task<IQueryable<VariantModel>?> GetAllVariantsForProduct(Guid _productId, CancellationToken ctx) {
            var result = await _SQLiteConn.Variants.ToListAsync(ctx);
            return result.AsQueryable();
        }

        public async Task<Guid> CreateVariantForProduct(Guid _productId, VariantModel createVariantModel, CancellationToken ctx) {
           var result = await _SQLiteConn.Variants.FirstOrDefaultAsync(x=>x.ProductModelId == _productId);
            if (result == null)
            {
                throw new ProductNotFoundError($"{nameof(_productId)} doesn't exist as variant ");
            }
            var result_add = await _SQLiteConn.Variants.AddAsync(createVariantModel, ctx);
            await _SQLiteConn.SaveChangesAsync(ctx);
            return createVariantModel.VariantId;
        }

        public async Task UpdateSingleVariant(Guid _variantId, double size, decimal price, int units, CancellationToken ctx) {
            var result = await _SQLiteConn.Variants.FirstOrDefaultAsync(x => x.VariantId == _variantId, ctx);
            if (result == null)
            {
                throw new VariantNotFoundError(_variantId, $"{nameof(_variantId)} doesn't exist as variant ");
            }

            // Image and owning product are deliberately not reassignable here.
            result.Size = size;
            result.Price = price;
            result.Units = units;

            await _SQLiteConn.SaveChangesAsync(ctx);
        }

        public async Task DeleteSingleVariant(Guid _variantId, CancellationToken ctx) {
            var result = await _SQLiteConn.Variants.FirstOrDefaultAsync(x => x.VariantId == _variantId);
            if (result == null)
            {
                throw new VariantNotFoundError(_variantId,$"{nameof(_variantId)} doesn't exist as variant ");
            }
            var removed = _SQLiteConn.Variants.Remove(result);
            await _SQLiteConn.SaveChangesAsync(ctx);
        }

        public async Task DeleteAllVariantsForProduct(Guid _productId, CancellationToken ctx) {
            var check = await _SQLiteConn.Products.FirstOrDefaultAsync(x=>x.ProductId == _productId);
            if (check == null)
            {
                throw new ProductNotFoundError($"{_productId} doesn't exist ");
            }
            //var variants_where =_SQLiteConn.Variants.Where(x => x.ProductModelId == _productId);
            //var result_first = await Task.FromResult<IEnumerable<VariantModel>>(_SQLiteConn.Variants.Where(x => x.ProductModelId == _productId));
            var result_first = await _SQLiteConn.Variants.Where(x => x.ProductModelId == _productId).ToListAsync(ctx);

            _SQLiteConn.Variants.RemoveRange(result_first);

            await _SQLiteConn.SaveChangesAsync(ctx);
        }
    }
}
