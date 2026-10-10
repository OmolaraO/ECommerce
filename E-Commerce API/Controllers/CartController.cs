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
    [Authorize]
    public class CartController : ControllerBase
    {
        private readonly ICartService _cartService;

        public CartController(ICartService cartService)
        {
            _cartService = cartService;
        }

        [HttpGet(ApiEndpoints.Cart.Get)]
        [ProducesResponseType(typeof(CartResponse), StatusCodes.Status200OK)]
        public async Task<IActionResult> Get(CancellationToken token)
        {
            var cart = await _cartService.GetAsync(HttpContext.GetUserId()!.Value, token);
            return Ok(cart.MapToResponse());
        }

        [HttpPost(ApiEndpoints.Cart.AddItem)]
        [ProducesResponseType(typeof(CartResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ValidationFailureResponse), StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> AddItem([FromBody] AddCartItemRequest request, CancellationToken token)
        {
            var cart = await _cartService.AddItemAsync(HttpContext.GetUserId()!.Value, request.ProductId, request.Quantity, token);
            return Ok(cart.MapToResponse());
        }

        [HttpPut(ApiEndpoints.Cart.UpdateItem)]
        [ProducesResponseType(typeof(CartResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ValidationFailureResponse), StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> UpdateItem([FromRoute] Guid productId, [FromBody] UpdateCartItemRequest request, CancellationToken token)
        {
            var cart = await _cartService.UpdateItemAsync(HttpContext.GetUserId()!.Value, productId, request.Quantity, token);
            if (cart is null)
            {
                return NotFound();
            }
            return Ok(cart.MapToResponse());
        }

        [HttpDelete(ApiEndpoints.Cart.RemoveItem)]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> RemoveItem([FromRoute] Guid productId, CancellationToken token)
        {
            var removed = await _cartService.RemoveItemAsync(HttpContext.GetUserId()!.Value, productId, token);
            if (!removed)
            {
                return NotFound();
            }
            return NoContent();
        }

        [HttpDelete(ApiEndpoints.Cart.Clear)]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        public async Task<IActionResult> Clear(CancellationToken token)
        {
            await _cartService.ClearAsync(HttpContext.GetUserId()!.Value, token);
            return NoContent();
        }
    }
}
