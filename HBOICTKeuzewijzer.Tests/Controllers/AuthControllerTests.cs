using HBOICTKeuzewijzer.Api.Controllers;
using HBOICTKeuzewijzer.Api.Models;
using HBOICTKeuzewijzer.Api.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Moq;
using System.Security.Claims;

namespace HBOICTKeuzewijzer.Tests.Controllers
{
    public class AuthControllerTests
    {
        private readonly Mock<IApplicationUserService> _userServiceMock;
        private readonly AuthController _controller;

        public AuthControllerTests()
        {
            _userServiceMock = new Mock<IApplicationUserService>();

            // Create real configuration with in-memory settings
            var inMemorySettings = new Dictionary<string, string>
            {
                {"AllowedRedirectDomains:0", "localhost"}
            };

            IConfiguration configuration = new ConfigurationBuilder()
                .AddInMemoryCollection(inMemorySettings)
                .Build();

            _controller = new AuthController(_userServiceMock.Object, configuration);

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

            // RedirectUri is url-encoded inside controller
            var expectedRedirectUri = $"/auth/succes?returnUrl={Uri.EscapeDataString(returnUrl)}";
            Assert.Equal(expectedRedirectUri, result.Properties.RedirectUri);
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

            var expectedRedirectUri = $"/auth/logout-succes?returnUrl={Uri.EscapeDataString(returnUrl)}";
            Assert.Equal(expectedRedirectUri, result.Properties.RedirectUri);
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
