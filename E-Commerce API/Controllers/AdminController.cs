using E_Commerce_API.Auth;
using E_Commerce_API.Mapping;
using ECommerce.Application.Services;
using ECommerce.Contracts.Requests;
using ECommerce.Contracts.Responses;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace E_Commerce_API.Controllers
{
    [ApiController]
    [Authorize(AuthConstants.AdminUserPolicyName)]
    public class AdminController : ControllerBase
    {
        private readonly IOrderService _orderService;

        public AdminController(IOrderService orderService)
        {
            _orderService = orderService;
        }

        [HttpGet(ApiEndpoints.Admin.GetAllOrders)]
        [ProducesResponseType(typeof(OrdersResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ValidationFailureResponse), StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> GetAllOrders([FromQuery] string? status, CancellationToken token)
        {
            var parsedStatus = string.IsNullOrWhiteSpace(status)
                ? (ECommerce.Application.Models.OrderStatus?)null
                : ContractMapping.ParseOrderStatus(status, nameof(status));
            var orders = await _orderService.GetAllAsync(parsedStatus, token);
            return Ok(orders.MapToResponse());
        }

        [HttpPut(ApiEndpoints.Admin.UpdateOrderStatus)]
        [ProducesResponseType(typeof(OrderResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ValidationFailureResponse), StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> UpdateOrderStatus([FromRoute] Guid id, [FromBody] UpdateOrderStatusRequest request, CancellationToken token)
        {
            var status = ContractMapping.ParseOrderStatus(request.Status, nameof(request.Status));
            var order = await _orderService.UpdateStatusAsync(id, status, token);
            if (order is null)
            {
                return NotFound();
            }
            return Ok(order.MapToResponse());
        }
    }
}
