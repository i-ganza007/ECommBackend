using ECommBackend.DTOs.FrontendDTO;
using ECommBackend.DTOs.MapToDomain;
using ECommBackend.Models;
using ECommBackend.Services;
using ECommBackend.Services.IJWTServices;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ECommBackend.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(Policy = "adminUsers")]
    public class AdminController : ControllerBase
    {
        private readonly AdminService _adminService;
        private readonly IJWTService _jwtService;
        public AdminController(AdminService adminService, IJWTService jwtService)
        {
            _adminService  = adminService;
            _jwtService = jwtService;
        }

        [HttpGet("all")]
        public async Task<IActionResult> GetAllAdmins( CancellationToken ctx)
        {
            var result = await _adminService.GetAllAdmins(ctx);
            return Ok(result);
        }

        [HttpGet("{adminId}", Name = "singleadmin")]
        public async Task<IActionResult> GetSingleAdmin(string adminId, CancellationToken ctx)
        {
            var result = await _adminService.GetSingleAdmin(ctx, Guid.Parse(adminId));
            return Ok(result);
        }

        [HttpDelete("{adminId}")]
        public async Task<IActionResult> DeleteAdmin(string adminId, CancellationToken ctx)
        {
            await _adminService.DeleteAdmin(ctx, Guid.Parse(adminId));
            return Ok();
        }

        [AllowAnonymous]
        [HttpPost("auth/login")]
        public async Task<IActionResult> LoginAdmin(DTOLogin _adminLogin, CancellationToken ctx)
        {
            var admin = await _adminService.AuthenticateAdmin(ctx, _adminLogin);
            if (admin is null)
            {
                // Same answer for an unknown email and a wrong password, so neither can be probed for.
                return Unauthorized("Invalid email or password");
            }

            IssueAccessToken(admin);
            return Ok(admin.ModelToRecordDTO());
        }

        [AllowAnonymous]
        [HttpPost]
        public async Task<IActionResult> CreateAdmin(DTOAdmin _dtoadmin, CancellationToken ctx)
        {
            var admin = await _adminService.RegisterAdmin(ctx, _dtoadmin);

            IssueAccessToken(admin);
            return CreatedAtRoute("singleadmin", new { adminId = admin.UserId }, value: admin.ModelToRecordDTO());
        }

        private void IssueAccessToken(AdminModel admin)
        {
            Response.Cookies.Append("access_token", _jwtService.GenerateAccessToken(admin, Roles.admin), new CookieOptions
            {
                HttpOnly = true,
                Secure = true,
                SameSite = SameSiteMode.Strict,
            });
        }

    }
}
