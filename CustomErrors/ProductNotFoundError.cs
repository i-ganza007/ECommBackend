namespace ECommBackend.CustomErrors
{
    [Serializable]

    public class ProductNotFoundError:Exception
    {
        public Guid productId { get; set; }

        public ProductNotFoundError() { }

        public ProductNotFoundError(string message):base(message) 
        { 
        }

        public ProductNotFoundError(Guid _productId, string message, Exception InnerException):base(message,InnerException) {
            productId = _productId;
        }
    }
}
