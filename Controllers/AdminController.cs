using ECommBackend.DTOs.FrontendDTO;
using ECommBackend.Models;
using ECommBackend.Repositories.RepoInterfaces;
using ECommBackend.Services.IJWTServices;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace ECommBackend.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AdminController : ControllerBase
    {
        private readonly IAdminRepo _adminRepo;
        private readonly IJWTService _jwtService;
        public AdminController(IAdminRepo adminRepo, IJWTService jwtService)
        {
            _adminRepo  = adminRepo;
            _jwtService = jwtService;
        }

        [HttpGet("all")]
        public async Task<IActionResult> GetAllAdmins( CancellationToken ctx)
        {
            var result = await _adminRepo.GetAllAdmins(ctx);
            return Ok(result);
        }

        [HttpGet("{adminId}", Name = "singleadmin")]
        public async Task<IActionResult> GetSingleAdmin(string adminId, CancellationToken ctx)
        {
            var result = await _adminRepo.GetSingleAdmin(ctx, Guid.Parse(adminId));
            return Ok(result);
        }

        [HttpDelete("{adminId}")]
        public async Task<IActionResult> DeleteAdmin(string adminId, CancellationToken ctx)
        {
            await _adminRepo.DeleteAdmin(ctx, Guid.Parse(adminId));
            return Ok();
        }

        //[HttpPost("auth/login")]
        //public Task<IActionResult> LoginUser(DTOLogin _userLogin) { }

        [HttpPost]
        public async Task<IActionResult> CreateAdmin(DTOAdmin _dtoadmin, CancellationToken ctx)
        {
            Guid guserId = Guid.NewGuid();
            string hashpassword = BCrypt.Net.BCrypt.HashPassword(_dtoadmin.password, workFactor: 12);
            ICollection<ProductModel> productsOwned  = new List<ProductModel>();
           var res = await _adminRepo.CreateAdmin(ctx, new AdminModel(guserId, _dtoadmin._FirstName, _dtoadmin._LastName, _dtoadmin._Email, _dtoadmin.age, refreshToken: "", password: hashpassword) { ProductsOwned=productsOwned });
            return CreatedAtRoute("singleadmin", new { adminId = guserId }, value: res);
        }

    }
}
