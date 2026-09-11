using ECommBackend.Models;

namespace ECommBackend.DTOs.MapToDomain
{
    public static class OrderMapToDomain
    {
        public static OrderDTO ModelToRecordDTO(this OrderModel _order) {

            return new OrderDTO(
                _order.OrderId,
                _order.TotalPrice,
                _order.OrderCreator.ModelToRecordDTO(),
                _order.OrderCreatorId,
                _order.Products.Select(x=> x.ModelToRecordDTO()).ToList(),
                _order.CreatedDate,
                _order.OrderStatus
                );
        }

        //public static OrderDTO ModelToRecordDTOExtension(this OrderModel _order) {
        
        //}
    }
}
