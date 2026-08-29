namespace ECommBackend.CustomErrors
{
    public class OrderNotFoundException : Exception
    {
        public Guid OrderId { get; }

        public OrderNotFoundException()
        {
        }

        public OrderNotFoundException(string message)
            : base(message)
        {
        }

        public OrderNotFoundException(Guid orderId, string message)
            : base(message)
        {
            OrderId = orderId;
        }

        public OrderNotFoundException(
            string message,
            Guid _orderId,
            Exception innerException)
            : base(message, innerException)
        {
            OrderId = _orderId;
        }
    }
}