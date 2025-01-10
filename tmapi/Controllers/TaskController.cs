using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using tmapi.Models;

namespace tmapi.Controllers
{

    [Route("api/[controller]")]
    [ApiController]
    public class TaskController : ControllerBase
    {
        private readonly AppDbContext _context;

        public TaskController(AppDbContext context)
        {
            _context = context;
        }

        // POST: api/task
        [HttpPost]
        public async System.Threading.Tasks.Task<ActionResult<tmapi.Models.Task>> CreateTask(tmapi.Models.Task task)
        {
            if (task == null)
            {
                return BadRequest("Task data is null.");
            }

            task.CreatedAt = DateTime.UtcNow;
            _context.Tasks.Add(task);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetTask), new { id = task.Id }, task);
        }

        // GET: api/task
        [HttpGet]
        public async Task<ActionResult<IEnumerable<tmapi.Models.Task>>> GetTasks()
        {
            return await _context.Tasks.ToListAsync();
        }

        // GET: api/task/username/{username}
        [HttpGet("username/{username}")]
        public async Task<ActionResult<IEnumerable<tmapi.Models.Task>>> GetTasksByUsername(string username)
        {
            var tasks = await _context.Tasks.Where(t => t.CreatedBy == username).ToListAsync();

            if (tasks == null || !tasks.Any())
            {
                return NotFound($"No tasks found for user: {username}");
            }

            return Ok(tasks);
        }

        // GET: api/task/5
        [HttpGet("{id}")]
        public async System.Threading.Tasks.Task<ActionResult<tmapi.Models.Task>> GetTask(int id)
        {
            var task = await _context.Tasks.FindAsync(id);

            if (task == null)
            {
                return NotFound();
            }

            return Ok(task);
        }



        // PUT: api/task/5
        [HttpPut("{id}")]
        public async System.Threading.Tasks.Task<IActionResult> UpdateTask(int id, tmapi.Models.Task task)
        {
            if (id != task.Id)
            {
                return BadRequest("Task ID mismatch.");
            }

            _context.Entry(task).State = EntityState.Modified;
            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!TaskExists(id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }

            return NoContent();
        }

        // DELETE: api/task/5
        [HttpDelete("{id}")]
        public async System.Threading.Tasks.Task<IActionResult> DeleteTask(int id)
        {
            var task = await _context.Tasks.FindAsync(id);
            if (task == null)
            {
                return NotFound();
            }

            _context.Tasks.Remove(task);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private bool TaskExists(int id)
        {
            return _context.Tasks.Any(e => e.Id == id);
        }
    }

}