using Moq;
using Microsoft.AspNetCore.Mvc;
using HBOICTKeuzewijzer.Api.Controllers;
using HBOICTKeuzewijzer.Api.Models;
using HBOICTKeuzewijzer.Api.Repositories;
using HBOICTKeuzewijzer.Api.Services;
using System.Security.Claims;

namespace xUnitTest
{
    public class ModuleControllerTests
    {
        private readonly Mock<IRepository<Module>> _mockRepo;
        private readonly Mock<ApplicationUserService> _mockUserService;
        private readonly ModuleController _controller;

        public ModuleControllerTests()
        {
            _mockRepo = new Mock<IRepository<Module>>();
            _mockUserService = new Mock<ApplicationUserService>(null!);
            _controller = new ModuleController(_mockRepo.Object, _mockUserService.Object);
        }

        [Fact]
        public async Task GetModule_ReturnsModule_WhenModuleExists()
        {
            // Arrange
            var testId = Guid.NewGuid();
            var testModule = new Module { Id = testId, Name = "Testmodule" };

            _mockRepo.Setup(repo => repo.GetByIdAsync(testId)).ReturnsAsync(testModule);

            // Act
            var result = await _controller.GetModule(testId);

            // Assert
            var okResult = Assert.IsType<ActionResult<Module>>(result);
            var module = Assert.IsType<Module>(okResult.Value);
            Assert.Equal(testId, module.Id);
            Assert.Equal("Testmodule", module.Name);
        }

        [Fact]
        public async Task PostModule_ReturnsCreatedAtAction_WhenModuleIsValid()
        {
            // Arrange
            var newModule = new Module
            {
                Id = Guid.NewGuid(),
                Name = "Nieuwe Module"
            };

            _mockUserService
                .Setup(s => s.GetOrCreateUserAsync(It.IsAny<ClaimsPrincipal>()))
                .ReturnsAsync(new ApplicationUser());

            _mockRepo
                .Setup(r => r.AddAsync(newModule))
                .Returns(Task.CompletedTask)
                .Verifiable();

            // Act
            var result = await _controller.PostModule(newModule);

            // Assert
            var createdResult = Assert.IsType<CreatedAtActionResult>(result.Result);
            var returnedModule = Assert.IsType<Module>(createdResult.Value);
            Assert.Equal(newModule.Id, returnedModule.Id);
            Assert.Equal("Nieuwe Module", returnedModule.Name);

            _mockRepo.Verify();
        }

        [Fact]
        public async Task PostModule_ReturnsBadRequest_WhenModelStateIsInvalid()
        {
            // Arrange
            _controller.ModelState.AddModelError("Name", "Naam is verplicht");

            var newModule = new Module();

            // Act
            var result = await _controller.PostModule(newModule);

            // Assert
            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result.Result);
            Assert.IsType<SerializableError>(badRequestResult.Value);
        }

    }
}
