using Microsoft.AspNetCore.Mvc;
using HBOICTKeuzewijzer.Api.Controllers;
using HBOICTKeuzewijzer.Api.Models;
using HBOICTKeuzewijzer.Api.Repositories;
using HBOICTKeuzewijzer.Api.Services;
using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using Moq;

namespace HBOICTKeuzewijzer.Tests.Controllers
{
    public class ChatControllerTests
    {
        private readonly Mock<IRepository<Chat>> _mockRepo;
        private readonly Mock<ApplicationUserService> _mockUserService;
        private readonly ApplicationUser _seededUser;
        private readonly ChatController _controller;

        public ChatControllerTests()
        {
            _mockRepo = new Mock<IRepository<Chat>>();
            _mockUserService = new Mock<ApplicationUserService>(null!);

            _seededUser = new ApplicationUser
            {
                Id = Guid.NewGuid(),
                ExternalId = "external-id",
                Email = "test@example.com",
                DisplayName = "Test User",
                ApplicationUserRoles = new List<ApplicationUserRole>
                {
                    new ApplicationUserRole { Id = Guid.NewGuid(), Role = Role.User, ApplicationUserId = Guid.NewGuid() }
                }
            };

            // Setup default behavior for user service
            _mockUserService
                .Setup(s => s.GetOrCreateUserAsync(It.IsAny<ClaimsPrincipal>()))
                .ReturnsAsync(_seededUser);

            _mockUserService
                .Setup(s => s.GetByEmailAsync(It.IsAny<string>()))
                .ReturnsAsync(_seededUser);

            _mockUserService
                .Setup(s => s.GetUserWithRolesByIdAsync(It.IsAny<Guid>()))
                .ReturnsAsync(_seededUser);

            _controller = new ChatController(_mockRepo.Object, _mockUserService.Object);

            var user = new ClaimsPrincipal(new ClaimsIdentity(new[]
            {
                new Claim(ClaimTypes.NameIdentifier, _seededUser.ExternalId),
                new Claim(ClaimTypes.Email, _seededUser.Email),
                new Claim(ClaimTypes.Name, _seededUser.DisplayName)
            }, "mock"));

            _controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext { User = user }
            };
        }

        [Fact]
        public async Task Create_ReturnsCreatedAtAction_WhenChatIsValid()
        {
            var newChat = new Chat
            {
                Id = Guid.NewGuid(),
                SlbApplicationUserId = _seededUser.Id,
                StudentApplicationUserId = Guid.NewGuid()
            };

            _mockRepo
                .Setup(r => r.AddAsync(newChat))
                .Returns(Task.CompletedTask)
                .Verifiable();

            var result = await _controller.Create(newChat);

            var createdResult = Assert.IsType<CreatedAtActionResult>(result.Result);
            var returnedChat = Assert.IsType<Chat>(createdResult.Value);
            Assert.Equal(newChat.Id, returnedChat.Id);

            _mockRepo.Verify();
        }

        [Fact]
        public async Task Create_ReturnsCreatedAtAction_WhenModelStateIsInvalid()
        {
            _controller.ModelState.AddModelError("SlbApplicationUserId", "SLB gebruiker is verplicht");
            var newChat = new Chat();
            var result = await _controller.Create(newChat);
            Assert.IsType<CreatedAtActionResult>(result.Result);
        }
    }
}
