namespace ECommBackend.DTOs.FrontendDTO
{
    public record DTOCategory(string _CategoryName, string _CategoryDescription, Guid? _ParentCategoryId, CategoryDTO? _ParentCategory, IEnumerable<DTOCategory> _ChildCategories, IEnumerable<DTOProduct> _Products)
    {
    }
}
