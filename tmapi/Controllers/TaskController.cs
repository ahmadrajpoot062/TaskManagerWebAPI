using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using tmapi.Models;

namespace tmapi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class TaskController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly ILogger<TaskController> _logger;

        public TaskController(AppDbContext context, ILogger<TaskController> logger)
        {
            _context = context;
            _logger = logger;
        }

        // POST: api/task
        [HttpPost]
        public async Task<ActionResult<tmapi.Models.Task>> CreateTask(tmapi.Models.Task task)
        {
            if (task == null)
            {
                _logger.LogWarning("Attempt to create a task with null data.");
                return BadRequest("Task data is null.");
            }

            task.CreatedAt = DateTime.UtcNow;
            _context.Tasks.Add(task);
            await _context.SaveChangesAsync();

            _logger.LogInformation("Task with ID {TaskId} created successfully.", task.Id);

            return CreatedAtAction(nameof(GetTask), new { id = task.Id }, task);
        }

        // GET: api/task
        [HttpGet]
        public async Task<ActionResult<IEnumerable<tmapi.Models.Task>>> GetTasks()
        {
            var tasks = await _context.Tasks.ToListAsync();
            _logger.LogInformation("Fetched {TaskCount} tasks from the database.", tasks.Count);

            return Ok(tasks);
        }

        // GET: api/task/username/{username}
        [HttpGet("username/{username}")]
        public async Task<ActionResult<IEnumerable<tmapi.Models.Task>>> GetTasksByUsername(string username)
        {
            var tasks = await _context.Tasks.Where(t => t.CreatedBy == username).ToListAsync();

            if (tasks == null || !tasks.Any())
            {
                _logger.LogWarning("No tasks found for user: {Username}.", username);
                return NotFound($"No tasks found for user: {username}");
            }

            _logger.LogInformation("{TaskCount} tasks found for user: {Username}.", tasks.Count, username);
            return Ok(tasks);
        }

        // GET: api/task/5
        [HttpGet("{id}")]
        public async Task<ActionResult<tmapi.Models.Task>> GetTask(int id)
        {
            var task = await _context.Tasks.FindAsync(id);

            if (task == null)
            {
                _logger.LogWarning("Task with ID {TaskId} not found.", id);
                return NotFound();
            }

            _logger.LogInformation("Fetched task with ID {TaskId}.", id);
            return Ok(task);
        }

        // PUT: api/task/5
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateTask(int id, tmapi.Models.Task task)
        {
            if (id != task.Id)
            {
                _logger.LogWarning("Task ID mismatch. URL ID: {UrlId}, Task ID: {TaskId}.", id, task.Id);
                return BadRequest("Task ID mismatch.");
            }

            _context.Entry(task).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
                _logger.LogInformation("Task with ID {TaskId} updated successfully.", id);
            }
            catch (DbUpdateConcurrencyException ex)
            {
                if (!TaskExists(id))
                {
                    _logger.LogWarning("Task with ID {TaskId} does not exist during update.", id);
                    return NotFound();
                }
                else
                {
                    _logger.LogError(ex, "Concurrency exception while updating task with ID {TaskId}.", id);
                    throw;
                }
            }

            return NoContent();
        }

        // DELETE: api/task/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteTask(int id)
        {
            var task = await _context.Tasks.FindAsync(id);
            if (task == null)
            {
                _logger.LogWarning("Attempted to delete non-existent task with ID {TaskId}.", id);
                return NotFound();
            }

            _context.Tasks.Remove(task);
            await _context.SaveChangesAsync();

            _logger.LogInformation("Task with ID {TaskId} deleted successfully.", id);
            return NoContent();
        }

        private bool TaskExists(int id)
        {
            bool exists = _context.Tasks.Any(e => e.Id == id);
            _logger.LogDebug("Task with ID {TaskId} exists: {Exists}.", id, exists);
            return exists;
        }
    }
}
