namespace ECommBackend.CustomErrors
{
    public class VariantNotFoundError : Exception
    {
        public Guid VariantId { get; set; }
        public VariantNotFoundError()
        {

        }

        public VariantNotFoundError(Guid _variantId, string message) : base(message)
        {
            VariantId = _variantId;
        }

        public VariantNotFoundError(string message,Guid _variantId ,Exception InnerException) : base(message, InnerException)
        {
            VariantId = _variantId;
        }
    }
}
