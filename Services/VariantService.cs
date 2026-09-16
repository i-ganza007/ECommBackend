using ECommBackend.DTOs;
using ECommBackend.DTOs.FrontendDTO;
using ECommBackend.DTOs.MapToDomain;
using ECommBackend.Models;
using ECommBackend.Repositories.RepoInterfaces;

namespace ECommBackend.Services
{
    public class VariantService
    {
        private readonly IVariantRepo _variantRepo;
        public VariantService(IVariantRepo _VariantRepo) { 
          _variantRepo = _VariantRepo;
        }

        public async Task<VariantDTO> GetSingleVariant(Guid _variantId, CancellationToken ctx) {
          var result = await _variantRepo.GetSingleVariant(_variantId, ctx);
          return VariantMapToDomain.ModelToRecordDTO(result);
        }
        public async Task<IQueryable<VariantDTO>?> GetAllVariantsForProduct(Guid _productId, CancellationToken ctx) {
          var result = await _variantRepo.GetAllVariantsForProduct(_productId, ctx);
          return result.Select(x=>VariantMapToDomain.ModelToRecordDTO(x));
        }

        public async Task<Guid> CreateVariantForProduct(Guid _productId, VariantModel createVariantModel, CancellationToken ctx) {
           await _variantRepo.CreateVariantForProduct(_productId, createVariantModel, ctx);
           return createVariantModel.VariantId;
        }

        public async Task UpdateSingleVariant(Guid _variantId, DTOVariant updatedVariant, CancellationToken ctx) {
          await _variantRepo.UpdateSingleVariant(_variantId, updatedVariant._Size, updatedVariant._Price, updatedVariant._Units, ctx);
        }

        public async Task<bool> TryReserveUnits(IReadOnlyCollection<VariantReservation> reservations, CancellationToken ctx) {
          return await _variantRepo.TryReserveUnits(reservations, ctx);
        }

        public async Task DeleteSingleVariant(Guid _variantId, CancellationToken ctx) {
          await _variantRepo.DeleteSingleVariant(_variantId, ctx);
        }

        public async Task DeleteAllVariantsForProduct(Guid _productId, CancellationToken ctx) {
          await _variantRepo.DeleteAllVariantsForProduct(_productId, ctx);
        }
    }
}
