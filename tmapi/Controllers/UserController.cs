using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using tmapi.Models;

namespace tmapi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UserController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly ILogger<UserController> _logger;

        public UserController(AppDbContext context, ILogger<UserController> logger)
        {
            _context = context;
            _logger = logger;
        }

        // POST: api/user
        [HttpPost]
        public async Task<ActionResult<User>> CreateUser(User user)
        {
            _logger.LogInformation("CreateUser endpoint called.");

            if (user == null || string.IsNullOrEmpty(user.Username))
            {
                _logger.LogWarning("Invalid user data provided.");
                return BadRequest("User data is invalid.");
            }

            try
            {
                user.CreatedOn = DateTime.UtcNow;
                _context.Users.Add(user);
                await _context.SaveChangesAsync();

                _logger.LogInformation("User created successfully with ID {UserId}.", user.Id);
                return CreatedAtAction(nameof(GetUserById), new { id = user.Id }, user);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while creating user.");
                return StatusCode(500, "Internal server error.");
            }
        }

        // GET: api/user
        [HttpGet]
        public async Task<ActionResult<IEnumerable<User>>> GetUsers()
        {
            _logger.LogInformation("GetUsers endpoint called.");

            try
            {
                var users = await _context.Users.ToListAsync();
                _logger.LogInformation("Retrieved {UserCount} users.", users.Count);
                return Ok(users);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while retrieving users.");
                return StatusCode(500, "Internal server error.");
            }
        }

        // GET: api/user/5
        [HttpGet("{id}")]
        public async Task<ActionResult<User>> GetUserById(int id)
        {
            _logger.LogInformation("GetUserById endpoint called with ID {UserId}.", id);

            var user = await _context.Users.FindAsync(id);
            if (user == null)
            {
                _logger.LogWarning("User with ID {UserId} not found.", id);
                return NotFound();
            }

            _logger.LogInformation("User with ID {UserId} retrieved successfully.", id);
            return Ok(user);
        }

        // PUT: api/user/5
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateUser(int id, User user)
        {
            _logger.LogInformation("UpdateUser endpoint called with ID {UserId}.", id);

            if (id != user.Id)
            {
                _logger.LogWarning("User ID mismatch: {UserId} != {RequestId}.", id, user.Id);
                return BadRequest("User ID mismatch.");
            }

            _context.Entry(user).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
                _logger.LogInformation("User with ID {UserId} updated successfully.", id);
                return NoContent();
            }
            catch (DbUpdateConcurrencyException ex)
            {
                if (!UserExists(id))
                {
                    _logger.LogWarning("User with ID {UserId} not found during update.", id);
                    return NotFound();
                }
                else
                {
                    _logger.LogError(ex, "Concurrency error occurred while updating user with ID {UserId}.", id);
                    throw;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while updating user with ID {UserId}.", id);
                return StatusCode(500, "Internal server error.");
            }
        }

        // DELETE: api/user/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteUser(int id)
        {
            _logger.LogInformation("DeleteUser endpoint called with ID {UserId}.", id);

            var user = await _context.Users.FindAsync(id);
            if (user == null)
            {
                _logger.LogWarning("User with ID {UserId} not found.", id);
                return NotFound();
            }

            _context.Users.Remove(user);
            await _context.SaveChangesAsync();

            _logger.LogInformation("User with ID {UserId} deleted successfully.", id);
            return NoContent();
        }

        // GET: api/user/username/{username}
        [HttpGet("username/{username}")]
        public async Task<ActionResult<User>> GetUserByUsername(string username)
        {
            _logger.LogInformation("GetUserByUsername endpoint called with username: {Username}.", username);

            var user = await _context.Users.FirstOrDefaultAsync(u => u.Username == username);
            if (user == null)
            {
                _logger.LogWarning("User with username {Username} not found.", username);
                return NotFound(new { Message = $"User with username '{username}' not found." });
            }

            _logger.LogInformation("User with username {Username} retrieved successfully.", username);
            return Ok(user);
        }

        private bool UserExists(int id)
        {
            var exists = _context.Users.Any(e => e.Id == id);
            _logger.LogInformation("UserExists check for ID {UserId}: {Exists}.", id, exists);
            return exists;
        }
    }
}
