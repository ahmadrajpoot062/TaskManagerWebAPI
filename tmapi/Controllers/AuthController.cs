using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using tmapi.Services;
using tmapi.Models;

namespace tmapi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly AuthService _authService;
        private readonly ILogger<AuthController> _logger;

        public AuthController(AuthService authService, ILogger<AuthController> logger)
        {
            _authService = authService;
            _logger = logger;
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register(RegisterRequest request)
        {
            _logger.LogInformation("Register endpoint called for username: {Username}.", request.Username);

            try
            {
                var user = await _authService.Register(request);
                _logger.LogInformation("User registered successfully with username: {Username}.", request.Username);
                return Ok(user);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while registering user with username: {Username}.", request.Username);
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login(LoginRequest request)
        {
            _logger.LogInformation("Login endpoint called for username: {Username}.", request.Username);

            try
            {
                var token = await _authService.Login(request);
                _logger.LogInformation("User logged in successfully: {Username}.", request.Username);
                return Ok(new { token });
            }
            catch (Exception ex)
            {
                _logger.LogWarning("Unauthorized login attempt for username: {Username}. Reason: {Message}", request.Username, ex.Message);
                return Unauthorized(new { message = ex.Message });
            }
        }
    }
}
