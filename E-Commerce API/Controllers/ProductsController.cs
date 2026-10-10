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
    public class ProductsController : ControllerBase
    {
        private readonly IProductService _productService;

        public ProductsController(IProductService productService)
        {
            _productService = productService;
        }

        [Authorize(AuthConstants.AdminUserPolicyName)]
        [HttpPost(ApiEndpoints.Products.Create)]
        [ProducesResponseType(typeof(ProductResponse), StatusCodes.Status201Created)]
        [ProducesResponseType(typeof(ValidationFailureResponse), StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> Create([FromBody] CreateProductRequest request, CancellationToken token)
        {
            var product = request.MapToProduct();
            await _productService.CreateAsync(product, token);
            return CreatedAtAction(nameof(Get), new { id = product.Id }, product.MapToResponse());
        }

        [AllowAnonymous]
        [HttpGet(ApiEndpoints.Products.Get)]
        [ProducesResponseType(typeof(ProductResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> Get([FromRoute] Guid id, CancellationToken token)
        {
            var product = await _productService.GetByIdAsync(id, token);
            if (product is null)
            {
                return NotFound();
            }
            return Ok(product.MapToResponse());
        }

        [AllowAnonymous]
        [HttpGet(ApiEndpoints.Products.GetAll)]
        [ProducesResponseType(typeof(ProductsResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ValidationFailureResponse), StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> GetAll([FromQuery] GetAllProductsRequest request, CancellationToken token)
        {
            var options = request.MapToOptions();
            var products = await _productService.GetAllAsync(options, token);
            var total = await _productService.GetCountAsync(options, token);
            return Ok(products.MapToResponse(options.Page, options.PageSize, total));
        }

        [Authorize(AuthConstants.AdminUserPolicyName)]
        [HttpPut(ApiEndpoints.Products.Update)]
        [ProducesResponseType(typeof(ProductResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ValidationFailureResponse), StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> Update([FromRoute] Guid id, [FromBody] UpdateProductRequest request, CancellationToken token)
        {
            var updated = await _productService.UpdateAsync(request.MapToProduct(id), token);
            if (updated is null)
            {
                return NotFound();
            }
            return Ok(updated.MapToResponse());
        }

        [Authorize(AuthConstants.AdminUserPolicyName)]
        [HttpDelete(ApiEndpoints.Products.Delete)]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ValidationFailureResponse), StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> Delete([FromRoute] Guid id, CancellationToken token)
        {
            var deleted = await _productService.DeleteByIdAsync(id, token);
            if (!deleted)
            {
                return NotFound();
            }
            return NoContent();
        }
    }
}
