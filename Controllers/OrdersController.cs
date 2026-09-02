using ECommBackend.Repositories.RepoInterfaces;
using Microsoft.AspNetCore.Mvc;

namespace ECommBackend.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class OrdersController : ControllerBase
    {
        private readonly IOrderRepo _orderRepo;
        public OrdersController(IOrderRepo orderRepo) {
         _orderRepo = orderRepo;
        }

        [HttpGet("all")]
        public async Task<IActionResult> GetAllOrders([FromHeader(Name ="loggedInUserId")] string userId,CancellationToken ctx) {
          var result = await _orderRepo.GetAllOrders(Guid.Parse(userId),ctx);
          return Ok(result);
        }

        [HttpGet("{orderId}")]
        public async Task<IActionResult> GetSingleOrder(string orderId,CancellationToken ctx) {
         var result  = await _orderRepo.GetSingleOrder(Guid.Parse(orderId),ctx);
         return Ok(result);
        }

        //[HttpPost]
        //public async Task<IActionResult> CreateProduct(CancellationToken ctx)
        //{
        //    va
        //}
    }
}
