using ECommBackend.DTOs;
using ECommBackend.DTOs.FrontendDTO;
using ECommBackend.DTOs.MapToDomain;
using ECommBackend.Models;
using ECommBackend.Repositories.RepoInterfaces;
using ECommBackend.Services;


namespace ECommBackend.Services;

public class AdminService
{
    private readonly IAdminRepo _adminRepo;
    public AdminService(IAdminRepo _AdminRepo) {
       _adminRepo = _AdminRepo;
    }

    public async Task<IQueryable<AdminDTO>?> GetAllAdmins(CancellationToken ctx) {
      var result = await _adminRepo.GetAllAdmins(ctx);
      return result.Select(x=>x.ModelToRecordDTO());
    }

    public async Task<AdminDTO?> GetSingleAdmin(CancellationToken ctx, Guid _adminId) {
      var result = await _adminRepo.GetSingleAdmin(ctx, _adminId);
        return result.ModelToRecordDTO();
    }
    public async Task DeleteAdmin(CancellationToken ctx, Guid _adminId) {
      await _adminRepo.DeleteAdmin(ctx, _adminId);
    }
    public async Task CreateAdmin(CancellationToken ctx, AdminModel user) {
     await _adminRepo.CreateAdmin(ctx, user);
    }

    // Hashing lives here so no caller can construct an AdminModel around a plaintext password.
    public async Task<AdminModel> RegisterAdmin(CancellationToken ctx, DTOAdmin _newAdmin) {
      var newAdmin = new AdminModel(
          Guid.NewGuid(),
          _newAdmin._FirstName,
          _newAdmin._LastName,
          _newAdmin._Email,
          _newAdmin.age,
          password: PasswordHasher.Hash(_newAdmin.password),
          refreshToken: "");

      await _adminRepo.CreateAdmin(ctx, newAdmin);
      return newAdmin;
    }

    // Null means "these credentials are not valid", deliberately without saying which half was wrong.
    public async Task<AdminModel?> AuthenticateAdmin(CancellationToken ctx, DTOLogin _adminLogin) {
      var admin = await _adminRepo.GetSingleAdmin(ctx, _adminLogin);
      return PasswordHasher.Verify(_adminLogin.password, admin?.Password) ? admin : null;
    }
    //public Task UpdateAdmin(CancellationToken ctx, AdminModel user);
}