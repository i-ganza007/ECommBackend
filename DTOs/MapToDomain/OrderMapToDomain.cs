using ECommBackend.Models;

namespace ECommBackend.DTOs.MapToDomain
{
    public static class OrderMapToDomain
    {
        public static OrderDTO ModelToRecordDTO(OrderModel _order) {

            return new OrderDTO(
                _order.OrderId,
                _order.TotalPrice,
                UserMapToDomain.ModelToRecordDTO(_order.OrderCreator),
                _order.OrderCreatorId,
                _order.Products.Select(x=> ProductMapToDomain.ModelToRecordDTO(x)).ToList(),
                _order.CreatedDate,
                _order.OrderStatus
                );
        }

        //public static OrderDTO ModelToRecordDTOExtension(this OrderModel _order) {
        
        //}
    }
}
