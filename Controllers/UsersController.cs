using BCrypt.Net;
using ECommBackend.DTOs.FrontendDTO;
using ECommBackend.Models;
using ECommBackend.Repositories;
using ECommBackend.Repositories.RepoInterfaces;
using ECommBackend.Services.IJWTServices;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using System.Reflection.Metadata.Ecma335;


namespace ECommBackend.Controllers;

[Route("api/[controller]")]
[ApiController]
public class UsersController : ControllerBase
{
    private readonly IUserRepo _userRepo;
    private readonly IJWTService _jwtService;
    public UsersController(IUserRepo userRepo, IJWTService jwtService)
    {
        _userRepo = userRepo;
        _jwtService = jwtService;
    }

    [HttpGet("all")]
    public async Task<IActionResult> GetAllUsers([FromHeader(Name = "loggedInUserId")] string userId, CancellationToken ctx)
    {
        var result = await _userRepo.GetAllUsers(ctx);
        return Ok(result);
    }

    [HttpGet("{userId}",Name ="singleuser")]
    public async Task<IActionResult> GetSingleUser(string userId, CancellationToken ctx)
    {
        var result = await _userRepo.GetSingleUser(ctx, Guid.Parse(userId));
        return Ok(result);
    }

    [HttpDelete("{userId}")]
    public async Task<IActionResult> DeleteUser(string userId, CancellationToken ctx)
    {
        await _userRepo.DeleteUser(ctx, Guid.Parse(userId));
        return Ok();
    }

    //[HttpPost("auth/login")]
    //public Task<IActionResult> LoginUser(DTOLogin _userLogin) { }

    [HttpPost]
    public async Task<IActionResult>   CreateUser(DTOUser _dtouser,CancellationToken ctx)   
    {
        Guid guserId = Guid.NewGuid();
        string hashpassword = BCrypt.Net.BCrypt.HashPassword(_dtouser.password, workFactor: 12);
        ICollection<OrderModel> orderMade  = new List<OrderModel>();
        ICollection<ProductModel> productBought = new List<ProductModel>();
        var res = await _userRepo.CreateUser(ctx, new UserModel(guserId, _dtouser._FirstName, _dtouser._LastName, _dtouser._Email, _dtouser.age, refreshToken:"",password:hashpassword) { ProductsBought=productBought,Orders=orderMade});
        return CreatedAtRoute("singleuser",new {userId=guserId},value:  res);
    }

}


