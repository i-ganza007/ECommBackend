using ECommBackend.DTOs;
using ECommBackend.DTOs.MapToDomain;
using ECommBackend.Models;
using ECommBackend.Repositories.RepoInterfaces;

namespace ECommBackend.Services
{
    public class ImageService
    {
        private readonly IImageRepo _imageRepo;
        public ImageService(IImageRepo _ImageRepo) {
            _imageRepo = _ImageRepo;
        }

        public async Task<ImageDTO> GetSingleImage(Guid _imageId, CancellationToken ctx) { 
           var result = await _imageRepo.GetSingleImage(_imageId, ctx);
            return ImageMapToDomain.ModelToRecordDTO(result);
        }
        public async Task<IEnumerable<ImageDTO>?> GetAllImages(CancellationToken ctx) {
            var result = await _imageRepo.GetAllImages(ctx);
            return result.Select(x => ImageMapToDomain.ModelToRecordDTO(x));
        }
        public async Task UploadImage(ImageModel _imageModel, Guid _variantId, CancellationToken ctx) {
          await _imageRepo.UploadImage(_imageModel, _variantId, ctx);
        }

        public async Task DeleteImage(Guid _imageId, CancellationToken ctx) {
            await _imageRepo.DeleteImage(_imageId,ctx);
        }
        public async Task UpdateImage(ImageModel _imageModel, Guid _imageId, CancellationToken ctx) { }
    }
}
