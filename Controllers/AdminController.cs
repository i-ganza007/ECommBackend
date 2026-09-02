using ECommBackend.Repositories.RepoInterfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace ECommBackend.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AdminController : ControllerBase
    {
        private readonly IAdminRepo _adminRepo;
        public AdminController(IAdminRepo adminRepo)
        {
            _adminRepo = adminRepo;
        }

        //[HttpGet("all")]
        //public async Task<IActionResult> GetAllUsers([FromHeader(Name = "loggedInUserId")] string userId, CancellationToken ctx)
        //{
        //    var result = await _userRepo.GetAllUsers(ctx);
        //    return Ok(result);
        //}

        //[HttpGet("singleUser")]
        //public async Task<IActionResult> GetSingleUser([FromHeader(Name = "loggedInUserId")] string userId, CancellationToken ctx)
        //{
        //    var result = await _userRepo.GetSingleUser(ctx, Guid.Parse(userId));
        //    return Ok(result);
        //}

        //[HttpDelete]
        //public async Task<IActionResult> DeleteUser([FromHeader(Name = "loggedInUserId")] string userId, CancellationToken ctx)
        //{
        //    await _userRepo.DeleteUser(ctx, Guid.Parse(userId));
        //    return Ok();
        //}

        //[HttpPost]
        //public async Task<IActionResult> CreateResult()
    }
}
