using ECommBackend.Models;

namespace ECommBackend.Repositories.RepoInterfaces
{
    public interface IVariantRepo
    {
        public Task<VariantModel> GetSingleVariant(Guid _variantId ,CancellationToken ctx);
        public Task<IQueryable<VariantModel>?> GetAllVariantsForProduct(Guid _productId, CancellationToken ctx);

        public Task<Guid> CreateVariantForProduct(Guid _productId,VariantModel createVariantModel ,CancellationToken ctx);

        public Task UpdateSingleVariant(Guid _variantId, CancellationToken ctx);

        public Task DeleteSingleVariant(Guid _variantId,CancellationToken ctx);

        public Task DeleteAllVariantsForProduct(Guid _productId, CancellationToken ctx);

       
    }
}
