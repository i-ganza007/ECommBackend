using ECommBackend.Models;

namespace ECommBackend.Repositories.RepoInterfaces
{
    public interface IUserRepo
    {
        public Task<IQueryable<UserModel>?> GetAllUsers(CancellationToken ctx);

        public Task<UserModel?> GetSingleUser(CancellationToken ctx,Guid _userId);
        public Task DeleteUser(CancellationToken ctx,Guid _userId);
        public Task<Guid> CreateUser(CancellationToken ctx,UserModel _user);
        //public Task UpdateUser(CancellationToken ctx,UserModel _user);

    }

}
