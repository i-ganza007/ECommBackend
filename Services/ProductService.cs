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
           return result.ModelToRecordDTO();
        }
        public async Task<IQueryable<ProductDTO>?> GetAllProductsByUser(Guid _userId, CancellationToken ctx) { 
           var result = await productRepo.GetAllProductsByUser(_userId, ctx);
           return result.Select(x=>x.ModelToRecordDTO());
        }

        public async Task DeleteSingleProduct(Guid productId, CancellationToken ctx) {
            await productRepo.DeleteSingleProduct(productId, ctx);
        }

        public async Task<Guid> CreateProduct(ProductModel newProductModel, CancellationToken ctx) {
           return await productRepo.CreateProduct(newProductModel, ctx);
        }

        //public Task UpdateProduct(ProductModel newProductModel,CancellationToken ctx);

        public async Task<AdminDTO> GetProductOwner(Guid productId, CancellationToken ctx) {
           var result = await productRepo.GetProductOwner(productId, ctx);
          return result.ModelToRecordDTO();
        }
    }
}
