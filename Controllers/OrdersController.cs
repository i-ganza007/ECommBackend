using ECommBackend.CustomErrors;
using ECommBackend.DTOs.FrontendDTO;
using ECommBackend.Models;
using ECommBackend.Repositories.RepoInterfaces;
using ECommBackend.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

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


        [HttpGet("{orderId}/status")]
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

        [Authorize(Policy = "users")]
        [HttpPost("{orderId}/cancel")]
        public async Task<IActionResult> CancelOrder(string orderId, CancellationToken ctx)
        {
            if (!Guid.TryParse(orderId, out var parsedOrderId))
            {
                return BadRequest($"'{orderId}' is not a valid order id");
            }

            // Ownership is checked against the token, not a header the caller controls.
            if (!Guid.TryParse(User.FindFirstValue(ClaimTypes.NameIdentifier), out var userId))
            {
                return Unauthorized();
            }

            try
            {
                var order = await _orderService.CancelOrder(parsedOrderId, userId, ctx);
                return Ok(order);
            }
            catch (OrderNotFoundException ex)
            {
                return NotFound(ex.Message);
            }
            catch (OrderNotPendingError ex)
            {
                return Conflict(ex.Message);
            }
        }
    }
}
