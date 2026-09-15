using ECommBackend.DTOs;
using ECommBackend.DTOs.FrontendDTO;
using ECommBackend.DTOs.MapToDomain;
using ECommBackend.Models;
using ECommBackend.Repositories.RepoInterfaces;
using static ECommBackend.Services.PasswordHasher;

namespace ECommBackend.Services
{
    public class UserService
    {
        private readonly IUserRepo _userRepo;
        public UserService(IUserRepo _UserRepo) {
          _userRepo = _UserRepo;
        }

        public async Task<IQueryable<UserDTO>?> GetAllUsers(CancellationToken ctx) {
         var result = await _userRepo.GetAllUsers(ctx);
         return result.Select(x=>x.ModelToRecordDTO());
        }


        public async Task<UserDTO?> GetSingleUser(CancellationToken ctx, Guid _userId) {
         var result = await _userRepo.GetSingleUser(ctx, _userId);
            return result.ModelToRecordDTO();
        }

        public async Task<UserDTO?> GetSingleUser(CancellationToken ctx, DTOLogin _userLogin)
        {
            var result = await _userRepo.GetSingleUser(ctx, _userLogin);
            return result.ModelToRecordDTO();
        }
        public async Task DeleteUser(CancellationToken ctx, Guid _userId) {
           await _userRepo.DeleteUser(ctx, _userId);
        }
        public async Task CreateUser(CancellationToken ctx, UserModel _user) {
          await _userRepo.CreateUser(ctx, _user);
        }

        // Hashing lives here so no caller can construct a UserModel around a plaintext password.
        public async Task<UserModel> RegisterUser(CancellationToken ctx, DTOUser _newUser) {
          var newUser = new UserModel(
              Guid.NewGuid(),
              _newUser._FirstName,
              _newUser._LastName,
              _newUser._Email,
              _newUser.age,
              password: Hash(_newUser.password),
              refreshToken: "");

          await _userRepo.CreateUser(ctx, newUser);
          return newUser;
        }

        // Null means "these credentials are not valid", deliberately without saying which half was wrong.
        public async Task<UserModel?> AuthenticateUser(CancellationToken ctx, DTOLogin _userLogin) {
          var user = await _userRepo.GetSingleUser(ctx, _userLogin);
          return Verify(_userLogin.password, user?.Password) ? user : null;
        }
        //public Task UpdateUser(CancellationToken ctx,UserModel _user);

    }
}
