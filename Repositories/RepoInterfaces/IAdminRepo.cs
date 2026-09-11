using ECommBackend.Models;

namespace ECommBackend.Repositories.RepoInterfaces
{
    public interface IAdminRepo
    {
        public Task<IQueryable<AdminModel>?> GetAllAdmins(CancellationToken ctx);

        public Task<AdminModel?> GetSingleAdmin(CancellationToken ctx, Guid _adminId);
        public Task DeleteAdmin(CancellationToken ctx, Guid _adminId);
        public Task<Guid> CreateAdmin(CancellationToken ctx, AdminModel user);
        //public Task UpdateAdmin(CancellationToken ctx, AdminModel user);

    }
}
