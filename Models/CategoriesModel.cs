using System.ComponentModel.DataAnnotations;
namespace ECommBackend.Models
{
    public class CategoryModel
    {
        [Key]
        public Guid CategoryId { get; set; }

        [Required]
        public string CategoryName { get; set; }

        [Required]
        public string CategoryDescription { get; set; }

        public Guid? ParentCategoryId { get; set; }

        public CategoryModel? ParentCategory { get; set; }

        public IEnumerable<CategoryModel> ChildCategories { get; set; } = new List<CategoryModel>();

        public IEnumerable<ProductModel> Products { get; set; }  = new List<ProductModel>();

        public CategoryModel(Guid categoryId,string categoryName,string categoryDescription)
        {
            CategoryId = categoryId;
            CategoryName = categoryName;
            CategoryDescription = categoryDescription;
        }

        public CategoryModel(Guid categoryId, string categoryName, string categoryDescription,Guid parentCategoryId) :this(categoryId,  categoryName,  categoryDescription)
        {
           ParentCategoryId = parentCategoryId;   
        }
    }
}
