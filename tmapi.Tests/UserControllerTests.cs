using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using tmapi.Controllers;
using tmapi.Models;
using Microsoft.Extensions.Logging;
using Xunit;
using Microsoft.EntityFrameworkCore.ChangeTracking;

namespace tmapi.Tests
{
    public class UserControllerTests
    {
        private readonly DbContextOptions<AppDbContext> _dbContextOptions;
        private readonly AppDbContext _dbContext;
        private readonly Mock<ILogger<UserController>> _mockLogger;
        private readonly UserController _controller;

        public UserControllerTests()
        {
            // Use InMemoryDatabase for testing
            _dbContextOptions = new DbContextOptionsBuilder<AppDbContext>()
                .UseInMemoryDatabase(databaseName: "TestDb") // Test database name
                .Options;

            _dbContext = new AppDbContext(_dbContextOptions); // Creating in-memory context
            _mockLogger = new Mock<ILogger<UserController>>();

            _controller = new UserController(_dbContext, _mockLogger.Object);
        }

        // Test for CreateUser
        [Fact]
        public async System.Threading.Tasks.Task CreateUser_ReturnsCreatedAtAction_WhenUserIsValid()
        {
            // Arrange
            var user = new User
            {
                Id = 1,
                Username = "testuser",
                FirstName = "John",    // Added missing property
                LastName = "Doe",     // Added missing property
                PasswordHash = "hashedpassword", // Added missing property
                CreatedOn = DateTime.UtcNow
            };

            // Act
            var result = await _controller.CreateUser(user);

            // Assert
            var actionResult = Assert.IsType<ActionResult<User>>(result);
            var createdResult = Assert.IsType<CreatedAtActionResult>(actionResult.Result);
            Assert.Equal("GetUserById", createdResult.ActionName);

            // Verify that the user was actually saved in the database
            var savedUser = await _dbContext.Users.FindAsync(user.Id);
            Assert.NotNull(savedUser);
            Assert.Equal(user.Username, savedUser.Username);
            Assert.Equal(user.FirstName, savedUser.FirstName);  // Assert for added properties
            Assert.Equal(user.LastName, savedUser.LastName);    // Assert for added properties
        }

        // Test for GetUsers
        [Fact]
        public async System.Threading.Tasks.Task GetUsers_ReturnsOkResult_WhenUsersExist()
        {
            // Arrange
            var users = new List<User>
            {
                new User { Id = 1, Username = "user1", FirstName = "John", LastName = "Doe", PasswordHash = "hashedpassword" },
                new User { Id = 2, Username = "user2", FirstName = "Jane", LastName = "Smith", PasswordHash = "hashedpassword" }
            };

            await _dbContext.Users.AddRangeAsync(users);
            await _dbContext.SaveChangesAsync();

            // Act
            var result = await _controller.GetUsers();

            // Assert
            var actionResult = Assert.IsType<ActionResult<IEnumerable<User>>>(result);
            var okResult = Assert.IsType<OkObjectResult>(actionResult.Result);
            var returnValue = Assert.IsType<List<User>>(okResult.Value);
            Assert.Equal(2, returnValue.Count);
        }

      
        // Test for DeleteUser when user does not exist
        [Fact]
        public async System.Threading.Tasks.Task DeleteUser_ReturnsNoContent_WhenUserIsDeletedSuccessfully()
        {
            // Arrange
            var userId = 1;
            var user = new User
            {
                Id = userId,
                Username = "userToDelete",
                FirstName = "John",
                LastName = "Doe",
                PasswordHash = "hashedpassword"
            };

            // Ensure no user with the same ID exists before adding
            var existingUser = await _dbContext.Users.FindAsync(userId);
            if (existingUser != null)
            {
                _dbContext.Users.Remove(existingUser); // Remove any pre-existing user with the same ID
                await _dbContext.SaveChangesAsync();
            }

            // Add the user
            await _dbContext.Users.AddAsync(user);
            await _dbContext.SaveChangesAsync();

            // Act
            var result = await _controller.DeleteUser(userId);

            // Assert
            Assert.IsType<NoContentResult>(result);

            // Verify that the user is deleted
            var deletedUser = await _dbContext.Users.FindAsync(userId);
            Assert.Null(deletedUser);
        }

    }
}
