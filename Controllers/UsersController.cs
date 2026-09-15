using ECommBackend.DTOs.FrontendDTO;
using ECommBackend.DTOs.MapToDomain;
using ECommBackend.Models;
using ECommBackend.Repositories;
using ECommBackend.Repositories.RepoInterfaces;
using ECommBackend.Services;
using ECommBackend.Services.IJWTServices;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;


namespace ECommBackend.Controllers;

[Route("api/[controller]")]
[ApiController]
[Authorize(Policy = "users")]
public class UsersController : ControllerBase
{
    private readonly UserService _userService;
    private readonly IJWTService _jwtService;
    public UsersController(UserService userService, IJWTService jwtService)
    {
        _userService = userService;
        _jwtService = jwtService;
    }

    [HttpGet("all")]
    public async Task<IActionResult> GetAllUsers([FromHeader(Name = "loggedInUserId")] string userId, CancellationToken ctx)
    {
        var result = await _userService.GetAllUsers(ctx);
        return Ok(result);
    }

    [HttpGet("{userId}",Name ="singleuser")]
    public async Task<IActionResult> GetSingleUser(string userId, CancellationToken ctx)
    {
        var result = await _userService.GetSingleUser(ctx, Guid.Parse(userId));
        return Ok(result);
    }

    [HttpDelete("{userId}")]
    public async Task<IActionResult> DeleteUser(string userId, CancellationToken ctx)
    {
        await _userService.DeleteUser(ctx, Guid.Parse(userId));
        return Ok();
    }

    [AllowAnonymous]
    [HttpPost("auth/login")]
    public async Task<IActionResult> LoginUser(DTOLogin _userLogin, CancellationToken ctx) {
        var user = await _userService.AuthenticateUser(ctx, _userLogin);
        if (user is null)
        {
            // Same answer for an unknown email and a wrong password, so neither can be probed for.
            return Unauthorized("Invalid email or password");
        }

        IssueAccessToken(user, Roles.logged_in);
        return Ok(user.ModelToRecordDTO());
    }

    [AllowAnonymous]
    [HttpPost]
    public async Task<IActionResult>   CreateUser(DTOUser _dtouser,CancellationToken ctx)
    {
        var userMod = await _userService.RegisterUser(ctx, _dtouser);

        IssueAccessToken(userMod, Roles.logged_in);
        return CreatedAtRoute("singleuser",new {userId=userMod.UserId},value: userMod.ModelToRecordDTO());
    }

    private void IssueAccessToken(UserModel user, Roles role)
    {
        Response.Cookies.Append("access_token", _jwtService.GenerateAccessToken(user, role), new CookieOptions
        {
            HttpOnly = true,
            Secure = true,
            SameSite = SameSiteMode.Strict,
        });
    }

}


