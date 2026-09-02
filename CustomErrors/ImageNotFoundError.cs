namespace ECommBackend.CustomErrors
{
    [Serializable]
    public class ImageNotFoundError : Exception
    {
        public Guid ImageId { get; set; }
        public ImageNotFoundError()
        {

        }

        public ImageNotFoundError(Guid _imageId, string message) : base(message)
        {
            ImageId = _imageId;
        }

        public ImageNotFoundError(string message, Guid _imageId, Exception InnerException) : base(message, InnerException)
        {
            ImageId = _imageId;
        }
    }
}
