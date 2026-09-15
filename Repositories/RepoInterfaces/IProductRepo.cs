using ECommBackend.Models;

namespace ECommBackend.Repositories.RepoInterfaces
{
    public interface IProductRepo
    {
        public Task<ProductModel?> GetSingleProduct(Guid productId,CancellationToken ctx);
        public Task<IQueryable<ProductModel>?> GetAllProductsByUser(Guid _userId , CancellationToken ctx);

        public Task<List<ProductModel>> GetProductsByIds(IReadOnlyCollection<Guid> productIds, CancellationToken ctx);

        public Task DeleteSingleProduct(Guid productId, CancellationToken ctx);

        public Task<Guid> CreateProduct(ProductModel newProductModel,CancellationToken ctx);

        //public Task UpdateProduct(ProductModel newProductModel,CancellationToken ctx);

        public Task<AdminModel> GetProductOwner(Guid productId,CancellationToken ctx);

    }
}
