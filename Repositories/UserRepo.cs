using ECommBackend.CustomErrors;
using ECommBackend.DatabaseConns;
using ECommBackend.DTOs.FrontendDTO;
using ECommBackend.Models;
using ECommBackend.Models.ModInterfaces;
using ECommBackend.Repositories.RepoInterfaces;
using Microsoft.EntityFrameworkCore;
namespace ECommBackend.Repositories
{
    public class UserRepo:IUserRepo
    {
        private readonly SQLiteConn _SQLiteConn;
        public UserRepo(SQLiteConn sqliteConn) {
        _SQLiteConn = sqliteConn;
        }

        public async Task<IQueryable<UserModel>?> GetAllUsers(CancellationToken ctx) {
            var result = await _SQLiteConn.Users.ToListAsync(ctx);
            return result.AsQueryable();
        }

        public async Task<UserModel?> GetSingleUser(CancellationToken ctx, Guid _userId) {
            var result = await _SQLiteConn.Users.FirstOrDefaultAsync(x => x.UserId == _userId, ctx);
            if (result == null) {
                throw new UserNotFoundError(_userId,$"{nameof(_userId)} doesn't exist");
            }
            return result;
        }

        // Returns null rather than throwing: an unknown email is a failed login, not a server fault,
        // and the caller must not be able to tell it apart from a wrong password.
        public async Task<UserModel?> GetSingleUser(CancellationToken ctx, DTOLogin _login)
        {
            return await _SQLiteConn.Users.FirstOrDefaultAsync(x => x.Email == _login.email, ctx);
        }
        public async Task DeleteUser(CancellationToken ctx, Guid _userId) {

            var result = await _SQLiteConn.Users.FirstAsync(x => x.UserId == _userId,ctx);
            var result_removed = _SQLiteConn.Users.Remove(result);
            await _SQLiteConn.SaveChangesAsync(ctx);
        }
        public async Task<Guid> CreateUser(CancellationToken ctx, UserModel _user) {
            var result = _SQLiteConn.Users.Add(_user);
            await _SQLiteConn.SaveChangesAsync(ctx);
            return _user.UserId;
        }
        //public Task UpdateUser(CancellationToken ctx, UserModel _user) { }
    }
}
