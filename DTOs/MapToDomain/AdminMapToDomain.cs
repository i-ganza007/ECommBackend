using ECommBackend.Models;

namespace ECommBackend.DTOs.MapToDomain
{
    public static class AdminMapToDomain
    {
        public static AdminDTO ModelToRecordDTO(this AdminModel _admin)
        {
            return new AdminDTO(
                 _admin.UserId,
                _admin.FirstName,
                _admin.LastName,
                _admin.Email,
                _admin.ProductsOwned.Select(x=> x.ModelToRecordDTO()).ToList(),
                _admin.CreatedDate
                );
        }
    }
}
