using ECommBackend.CustomErrors;
using ECommBackend.DTOs.FrontendDTO;
using ECommBackend.Models;
using ECommBackend.Repositories.RepoInterfaces;
using ECommBackend.Services;
using Microsoft.AspNetCore.Mvc;

namespace ECommBackend.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class OrdersController : ControllerBase
    {
        private readonly IOrderRepo _orderRepo;
        private readonly OrderService _orderService;
        public OrdersController(IOrderRepo orderRepo, OrderService orderService) {
         _orderRepo = orderRepo;
         _orderService = orderService;
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


        [HttpGet("{orderId}")]
        public async Task<IActionResult> GetOrderStatus(string orderId, CancellationToken ctx)
        {
            var result = await _orderRepo.GetSingleOrder(Guid.Parse(orderId), ctx);
            return Ok(result.OrderStatus);
        }

        [HttpPost]
        public async Task<IActionResult> CreateOrder([FromHeader(Name = "loggedInUserId")] string userId, DTOrder _order, CancellationToken ctx)
        {
            // The caller owns the order, not whoever the body claims — _order._OrderCreatorId is ignored.
            if (!Guid.TryParse(userId, out var orderCreatorId))
            {
                return BadRequest($"'{userId}' is not a valid user id");
            }

            try
            {
                var orderId = await _orderService.CreateOrder(_order, orderCreatorId, ctx);
                return CreatedAtAction(nameof(GetSingleOrder), new { orderId }, orderId);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(ex.Message);
            }
            catch (ProductNotFoundError ex)
            {
                return NotFound(ex.Message);
            }
            catch (VariantNotFoundError ex)
            {
                return Conflict(ex.Message);
            }
        }
    }
}
