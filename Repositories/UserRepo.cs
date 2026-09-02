using ECommBackend.DatabaseConns;
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

        public async Task<IEnumerable<UserModel>?> GetAllUsers(CancellationToken ctx) {
            var result = await _SQLiteConn.Users.ToListAsync(ctx);
            return result;
        }

        public async Task<UserModel?> GetSingleUser(CancellationToken ctx, Guid _userId) {
            var result = await _SQLiteConn.Users.FirstAsync(x => x.UserId == _userId);
            if (result == null) {
                throw new KeyNotFoundException($"{nameof(_userId)} doesn't exist");
            }
            return result;
        }
        public async Task DeleteUser(CancellationToken ctx, Guid _userId) {

            var result = await _SQLiteConn.Users.FirstAsync(x => x.UserId == _userId,ctx);
            var result_removed = _SQLiteConn.Users.Remove(result);
            await _SQLiteConn.SaveChangesAsync(ctx);
        }
        public async Task CreateUser(CancellationToken ctx, UserModel _user) {
            var result = _SQLiteConn.Users.Add(_user);
            await _SQLiteConn.SaveChangesAsync(ctx);
        }
        //public Task UpdateUser(CancellationToken ctx, UserModel _user) { }
    }
}
