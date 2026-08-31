using System;
using System.ComponentModel.DataAnnotations;

namespace ECommBackend.Models
{
    public class ImageModel
    {
        [Key]
        public Guid ImageId { get; set; }

        public byte[] BytesArray { get; set; } = [];

        public int BytesSize { get; set; }

        public ImageModel()
        {
        }

        public ImageModel(Guid imageId, byte[] bytesArray)
        {
            ImageId = imageId;
            BytesArray = bytesArray;
            BytesSize = bytesArray.Length;
        }
    }
}
