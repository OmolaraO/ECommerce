using E_Commerce_API.Mapping;
using ECommerce.Application.Services;
using ECommerce.Contracts.Requests;
using ECommerce.Contracts.Responses;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace E_Commerce_API.Controllers
{
    [ApiController]
    [AllowAnonymous]
    public class AuthController : ControllerBase
    {
        private readonly IAuthService _authService;
        private readonly ITokenService _tokenService;

        public AuthController(IAuthService authService, ITokenService tokenService)
        {
            _authService = authService;
            _tokenService = tokenService;
        }

        [HttpPost(ApiEndpoints.Auth.Register)]
        [ProducesResponseType(typeof(AuthResponse), StatusCodes.Status201Created)]
        [ProducesResponseType(typeof(ValidationFailureResponse), StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> Register([FromBody] RegisterRequest request, CancellationToken token)
        {
            var user = await _authService.RegisterAsync(request.UserName, request.Email, request.Password, token);
            var response = user.MapToResponse(_tokenService.GenerateToken(user));
            return StatusCode(StatusCodes.Status201Created, response);
        }

        [HttpPost(ApiEndpoints.Auth.Login)]
        [ProducesResponseType(typeof(AuthResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> Login([FromBody] LoginRequest request, CancellationToken token)
        {
            var user = await _authService.LoginAsync(request.Email, request.Password, token);
            if (user is null)
            {
                return Unauthorized();
            }
            return Ok(user.MapToResponse(_tokenService.GenerateToken(user)));
        }
    }
}
