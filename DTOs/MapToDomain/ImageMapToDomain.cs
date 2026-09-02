using ECommBackend.Models;

namespace ECommBackend.DTOs.MapToDomain
{
    public static class ImageMapToDomain
    {
        public static ImageDTO ModelToRecordDTO(ImageModel _imageModel)
        {
            return new ImageDTO(
                _imageModel.ImageId,
                _imageModel.BytesArray,
                _imageModel.BytesSize
                );
        }
    }
}
