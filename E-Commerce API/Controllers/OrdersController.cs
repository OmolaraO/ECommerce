using E_Commerce_API.Auth;
using E_Commerce_API.Mapping;
using ECommerce.Application.Models;
using ECommerce.Application.Services;
using ECommerce.Contracts.Responses;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace E_Commerce_API.Controllers
{
    [ApiController]
    [Authorize]
    public class OrdersController : ControllerBase
    {
        private readonly IOrderService _orderService;

        public OrdersController(IOrderService orderService)
        {
            _orderService = orderService;
        }

        [HttpPost(ApiEndpoints.Orders.Checkout)]
        [ProducesResponseType(typeof(OrderResponse), StatusCodes.Status201Created)]
        [ProducesResponseType(typeof(ValidationFailureResponse), StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> Checkout(CancellationToken token)
        {
            var order = await _orderService.CheckoutAsync(HttpContext.GetUserId()!.Value, token);
            return CreatedAtAction(nameof(Get), new { id = order.Id }, order.MapToResponse());
        }

        [HttpGet(ApiEndpoints.Orders.GetMine)]
        [ProducesResponseType(typeof(OrdersResponse), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetMine(CancellationToken token)
        {
            var orders = await _orderService.GetByUserIdAsync(HttpContext.GetUserId()!.Value, token);
            return Ok(orders.MapToResponse());
        }

        [HttpGet(ApiEndpoints.Orders.Get)]
        [ProducesResponseType(typeof(OrderResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> Get([FromRoute] Guid id, CancellationToken token)
        {
            var order = await GetVisibleOrderAsync(id, token);
            if (order is null)
            {
                return NotFound();
            }
            return Ok(order.MapToResponse());
        }

        [HttpPost(ApiEndpoints.Orders.Cancel)]
        [ProducesResponseType(typeof(OrderResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ValidationFailureResponse), StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> Cancel([FromRoute] Guid id, CancellationToken token)
        {
            if (await GetVisibleOrderAsync(id, token) is null)
            {
                return NotFound();
            }
            var order = await _orderService.UpdateStatusAsync(id, OrderStatus.Cancelled, token);
            if (order is null)
            {
                return NotFound();
            }
            return Ok(order.MapToResponse());
        }

        // Customers only see their own orders; another user's order id returns 404, not 403, so ids can't be probed
        private async Task<Order?> GetVisibleOrderAsync(Guid id, CancellationToken token)
        {
            var order = await _orderService.GetByIdAsync(id, token);
            if (order is null || (order.UserId != HttpContext.GetUserId() && !HttpContext.IsAdmin()))
            {
                return null;
            }
            return order;
        }
    }
}
