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
        public double TotalPrice { get; set; }

        public Guid OrderCreatorId { get; set; }
        public UserModel OrderCreator { get; set; }

        public ICollection<ProductModel> Products { get; init; }
        public DateTime CreatedDate { get; set; }
        public OrderStatus OrderStatus { get; set; }
    }
}
