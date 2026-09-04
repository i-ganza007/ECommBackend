using ECommBackend.DTOs;
using ECommBackend.DTOs.MapToDomain;
using ECommBackend.Models;
using ECommBackend.Repositories.RepoInterfaces;

namespace ECommBackend.Services
{
    public class ProductService
    {
        private readonly IProductRepo productRepo;
        public ProductService(IProductRepo _productRepo)
        {
            productRepo = _productRepo;
        }

        public async Task<ProductDTO?> GetSingleProduct(Guid productId, CancellationToken ctx) {
           var result = await productRepo.GetSingleProduct(productId, ctx);
           return ProductMapToDomain.ModelToRecordDTO(result);
        }
        public async Task<IEnumerable<ProductDTO>?> GetAllProductsByUser(Guid _userId, CancellationToken ctx) { 
           var result = await productRepo.GetAllProductsByUser(_userId, ctx);
           return result.Select(x=>ProductMapToDomain.ModelToRecordDTO(x));
        }

        public async Task DeleteSingleProduct(Guid productId, CancellationToken ctx) {
            await productRepo.DeleteSingleProduct(productId, ctx);
        }

        public async Task CreateProduct(ProductModel newProductModel, CancellationToken ctx) { }

        //public Task UpdateProduct(ProductModel newProductModel,CancellationToken ctx);

        public async Task<AdminDTO> GetProductOwner(Guid productId, CancellationToken ctx) {
           var result = await productRepo.GetProductOwner(productId, ctx);
          return AdminMapToDomain.ModelToRecordDTO(result);
        }
    }
}
