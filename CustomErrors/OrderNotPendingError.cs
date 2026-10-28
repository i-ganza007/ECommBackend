namespace ECommBackend.CustomErrors
{
    public class OrderNotPendingError : Exception
    {
        public Guid OrderId { get; }

        public OrderNotPendingError()
        {
        }

        public OrderNotPendingError(Guid orderId, string message)
            : base(message)
        {
            OrderId = orderId;
        }

        public OrderNotPendingError(Guid orderId, string message, Exception innerException)
            : base(message, innerException)
        {
            OrderId = orderId;
        }
    }
}
