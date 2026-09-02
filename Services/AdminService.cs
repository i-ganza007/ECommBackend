using ECommBackend.DTOs;
using ECommBackend.DTOs.MapToDomain;
using ECommBackend.Models;
using ECommBackend.Repositories.RepoInterfaces;

namespace ECommBackend.Services;

public class AdminService
{
    private readonly IAdminRepo _adminRepo;
    public AdminService(IAdminRepo _AdminRepo) {
       _adminRepo = _AdminRepo;
    }

    public async Task<IEnumerable<AdminDTO>?> GetAllAdmins(CancellationToken ctx) {
      var result = await _adminRepo.GetAllAdmins(ctx);
      return result.Select(x=>AdminMapToDomain.ModelToRecordDTO(x));
    }

    public async Task<AdminDTO?> GetSingleAdmin(CancellationToken ctx, Guid _adminId) {
      var result = await _adminRepo.GetSingleAdmin(ctx, _adminId);
        return AdminMapToDomain.ModelToRecordDTO(result);
    }
    public async Task DeleteAdmin(CancellationToken ctx, Guid _adminId) {
      await _adminRepo.DeleteAdmin(ctx, _adminId);
    }
    public async Task CreateAdmin(CancellationToken ctx, AdminModel user) {
     await _adminRepo.CreateAdmin(ctx, user);
    }
    //public Task UpdateAdmin(CancellationToken ctx, AdminModel user);
}