using Moq;
using Microsoft.AspNetCore.Mvc;
using HBOICTKeuzewijzer.Api.Controllers;
using HBOICTKeuzewijzer.Api.Models;
using HBOICTKeuzewijzer.Api.Repositories;
using HBOICTKeuzewijzer.Api.Services;
using HBOICTKeuzewijzer.Api.Dtos;
using System.Linq.Expressions;
using Microsoft.Identity.Client;
using System.Security.Claims;
using Microsoft.AspNetCore.Http;

namespace HBOICTKeuzewijzer.tests.Controllers
{
    public class SlbControllerTests
    {
        private readonly Mock<ISlbRepository> _mockSlbRepo;
        private readonly Mock<IApplicationUserService> _mockUserService;
        private readonly SlbController _controller;

        public SlbControllerTests()
        {
            _mockSlbRepo = new Mock<ISlbRepository>();
            _mockUserService = new Mock<IApplicationUserService>();
            _controller = new SlbController(_mockSlbRepo.Object, _mockUserService.Object);
        }

        [Fact]
        public async Task GetPagedSlb_ReturnsOkResult_WithSlbList()
        {
            // Arrange
            var request = new GetAllRequestQuery { Page=1, PageSize=10 };
            var slbId = Guid.NewGuid();
            var slbList = new List<Slb>
            {
                new Slb
                {
                    Id = slbId,
                    SlbApplicationUser = new ApplicationUser
                    {
                    Id = Guid.NewGuid(),
                    DisplayName = "SLB'er 1",
                    Email = "slber1@example.com",
                    ExternalId = "ext-id-1"
                    },
                    SlbApplicationUserId = Guid.NewGuid(),
                    StudentApplicationUserId = Guid.NewGuid()
                 }
            };
            
            var expectedResult = new PaginatedResult<Slb>
            {
                Items = slbList,
                TotalCount = slbList.Count
            };

            _mockSlbRepo
                .Setup(repo => repo.GetPaginatedAsync(request, It.IsAny<Expression<Func<Slb, object>>>()))
                .ReturnsAsync(expectedResult);

            // Act
            var result = await _controller.GetPagedSlb(request);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result.Result);
            var actual = Assert.IsAssignableFrom<PaginatedResult<Slb>>(okResult.Value);
            Assert.Single(actual.Items);
            Assert.Equal("SLB'er 1", actual.Items.First().SlbApplicationUser.DisplayName);  
        }

        [Fact]
        public async Task GetStudentsForSlb_ReturnsOkResult_WithStudentList()
        {
            // Arrange
            var slbId = Guid.NewGuid();
            var request = new GetAllRequestQuery { Page = 1, PageSize = 10 };
            var studentList = new List<StudentDto>
            {
                new StudentDto
                {
                    Id = Guid.NewGuid(),
                    DisplayName = "Student 1",
                    Email = "student1@example.com",
                    Code = "S1234567",
                    Cohort = "2025",
                    SlbId = slbId
                }
            };

            var expectedResult = new PaginatedResult<StudentDto>
            {
                Items = studentList,
                TotalCount = studentList.Count
            };

            _mockSlbRepo
                .Setup(repo => repo.GetStudentsBySlbAsync(slbId, request))
                .ReturnsAsync(expectedResult);

            // Act
            var result = await _controller.GetStudentsForSlb(slbId, request);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result.Result);
            var actual = Assert.IsAssignableFrom<PaginatedResult<StudentDto>>(okResult.Value);
            Assert.Single(actual.Items);
            Assert.Equal("Student 1", actual.Items.First().DisplayName);
            Assert.Equal("S1234567", actual.Items.First().Code);
        }

        [Fact]
        public async Task GetStudents_CallsRepoWithCurrentUserId_ReturnsStudents()
        {
            // Arrange
            var slbUserId = Guid.NewGuid();

            // Mock ApplicationUserService to get current user Id
            _mockUserService
                .Setup(s => s.GetOrCreateUserAsync(It.IsAny<ClaimsPrincipal>()))
                .ReturnsAsync(new ApplicationUser
                {
                    Id = slbUserId,
                    ExternalId = "external-id-123",
                    DisplayName = "SLB Counselor",
                    Email = "slb@example.com"
                });

            var request = new GetAllRequestQuery { Page = 1, PageSize = 10 };

            var students = new List<StudentDto>
            {
                new StudentDto
                {
                    Id = Guid.NewGuid(),
                    DisplayName = "Student A",
                    Email = "student1@example.com",
                    Code = "S1234567",
                    Cohort = "2025",
                    SlbId = slbUserId
                }
            };

            var expectedResult = new PaginatedResult<StudentDto>
            {
                Items = students,
                TotalCount = students.Count
            };

            _mockSlbRepo
                .Setup(r => r.GetStudentsBySlbAsync(slbUserId, request))
                .ReturnsAsync(expectedResult);

            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, "external-id-123")
            };

            var identity = new ClaimsIdentity(claims, "TestAuthType");
            var claimsPrincipal = new ClaimsPrincipal(identity);

            _controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext { User = claimsPrincipal }
            };

            // Act
            var result = await _controller.GetStudents(request);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result.Result);
            var actual = Assert.IsAssignableFrom<PaginatedResult<StudentDto>>(okResult.Value);
            Assert.Single(actual.Items);
            Assert.Equal("Student A", actual.Items.First().DisplayName);

            // Verify that repo is called with correct User Id
            _mockSlbRepo.Verify(r => r.GetStudentsBySlbAsync(slbUserId, request));
        }
    }
}