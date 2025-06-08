using HBOICTKeuzewijzer.Api.Controllers;
using HBOICTKeuzewijzer.Api.Models;
using HBOICTKeuzewijzer.Api.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;
using System.Security.Claims;
using Microsoft.Extensions.Configuration;

namespace HBOICTKeuzewijzer.Tests.Controllers
{
    public class AuthControllerTests
    {
        private readonly Mock<IApplicationUserService> _userServiceMock;
        private readonly Mock<IConfiguration> _configMock;
        private readonly AuthController _controller;

        public AuthControllerTests()
        {
            _userServiceMock = new Mock<IApplicationUserService>();
            _configMock = new Mock<IConfiguration>();

            _configMock.Setup(c => c.GetSection("AllowedRedirectDomains").Get<string[]>())
                .Returns(new string[] { "localhost" });

            _controller = new AuthController(_userServiceMock.Object, _configMock.Object);

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
        public void Login_ReturnsChallengeResultWithCorrectRedirect()
        {
            var returnUrl = "https://localhost:3000";
            var result = _controller.Login(returnUrl) as ChallengeResult;

            Assert.NotNull(result);
            Assert.Equal("Saml2", result.AuthenticationSchemes[0]);
            Assert.Equal($"/auth/succes?returnUrl=https//localhost:3000", result.Properties.RedirectUri);
        }

        [Fact]
        public async Task LoginSucces_RedirectsToReturnUrl_AfterUserCreation()
        {
            var returnUrl = "https://localhost:3000";
            var user = new ApplicationUser { Id = Guid.NewGuid() };
            _userServiceMock.Setup(x => x.GetOrCreateUserAsync(It.IsAny<ClaimsPrincipal>()))
                .ReturnsAsync(user);

            var result = await _controller.Succes(returnUrl) as RedirectResult;

            Assert.NotNull(result);
            Assert.Equal("https://localhost:3000", result.Url);
        }

        [Fact]
        public void Logout_ReturnsSignOutResultWithCorrectRedirect()
        {
            var returnUrl = "https://localhost:3000";
            var result = _controller.Logout(returnUrl) as SignOutResult;

            Assert.NotNull(result);
            Assert.Equal($"/auth/logout-succes?returnUrl=https//localhost:3000", result.Properties.RedirectUri);
        }

        [Fact]
        public void LogoutSucces_RedirectsToGivenReturnUrl()
        {
            var returnUrl = "https://localhost:3000";
            var result = _controller.LogoutSucces(returnUrl) as RedirectResult;

            Assert.NotNull(result);
            Assert.Equal("https://localhost:3000", result.Url);
        }
    }
}
