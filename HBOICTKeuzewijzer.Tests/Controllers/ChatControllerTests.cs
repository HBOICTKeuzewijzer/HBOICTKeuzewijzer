using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using HBOICTKeuzewijzer.Api.Controllers;
using HBOICTKeuzewijzer.Api.Dtos;
using HBOICTKeuzewijzer.Api.Models;
using HBOICTKeuzewijzer.Api.Repositories;
using HBOICTKeuzewijzer.Api.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore.Query;
using Moq;
using Xunit;

namespace HBOICTKeuzewijzer.Tests.Controllers
{
    public class ChatControllerTests
    {
        private readonly Mock<IRepository<Chat>> _chatRepoMock;
        private readonly Mock<IApplicationUserService> _userServiceMock;
        private readonly ChatController _controller;

        public ChatControllerTests()
        {
            _chatRepoMock = new Mock<IRepository<Chat>>();
            _userServiceMock = new Mock<IApplicationUserService>();
            _controller = new ChatController(_chatRepoMock.Object, _userServiceMock.Object);

            // Set up a fake user for controller context
            var user = new ClaimsPrincipal(new ClaimsIdentity(new Claim[]
            {
                new Claim(ClaimTypes.NameIdentifier, "user-id")
            }, "mock"));
            _controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext { User = user }
            };
        }

        [Fact]
        public async Task Read_ReturnsNotFound_WhenChatNotAuthorized()
        {
            // Arrange
            var chatId = Guid.NewGuid();
            var user = new ApplicationUser { Id = Guid.NewGuid() };
            _userServiceMock.Setup(s => s.GetOrCreateUserAsync(It.IsAny<ClaimsPrincipal>()))
                .ReturnsAsync(user);
            _chatRepoMock.Setup(r => r.GetByIdAsync(chatId))
                .ReturnsAsync((Chat)null);

            // Act
            var result = await _controller.Read(chatId);

            // Assert
            Assert.IsType<NotFoundResult>(result.Result);
        }

        [Fact]
        public async Task Create_ReturnsCreatedAtAction()
        {
            // Arrange
            var chat = new Chat { Id = Guid.NewGuid() };
            _chatRepoMock.Setup(r => r.AddAsync(chat)).Returns(Task.CompletedTask);

            // Act
            var result = await _controller.Create(chat);

            // Assert
            var created = Assert.IsType<CreatedAtActionResult>(result.Result);
            Assert.Equal(nameof(_controller.Read), created.ActionName);
            Assert.Equal(chat, created.Value);
        }

        [Fact]
        public async Task Delete_ReturnsNotFound_WhenChatNotAuthorized()
        {
            // Arrange
            var chatId = Guid.NewGuid();
            var user = new ApplicationUser { Id = Guid.NewGuid() };
            _userServiceMock.Setup(s => s.GetOrCreateUserAsync(It.IsAny<ClaimsPrincipal>()))
                .ReturnsAsync(user);
            _chatRepoMock.Setup(r => r.GetByIdAsync(chatId))
                .ReturnsAsync((Chat)null);

            // Act
            var result = await _controller.Delete(chatId);

            // Assert
            Assert.IsType<NotFoundResult>(result);
        }

        [Fact]
        public async Task Delete_ReturnsNoContent_WhenChatAuthorized()
        {
            // Arrange
            var chatId = Guid.NewGuid();
            var user = new ApplicationUser { Id = Guid.NewGuid() };
            var chat = new Chat { Id = chatId, SlbApplicationUserId = user.Id };
            _userServiceMock.Setup(s => s.GetOrCreateUserAsync(It.IsAny<ClaimsPrincipal>()))
                .ReturnsAsync(user);
            _chatRepoMock.Setup(r => r.GetByIdAsync(chatId))
                .ReturnsAsync(chat);
            _chatRepoMock.Setup(r => r.DeleteAsync(chatId)).Returns(Task.CompletedTask);

            // Act
            var result = await _controller.Delete(chatId);

            // Assert
            Assert.IsType<NoContentResult>(result);
        }

        [Fact]
        public async Task CreateWithEmail_ReturnsNotFound_WhenOtherUserNotFound()
        {
            // Arrange
            _userServiceMock.Setup(s => s.GetByEmailAsync(It.IsAny<string>()))
                .ReturnsAsync((ApplicationUser)null);

            // Act
            var result = await _controller.CreateWithEmail(new CreateChatDto{Email = "test@example.com" });

            // Assert
            var notFound = Assert.IsType<NotFoundObjectResult>(result.Result);
            Assert.Contains("not found", notFound.Value.ToString(), StringComparison.OrdinalIgnoreCase);
        }

        [Fact]
        public async Task CreateWithEmail_ReturnsBadRequest_WhenCreatingWithSelf()
        {
            // Arrange
            var user = new ApplicationUser { Id = Guid.NewGuid() };
            _userServiceMock.Setup(s => s.GetByEmailAsync(It.IsAny<string>()))
                .ReturnsAsync(user);
            _userServiceMock.Setup(s => s.GetOrCreateUserAsync(It.IsAny<ClaimsPrincipal>()))
                .ReturnsAsync(user);

            // Act
            var result = await _controller.CreateWithEmail("test@example.com");

            // Assert
            var badRequest = Assert.IsType<BadRequestObjectResult>(result.Result);
            Assert.Contains("yourself", badRequest.Value.ToString(), StringComparison.OrdinalIgnoreCase);
        }

    }
}
