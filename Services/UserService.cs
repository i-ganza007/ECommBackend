using ECommBackend.DTOs;
using ECommBackend.DTOs.MapToDomain;
using ECommBackend.Models;
using ECommBackend.Repositories.RepoInterfaces;

namespace ECommBackend.Services
{
    public class UserService
    {
        private readonly IUserRepo _userRepo;
        public UserService(IUserRepo _UserRepo) {
          _userRepo = _UserRepo;
        }

        public async Task<IEnumerable<UserDTO>?> GetAllUsers(CancellationToken ctx) {
         var result = await _userRepo.GetAllUsers(ctx);
         return result.Select(x=>UserMapToDomain.ModelToRecordDTO(x));
        }

        public async Task<UserDTO?> GetSingleUser(CancellationToken ctx, Guid _userId) {
         var result = await _userRepo.GetSingleUser(ctx, _userId);
            return UserMapToDomain.ModelToRecordDTO(result);
        }
        public async Task DeleteUser(CancellationToken ctx, Guid _userId) {
           await _userRepo.DeleteUser(ctx, _userId);
        }
        public async Task CreateUser(CancellationToken ctx, UserModel _user) {
          await _userRepo.CreateUser(ctx, _user);
        }
        //public Task UpdateUser(CancellationToken ctx,UserModel _user);

    }
}
