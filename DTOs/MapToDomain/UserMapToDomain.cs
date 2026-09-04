using ECommBackend.Models;

namespace ECommBackend.DTOs.MapToDomain
{
    public static class UserMapToDomain
    {
        public static ProductDTO ConvertToProductDTO( ProductModel userProduct) {
            return ProductMapToDomain.ModelToRecordDTO(userProduct);
        }
        public static UserDTO ModelToRecordDTO(UserModel _user)
        {
            return new UserDTO(
                _user.UserId,
                _user.FirstName,
                _user.LastName,
                _user.Email,
                _user.ProductsBought.Select(x=>UserMapToDomain.ConvertToProductDTO(x)).ToList(),
                _user.CreatedDate,
                _user.Orders.Select(x=> OrderMapToDomain.ModelToRecordDTO(x)).ToList()
                );
        }
    }
}
