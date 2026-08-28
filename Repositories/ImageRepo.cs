using ECommBackend.DatabaseConns;
using ECommBackend.Models;
using ECommBackend.Repositories.RepoInterfaces;
using Microsoft.EntityFrameworkCore;

namespace ECommBackend.Repositories
{
    public class ImageRepo:IImageRepo
    {
        private readonly SQLiteConn _SQLiteConn;
        public ImageRepo(SQLiteConn SQLiteConn)
        {
            _SQLiteConn = SQLiteConn;

        }
        public async Task<ImageModel> GetSingleImage(Guid _imageId, CancellationToken ctx) {
          var result = await _SQLiteConn.Images.FirstOrDefaultAsync(x=>x.ImageId == _imageId,ctx);
            if (result == null)
            {
                throw new KeyNotFoundException($"{nameof(_imageId)} doesn't exist as image ");
            }
            return result;
        }

        public async Task<IEnumerable<ImageModel>?> GetAllImages(CancellationToken ctx) { 
          var result = await _SQLiteConn.Images.ToListAsync(ctx);
          return result;
        }

        public async Task UploadImage(ImageModel _imageModel, Guid _variantId,CancellationToken ctx) {
            var result = await _SQLiteConn.Variants.FirstOrDefaultAsync(x => x.VariantId == _variantId);
            if (result == null)
            {
                throw new KeyNotFoundException($"{nameof(_variantId)} doesn't exist as variant ");
            }
            var result_add = await _SQLiteConn.Images.AddAsync(_imageModel, ctx);
            await _SQLiteConn.SaveChangesAsync(ctx);

        }

        public async Task DeleteImage(Guid _imageId, CancellationToken ctx) { }
        public async Task UpdateImage(ImageModel _imageModel, Guid _imageId, CancellationToken ctx) { }

        //public async Task<IEnumerable<ImageModel>?> GetAllImagesFromProduct(Guid _productId, CancellationToken ctx) {
        //    var result = await _SQLiteConn.Products.FirstOrDefaultAsync(_ => _.ProductId == _productId);
        //    if (result == null)
        //    {
        //        throw new KeyNotFoundException($"{_productId} doesn't exist on the product");
        //    }
        //}
    }
}
