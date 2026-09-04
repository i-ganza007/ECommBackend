using ECommBackend.Models;

namespace ECommBackend.Repositories.RepoInterfaces
{
    public interface IImageRepo
    {
        public Task<ImageModel> GetSingleImage(Guid _imageId, CancellationToken ctx);
        public Task<IEnumerable<ImageModel>?> GetAllImages(CancellationToken ctx);
        public Task UploadImage(ImageModel _imageModel,Guid _variantId ,CancellationToken ctx);

        public Task DeleteImage(Guid _imageId, CancellationToken ctx);
        public Task UpdateImage(ImageModel _imageModel, Guid _imageId, CancellationToken ctx);

        //public Task<IEnumerable<ImageModel>?> GetAllImagesFromProduct(Guid _productId, CancellationToken ctx);
    }
}
