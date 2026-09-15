using ECommBackend.Models;

namespace ECommBackend.DTOs
{
    public record CategoryDTO(Guid _CategoryId,string _CategoryName,string _CategoryDescription,Guid? _ParentCategoryId,CategoryDTO? _ParentCategory, IEnumerable<CategoryDTO> _ChildCategories, IEnumerable<ProductDTO> _Products)
    {
    }
}
