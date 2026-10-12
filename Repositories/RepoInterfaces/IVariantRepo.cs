using ECommBackend.Models;

namespace ECommBackend.Repositories.RepoInterfaces
{
    // How many units of one variant an order wants taken off the shelf.
    public record VariantReservation(Guid VariantId, int Quantity);

    public interface IVariantRepo
    {
        public Task<VariantModel> GetSingleVariant(Guid _variantId ,CancellationToken ctx);
        public Task<IQueryable<VariantModel>?> GetAllVariantsForProduct(Guid _productId, CancellationToken ctx);

        public Task<Guid> CreateVariantForProduct(Guid _productId,VariantModel createVariantModel ,CancellationToken ctx);

        public Task UpdateSingleVariant(Guid _variantId, double size, decimal price, int units, CancellationToken ctx);

        // All-or-nothing stock decrement for a whole order. Returns false without
        // touching anything when any variant is missing or short on stock, so a
        // caller never has to unwind a half-applied reservation.
        public Task<bool> TryReserveUnits(IReadOnlyCollection<VariantReservation> reservations, CancellationToken ctx);

        public Task DeleteSingleVariant(Guid _variantId,CancellationToken ctx);

        public Task DeleteAllVariantsForProduct(Guid _productId, CancellationToken ctx);

       
    }
}
