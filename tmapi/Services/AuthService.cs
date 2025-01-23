using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using tmapi.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace tmapi.Services
{
    public class AuthService
    {
        private readonly AppDbContext _context;
        private readonly ILogger<AuthService> _logger;

        public AuthService(AppDbContext context, ILogger<AuthService> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<User> Register(RegisterRequest request)
        {
            _logger.LogInformation("Register method called for username: {Username}", request.Username);

            try
            {
                if (await _context.Users.AnyAsync(u => u.Username == request.Username))
                {
                    _logger.LogWarning("Registration failed: Username '{Username}' already exists.", request.Username);
                    throw new Exception("Username already exists.");
                }

                var user = new User
                {
                    Username = request.Username,
                    FirstName = request.FirstName,
                    LastName = request.LastName,
                    CreatedOn = DateTime.UtcNow,
                    PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.Password)
                };

                _context.Users.Add(user);
                await _context.SaveChangesAsync();

                _logger.LogInformation("User '{Username}' registered successfully with ID {UserId}.", request.Username, user.Id);
                return user;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred during user registration for username: {Username}.", request.Username);
                throw;
            }
        }

        public async Task<string> Login(LoginRequest request)
        {
            _logger.LogInformation("Login method called for username: {Username}", request.Username);

            try
            {
                var user = await _context.Users.FirstOrDefaultAsync(u => u.Username == request.Username);

                if (user == null || !BCrypt.Net.BCrypt.Verify(request.Password, user.PasswordHash))
                {
                    _logger.LogWarning("Login failed: Invalid credentials for username: {Username}.", request.Username);
                    throw new Exception("Invalid username or password.");
                }

                var token = GenerateJwtToken(user);
                _logger.LogInformation("Login successful for username: {Username}. JWT token generated.", request.Username);
                return token;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred during login for username: {Username}.", request.Username);
                throw;
            }
        }

        private string GenerateJwtToken(User user)
        {
            _logger.LogInformation("Generating JWT token for username: {Username}.", user.Username);

            try
            {
                var tokenHandler = new JwtSecurityTokenHandler();
                var key = Encoding.ASCII.GetBytes("V2lU1YJvErGyyv9frPq33DE+K7HoD/9gD+zQj9e2uAo="); // Replace with a strong key
                var tokenDescriptor = new SecurityTokenDescriptor
                {
                    Subject = new ClaimsIdentity(new[]
                    {
                        new Claim(ClaimTypes.Name, user.Username)
                    }),
                    Expires = DateTime.UtcNow.AddDays(7),
                    SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
                };

                var token = tokenHandler.CreateToken(tokenDescriptor);
                _logger.LogInformation("JWT token generated successfully for username: {Username}.", user.Username);
                return tokenHandler.WriteToken(token);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while generating JWT token for username: {Username}.", user.Username);
                throw;
            }
        }
    }
}
