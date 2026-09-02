using ECommBackend.DatabaseConns;
using ECommBackend.Models;
using ECommBackend.Models.ModInterfaces;
using ECommBackend.Repositories.RepoInterfaces;
using Microsoft.EntityFrameworkCore;

namespace ECommBackend.Repositories
{
    public class AdminRepo:IAdminRepo
    {
        private readonly SQLiteConn _SQLiteConn;
        public AdminRepo(SQLiteConn sqliteConn)
        {
            _SQLiteConn = sqliteConn;
        }

        public async Task<IEnumerable<AdminModel>?> GetAllAdmins(CancellationToken ctx)
        {
            //var result = await _SQLiteConn.Admins.ToListAsync(ctx);
            var result = await _SQLiteConn.Admins.ToListAsync(ctx);
            return result;
        }

        public async Task<AdminModel?> GetSingleAdmin(CancellationToken ctx, Guid _userId)
        {
            var result = await _SQLiteConn.Admins.FirstAsync(x => x.UserId == _userId);
            if (result == null)
            {
                throw new KeyNotFoundException($"{nameof(_userId)} doesn't exist");
            }
            return result;
        }
        public async Task DeleteAdmin(CancellationToken ctx, Guid _userId)
        {

            var result = await _SQLiteConn.Admins.FirstAsync(x => x.UserId == _userId, ctx);
            var result_removed = _SQLiteConn.Admins.Remove(result);
            await _SQLiteConn.SaveChangesAsync(ctx);
        }
        public async Task CreateAdmin(CancellationToken ctx, AdminModel _admin)
        {
            var result = _SQLiteConn.Admins.Add(_admin);
            await _SQLiteConn.SaveChangesAsync(ctx);
        }
    }
}
