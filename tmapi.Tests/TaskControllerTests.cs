using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using tmapi.Controllers;
using tmapi.Models;
using Xunit;

namespace tmapi.Tests
{
    public class TaskControllerTests
    {
        private readonly TaskController _controller;
        private readonly AppDbContext _context;

        public TaskControllerTests()
        {
            // Set up in-memory database
            var options = new DbContextOptionsBuilder<AppDbContext>()
                .UseInMemoryDatabase("TestDb")
                .Options;

            _context = new AppDbContext(options);
            var logger = new LoggerFactory().CreateLogger<TaskController>();
            _controller = new TaskController(_context, logger);
        }

        [Fact]
        public async System.Threading.Tasks.Task CreateTask_Returns_CreatedAtActionResult()
        {
            // Arrange
            var task = new tmapi.Models.Task
            {
                CreatedBy = "TestUser",
                Title = "Test Task",
                Description = "Task description",
                CreatedAt = System.DateTime.UtcNow,
                Priority = "High",  // Provide a value for Priority
                Status = 0,  // Assuming Status needs to be set
                DueDate = System.DateTime.UtcNow.AddDays(1),  // Provide a DueDate
                Progress = 0  // Provide a Progress value
            };

            // Act
            var result = await _controller.CreateTask(task);

            // Assert
            Assert.IsType<ActionResult<tmapi.Models.Task>>(result);
            var createdAtActionResult = Assert.IsType<CreatedAtActionResult>(result.Result);
            Assert.Equal("GetTask", createdAtActionResult.ActionName);
            Assert.Equal(task.Id, ((tmapi.Models.Task)createdAtActionResult.Value).Id);
        }
        [Fact]
        public async System.Threading.Tasks.Task GetTasks_Returns_OkResult_With_Tasks()
        {
            // Arrange
            var task1 = new tmapi.Models.Task
            {
                CreatedBy = "TestUser1",
                Title = "Test Task 1",
                Description = "Description 1",
                CreatedAt = System.DateTime.UtcNow,
                Priority = "High",  // Provide a value for Priority
                Status = 0,  // Assuming Status needs to be set
                DueDate = System.DateTime.UtcNow.AddDays(1),  // Provide a DueDate
                Progress = 0  // Provide a Progress value
            };

            var task2 = new tmapi.Models.Task
            {
                CreatedBy = "TestUser2",
                Title = "Test Task 2",
                Description = "Description 2",
                CreatedAt = System.DateTime.UtcNow,
                Priority = "Medium",  // Provide a value for Priority
                Status = 0,  // Assuming Status needs to be set
                DueDate = System.DateTime.UtcNow.AddDays(2),  // Provide a DueDate
                Progress = 0  // Provide a Progress value
            };

            _context.Tasks.AddRange(task1, task2);
            await _context.SaveChangesAsync();

            // Act
            var result = await _controller.GetTasks();

            // Assert
            Assert.IsType<ActionResult<IEnumerable<tmapi.Models.Task>>>(result);
            var okObjectResult = Assert.IsType<OkObjectResult>(result.Result);
            var tasks = Assert.IsType<List<tmapi.Models.Task>>(okObjectResult.Value);
            Assert.Contains(tasks, t => t.Title == task1.Title);
            Assert.Contains(tasks, t => t.Title == task2.Title);
        }


        [Fact]
        public async System.Threading.Tasks.Task GetTask_Returns_NotFound_When_TaskDoesNotExist()
        {
            // Act
            var result = await _controller.GetTask(999); // Task ID that doesn't exist

            // Assert
            Assert.IsType<ActionResult<tmapi.Models.Task>>(result);
            Assert.IsType<NotFoundResult>(result.Result);
        }

        [Fact]
        public async System.Threading.Tasks.Task DeleteTask_Returns_NoContent_When_TaskIsDeleted()
        {
            // Arrange
            var task = new tmapi.Models.Task
            {
                CreatedBy = "TestUser",
                Title = "Test Task",
                Description = "Task description",
                CreatedAt = System.DateTime.UtcNow,
                Priority = "High",  // Set Priority
                Status = 0,  // Set Status
                DueDate = System.DateTime.UtcNow.AddDays(1),  // Set DueDate
                Progress = 0  // Set Progress
            };

            _context.Tasks.Add(task);
            await _context.SaveChangesAsync();

            // Act
            var result = await _controller.DeleteTask(task.Id);

            // Assert
            Assert.IsType<NoContentResult>(result);
        }

        [Fact]
        public async System.Threading.Tasks.Task DeleteTask_Returns_NotFound_When_TaskDoesNotExist()
        {
            // Act
            var result = await _controller.DeleteTask(999); // Non-existent task

            // Assert
            Assert.IsType<NotFoundResult>(result);
        }

        [Fact]
        public async System.Threading.Tasks.Task UpdateTask_UpdatesTask_When_TaskExists()
        {
            // Arrange
            var task = new tmapi.Models.Task
            {
                CreatedBy = "TestUser",
                Title = "Test Task",
                Description = "Task description",
                CreatedAt = System.DateTime.UtcNow,
                Priority = "High",  // Provide a value for Priority
                Status = 0,  // Assuming Status needs to be set
                DueDate = System.DateTime.UtcNow.AddDays(1),  // Provide a DueDate
                Progress = 0  // Provide a Progress value
            };

            _context.Tasks.Add(task);
            await _context.SaveChangesAsync();

            // Update the task
            task.Title = "Updated Task";

            // Act
            var result = await _controller.UpdateTask(task.Id, task);

            // Assert
            Assert.IsType<NoContentResult>(result);
            var updatedTask = await _context.Tasks.FindAsync(task.Id);
            Assert.Equal("Updated Task", updatedTask.Title);
        }

        [Fact]
        public async System.Threading.Tasks.Task UpdateTask_Returns_BadRequest_When_TaskIdMismatch()
        {
            // Arrange
            var task = new tmapi.Models.Task
            {
                CreatedBy = "TestUser",
                Title = "Test Task",
                Description = "Task description",
                CreatedAt = System.DateTime.UtcNow,
                Priority = "High",  // Provide a value for Priority
                Status = 0,  // Assuming Status needs to be set
                DueDate = System.DateTime.UtcNow.AddDays(1),  // Provide a DueDate
                Progress = 0  // Provide a Progress value
            };

            _context.Tasks.Add(task);
            await _context.SaveChangesAsync();

            // Act
            var result = await _controller.UpdateTask(999, task); // Task ID mismatch

            // Assert
            Assert.IsType<BadRequestObjectResult>(result);
            var badRequestObjectResult = (BadRequestObjectResult)result;
            Assert.Equal("Task ID mismatch.", badRequestObjectResult.Value);
        }
        [Fact]
        public async System.Threading.Tasks.Task UpdateTask_Returns_NotFound_When_TaskDoesNotExist()
        {
            // Act
            var result = await _controller.UpdateTask(999, new tmapi.Models.Task { Id = 999 }); // Task ID that doesn't exist

            // Assert
            Assert.IsType<NotFoundResult>(result);
        }

        [Fact]
        public async System.Threading.Tasks.Task GetTasksByUsername_Returns_NotFoundResult_When_NoTasksFound()
        {
            // Act
            var result = await _controller.GetTasksByUsername("NonExistingUser");

            // Assert
            Assert.IsType<ActionResult<IEnumerable<tmapi.Models.Task>>>(result);
            var notFoundResult = Assert.IsType<NotFoundObjectResult>(result.Result);
            Assert.Equal("No tasks found for user: NonExistingUser", notFoundResult.Value);
        }
       
    }
}
