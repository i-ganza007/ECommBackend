namespace ECommBackend.Models.ModInterfaces
{
    public enum OrderStatus
    {
        Pending,
        Processed,
        Rejected
    }
    public interface IOrder
    {
        public Guid OrderId { get; set; }
        public decimal TotalPrice { get; set; }
        public UserModel OrderCreator { get; init; }

        public ProductModel[] Products { get; init; }
        public DateTime CreatedDate { get; set; }
        public OrderStatus OrderStatus { get; set; }
    }
}
