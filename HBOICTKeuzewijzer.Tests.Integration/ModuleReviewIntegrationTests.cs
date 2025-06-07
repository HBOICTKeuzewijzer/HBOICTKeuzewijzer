using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using HBOICTKeuzewijzer.Api.Dtos;
using HBOICTKeuzewijzer.Api.Models;
using HBOICTKeuzewijzer.Tests.Integration.Shared;
using Microsoft.EntityFrameworkCore;

namespace HBOICTKeuzewijzer.Tests.Integration;

public class ModuleReviewIntegrationTests
{
    public class Post : ModuleReviewIntegrationTests
    {
        [Theory]
        [InlineData("Student")]
        public async Task PostReview_CreatesReviewInDatabase(string role)
        {
            using var factory = new TestAppFactory();

            var moduleId = Guid.NewGuid();

            var dto = new ModuleReviewDto
            {
                ModuleId = moduleId,
                ReviewText = "Zeer nuttige module"
            };

            var request = new HttpRequestMessage(HttpMethod.Post, "/ModuleReview")
            {
                Content = JsonContent.Create(dto)
            };
            request.Headers.Add("X-Test-Auth", "true");
            request.Headers.Add("X-Test-Role", role);

            var response = await factory.Client.SendAsync(request);

            response.StatusCode.Should().Be(HttpStatusCode.OK);

            await using var db = factory.CreateDbContext();
            var saved = await db.ModuleReviews.FirstOrDefaultAsync(r => r.ModuleId == moduleId);

            saved.Should().NotBeNull();
            saved!.ReviewText.Should().Be(dto.ReviewText);
        }

        [Theory]
        [InlineData("Docent")]
        [InlineData("SystemAdmin")]
        public async Task PostReview_ReturnsForbidden_WhenUserNotStudent(string role)
        {
            using var factory = new TestAppFactory();

            var dto = new ModuleReviewDto
            {
                ModuleId = Guid.NewGuid(),
                ReviewText = "Niet toegestaan"
            };

            var request = new HttpRequestMessage(HttpMethod.Post, "/ModuleReview")
            {
                Content = JsonContent.Create(dto)
            };
            request.Headers.Add("X-Test-Auth", "true");
            request.Headers.Add("X-Test-Role", role);

            var response = await factory.Client.SendAsync(request);

            response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
        }

        [Theory]
        [InlineData("Student")]
        public async Task PostReview_ReturnsBadRequest_WhenModelInvalid(string role)
        {
            using var factory = new TestAppFactory();

            var dto = new ModuleReviewDto
            {
                ModuleId = Guid.NewGuid(),
                ReviewText = "" // Invalid input
            };

            var request = new HttpRequestMessage(HttpMethod.Post, "/ModuleReview")
            {
                Content = JsonContent.Create(dto)
            };
            request.Headers.Add("X-Test-Auth", "true");
            request.Headers.Add("X-Test-Role", role);

            var response = await factory.Client.SendAsync(request);

            response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        }

        [Fact]
        public async Task PostReview_ReturnsUnauthorized_WhenNotAuthenticated()
        {
            using var factory = new TestAppFactory();

            var dto = new ModuleReviewDto
            {
                ModuleId = Guid.NewGuid(),
                ReviewText = "Mooi vak"
            };

            var request = new HttpRequestMessage(HttpMethod.Post, "/ModuleReview")
            {
                Content = JsonContent.Create(dto)
            };

            var response = await factory.Client.SendAsync(request);

            response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
        }
    }

    public class Get : ModuleReviewIntegrationTests
    {
        [Fact]
        public async Task GetReviews_ReturnsEmptyList_WhenNoReviewsExist()
        {
            using var factory = new TestAppFactory();

            var moduleId = Guid.NewGuid();

            var request = new HttpRequestMessage(HttpMethod.Get, $"/ModuleReview/{moduleId}");

            var response = await factory.Client.SendAsync(request);

            response.StatusCode.Should().Be(HttpStatusCode.OK);

            var list = await response.Content.ReadFromJsonAsync<List<ModuleReviewResponseDto>>();

            list.Should().NotBeNull();
            list.Should().BeEmpty();
        }

        [Fact]
        public async Task GetReviews_ReturnsSeededReviews_ForModule()
        {
            using var factory = new TestAppFactory();

            var moduleId = Guid.NewGuid();
            var studentId = Guid.NewGuid();

            var review = new ModuleReview
            {
                Id = Guid.NewGuid(),
                ModuleId = moduleId,
                StudentId = studentId,
                ReviewText = "Seeded review",
                CreatedAt = DateTime.UtcNow,
                Student = new ApplicationUser
                {
                    Id = studentId,
                    DisplayName = "Test Student"
                }
            };

            await SeedHelper.SeedAsync(factory.Services, review);

            var request = new HttpRequestMessage(HttpMethod.Get, $"/ModuleReview/{moduleId}");

            var response = await factory.Client.SendAsync(request);

            response.StatusCode.Should().Be(HttpStatusCode.OK);

            var list = await response.Content.ReadFromJsonAsync<List<ModuleReviewResponseDto>>();

            list.Should().NotBeNull();
            list.Should().ContainSingle();
            list[0].ReviewText.Should().Be("Seeded review");
            list[0].StudentName.Should().Be("Test Student");
        }
    }
}
