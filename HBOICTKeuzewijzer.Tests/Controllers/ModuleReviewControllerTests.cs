using Moq;
using Xunit;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using HBOICTKeuzewijzer.Api.Controllers;
using HBOICTKeuzewijzer.Api.Models;
using HBOICTKeuzewijzer.Api.Repositories;
using HBOICTKeuzewijzer.Api.Services;
using HBOICTKeuzewijzer.Api.Dtos;

namespace HBOICTKeuzewijzer.Tests.Controllers
{
    public class ModuleReviewControllerTests
    {
        private readonly Mock<IRepository<ModuleReview>> _mockRepo;
        private readonly Mock<IApplicationUserService> _mockUserService;
        private readonly ModuleReviewController _controller;

        public ModuleReviewControllerTests()
        {
            _mockRepo = new Mock<IRepository<ModuleReview>>();
            _mockUserService = new Mock<IApplicationUserService>();
            _controller = new ModuleReviewController(_mockRepo.Object, _mockUserService.Object);
        }

        [Fact]
        public async Task PostReview_ReturnsOk_WhenInputIsValid()
        {
            // Arrange
            var dto = new ModuleReviewDto
            {
                ModuleId = Guid.NewGuid(),
                ReviewText = "Erg leerzaam!"
            };

            var user = new ApplicationUser { Id = Guid.NewGuid() };
            _mockUserService.Setup(s => s.GetOrCreateUserAsync(It.IsAny<ClaimsPrincipal>()))
                            .ReturnsAsync(user);

            var userContext = new ClaimsPrincipal(new ClaimsIdentity(new Claim[]
            {
                new Claim(ClaimTypes.NameIdentifier, "user-id"),
                new Claim(ClaimTypes.Role, "Student")
            }, "mock"));

            _controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext { User = userContext }
            };

            // Act
            var result = await _controller.PostReview(dto);

            // Assert
            Assert.IsType<OkResult>(result);
            _mockRepo.Verify(r => r.AddAsync(It.Is<ModuleReview>(r =>
                r.ModuleId == dto.ModuleId &&
                r.ReviewText == dto.ReviewText &&
                r.StudentId == user.Id
            )), Times.Once);
        }


        [Fact]
        public async Task PostReview_ReturnsOk_WhenValid()
        {
            // Arrange
            var dto = new ModuleReviewDto
            {
                ModuleId = Guid.NewGuid(),
                ReviewText = "Erg leerzaam!"
            };

            var user = new ApplicationUser { Id = Guid.NewGuid() };
            _mockUserService.Setup(s => s.GetOrCreateUserAsync(It.IsAny<ClaimsPrincipal>())).ReturnsAsync(user);

            var userContext = new ClaimsPrincipal(new ClaimsIdentity(new Claim[]
            {
                new Claim(ClaimTypes.NameIdentifier, "student-id"),
                new Claim(ClaimTypes.Role, "Student")
            }, "mock"));

            _controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext { User = userContext }
            };

            // Act
            var result = await _controller.PostReview(dto);

            // Assert
            Assert.IsType<OkResult>(result);
            _mockRepo.Verify(r => r.AddAsync(It.Is<ModuleReview>(r =>
                r.ModuleId == dto.ModuleId &&
                r.ReviewText == dto.ReviewText &&
                r.StudentId == user.Id
            )), Times.Once);
        }

        [Fact]
        public async Task PostReview_ReturnsBadRequest_WhenModelInvalid()
        {
            // Arrange
            _controller.ModelState.AddModelError("ReviewText", "Required");
            var dto = new ModuleReviewDto { ModuleId = Guid.NewGuid() };

            // Act
            var result = await _controller.PostReview(dto);

            // Assert
            Assert.IsType<BadRequestObjectResult>(result);
        }
                [Fact]
        public async Task PostReview_ThrowsException_WhenUserNotAuthenticated()
        {
            var dto = new ModuleReviewDto
            {
                ModuleId = Guid.NewGuid(),
                ReviewText = "Prima"
            };

            _controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext() // Geen user
            };

            _mockUserService.Setup(s => s.GetOrCreateUserAsync(It.IsAny<ClaimsPrincipal>()))
                            .ThrowsAsync(new Exception("Unauthorized"));

            await Assert.ThrowsAsync<Exception>(() => _controller.PostReview(dto));
        }

    }
}
