using ECommBackend.CustomErrors;
using ECommBackend.DatabaseConns;
using ECommBackend.DTOs.FrontendDTO;
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

        public async Task<IQueryable<AdminModel>?> GetAllAdmins(CancellationToken ctx)
        {
            //var result = await _SQLiteConn.Admins.ToListAsync(ctx);
            var result = await _SQLiteConn.Admins.ToListAsync(ctx);
            return result.AsQueryable();
        }

        public async Task<AdminModel?> GetSingleAdmin(CancellationToken ctx, Guid _userId)
        {
            var result = await _SQLiteConn.Admins.FirstOrDefaultAsync(x => x.UserId == _userId, ctx);
            if (result == null)
            {
                throw new UserNotFoundError(_userId,$"{nameof(_userId)} doesn't exist");
            }
            return result;
        }

        // Returns null rather than throwing: an unknown email is a failed login, not a server fault,
        // and the caller must not be able to tell it apart from a wrong password.
        public async Task<AdminModel?> GetSingleAdmin(CancellationToken ctx, DTOLogin _login)
        {
            return await _SQLiteConn.Admins.FirstOrDefaultAsync(x => x.Email == _login.email, ctx);
        }
        public async Task DeleteAdmin(CancellationToken ctx, Guid _userId)
        {

            var result = await _SQLiteConn.Admins.FirstAsync(x => x.UserId == _userId, ctx);
            var result_removed = _SQLiteConn.Admins.Remove(result);
            await _SQLiteConn.SaveChangesAsync(ctx);
        }
        public async Task<Guid> CreateAdmin(CancellationToken ctx, AdminModel _admin)
        {
            var result = _SQLiteConn.Admins.Add(_admin);
            await _SQLiteConn.SaveChangesAsync(ctx);
            return _admin.UserId;
        }
    }
}
