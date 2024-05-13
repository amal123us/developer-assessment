using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Extensions.Logging;
using Xunit;
using TodoList.Api.Controllers;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

namespace TodoList.Api.UnitTests
{
    public class DummyTestShould
    {
        private TodoItemsController _controller;
        private Mock<TodoContext> _mockContext;
        private Mock<ILogger<TodoItemsController>> _mockLogger;

        public DummyTestShould()
        {
            _mockContext = new Mock<TodoContext>(new DbContextOptions<TodoContext>());
            _mockLogger = new Mock<ILogger<TodoItemsController>>();
            _controller = new TodoItemsController(_mockContext.Object, _mockLogger.Object);
        }
        [Fact]
        public async Task GetTodoItems_ReturnsOkResult()
        {
             // Arrange
            var todoItems = new List<TodoItem>
            {
                new TodoItem { Id = Guid.NewGuid(), Description = "Task 1", IsCompleted = false },
                new TodoItem { Id = Guid.NewGuid(), Description = "Task 2", IsCompleted = false }
            }.AsQueryable();

            var mockSet = new Mock<DbSet<TodoItem>>();
            mockSet.As<IAsyncEnumerable<TodoItem>>().Setup(m => m.GetAsyncEnumerator(It.IsAny<CancellationToken>())).Returns(new TestAsyncEnumerator<TodoItem>(todoItems.GetEnumerator()));
            mockSet.As<IQueryable<TodoItem>>().Setup(m => m.Provider).Returns(new TestAsyncQueryProvider<TodoItem>(todoItems.Provider));
            mockSet.As<IQueryable<TodoItem>>().Setup(m => m.Expression).Returns(todoItems.Expression);
            mockSet.As<IQueryable<TodoItem>>().Setup(m => m.ElementType).Returns(todoItems.ElementType);
            mockSet.As<IQueryable<TodoItem>>().Setup(m => m.GetEnumerator()).Returns(todoItems.GetEnumerator());

            _mockContext.Setup(m => m.TodoItems).Returns(mockSet.Object);

            // Act
            var result = await _controller.GetTodoItems();

            // Assert
            Assert.IsType<OkObjectResult>(result);
        }

        [Fact]
        public async Task GetTodoItem_ReturnsNotFound_WhenItemNotFound()
        {
            // Arrange
            var id = Guid.NewGuid();

            _mockContext.Setup(m => m.TodoItems.FindAsync(id)).ReturnsAsync((TodoItem)null);

            // Act
            var result = await _controller.GetTodoItem(id);

            // Assert
            Assert.IsType<NotFoundResult>(result);
        }

        [Fact]
        public async Task UpdateTodoItem_ReturnsBadRequest_WhenIdsMismatch()
        {
            // Arrange
            var id = Guid.NewGuid();
            var todoItem = new TodoItem { Id = Guid.NewGuid() };

            // Act
            var result = await _controller.UpdateTodoItem(id, todoItem);

            // Assert
            Assert.IsType<BadRequestResult>(result);
        }

        [Fact]
        public async Task AddTodoItem_ReturnsBadRequest_WhenDescriptionIsNullOrEmpty()
        {
            // Arrange
            var todoItem = new TodoItem { Description = null };

            // Act
            var result = await _controller.AddTodoItem(todoItem);

            // Assert
            Assert.IsType<BadRequestObjectResult>(result);
        }


        [Fact]
        public async Task CompleteTodoItem_ReturnsBadRequest_WhenIdsMismatch()
        {
            // Arrange
            var id = Guid.NewGuid();
            var todoItem = new TodoItem { Id = Guid.NewGuid() };

            // Act
            var result = await _controller.CompleteTodoItem(id, todoItem);

            // Assert
            Assert.IsType<BadRequestResult>(result);
        }
    }
}
