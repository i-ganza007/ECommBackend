using ECommBackend.Models;

namespace ECommBackend.Repositories.RepoInterfaces
{
    public interface IAdminRepo
    {
        public Task<IQueryable<AdminModel>?> GetAllUsers(CancellationToken ctx);

        public Task<AdminModel?> GetSingleUser(CancellationToken ctx, Guid _userId);
        public Task DeleteUser(CancellationToken ctx, Guid _userId);
        public Task CreateUser(CancellationToken ctx, AdminModel user);
        public Task UpdateUser(CancellationToken ctx, AdminModel user);

    }
}
