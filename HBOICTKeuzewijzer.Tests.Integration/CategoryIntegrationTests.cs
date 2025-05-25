using HBOICTKeuzewijzer.Api.Models;
using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using HBOICTKeuzewijzer.Tests.Integration.Shared;
using System.Data;

namespace HBOICTKeuzewijzer.Tests.Integration;

public class CategoryIntegrationTests
{
    [Theory]
    [InlineData("SystemAdmin")]
    [InlineData("ModuleAdmin")]
    public async Task PostCategory_CreatesEntryInDatabase(string role)
    {
        using var factory = new TestAppFactory();

        var newCategory = new Category
        {
            Id = Guid.NewGuid(),
            Value = "Test Category",
            AccentColor = "#fff",
            PrimaryColor = "#aaa",
            Position = 1
        };

        var request = new HttpRequestMessage(HttpMethod.Post, "/Category")
        {
            Content = JsonContent.Create(newCategory)
        };
        request.Headers.Add("X-Test-Auth", "true");
        request.Headers.Add("X-Test-Role", role);

        var response = await factory.Client.SendAsync(request);

        response.StatusCode.Should().Be(HttpStatusCode.Created);
        response.Headers.Location!.AbsolutePath.Should().Be($"/Category/{newCategory.Id}");

        Category? saved;
        await using (var context = factory.CreateDbContext())
        {
            saved = await context.Categories.FindAsync(newCategory.Id);
        }

        saved.Should().NotBeNull();
        saved!.Value.Should().Be("Test Category");
        saved!.AccentColor.Should().Be("#fff");
        saved!.Position.Should().Be(1);
        saved!.PrimaryColor.Should().Be("#aaa");
    }

    [Theory]
    [InlineData("SystemAdmin")]
    [InlineData("ModuleAdmin")]
    public async Task PutCategory_UpdatesCategoryInDatabase(string role)
    {
        using var factory = new TestAppFactory();

        var categoryId = Guid.NewGuid();

        var oldCategory = new Category
        {
            Id = categoryId,
            AccentColor = "#fff",
            PrimaryColor = "#000",
            Position = 1,
            Value = "Test category"
        };

        await SeedHelper.SeedAsync(factory.Services, oldCategory);

        var updatedCategory = new Category
        {
            Id = categoryId,
            AccentColor = "#aaa",
            PrimaryColor = "#fff",
            Position = 1,
            Value = "Test category updated"
        };

        var request = new HttpRequestMessage(HttpMethod.Put, $"/Category/{updatedCategory.Id}")
        {
            Content = JsonContent.Create(updatedCategory)
        };
        request.Headers.Add("X-Test-Auth", "true");
        request.Headers.Add("X-Test-Role", role);

        var response = await factory.Client.SendAsync(request);

        response.StatusCode.Should().Be(HttpStatusCode.NoContent);

        Category? saved;
        await using (var context = factory.CreateDbContext())
        {
            saved = await context.Categories.FindAsync(updatedCategory.Id);
        }

        saved.Should().NotBeNull();
        saved!.Value.Should().Be("Test category updated");
        saved!.AccentColor.Should().Be("#aaa");
        saved!.Position.Should().Be(1);
        saved!.PrimaryColor.Should().Be("#fff");
    }
    
    [Theory]
    [InlineData("SystemAdmin")]
    [InlineData("ModuleAdmin")]
    public async Task PutCategory_RespondsWithBadRequest_WhenIdDoesNotMatchCategory(string role)
    {
        using var factory = new TestAppFactory();

        var category = new Category
        {
            Id = Guid.NewGuid(),
            AccentColor = "#aaa",
            PrimaryColor = "#fff",
            Position = 1,
            Value = "Test category updated"
        };

        var request = new HttpRequestMessage(HttpMethod.Put, $"/Category/{/*invalid id*/Guid.NewGuid()}")
        {
            Content = JsonContent.Create(category)
        };
        request.Headers.Add("X-Test-Auth", "true");
        request.Headers.Add("X-Test-Role", role);

        var response = await factory.Client.SendAsync(request);

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task PutCategory_RespondsWithUnauthorized_WhenNotAuthenticated()
    {
        using var factory = new TestAppFactory();

        var category = new Category
        {
            Id = Guid.NewGuid(),
            AccentColor = "#aaa",
            PrimaryColor = "#fff",
            Position = 1,
            Value = "Test category updated"
        };

        var request = new HttpRequestMessage(HttpMethod.Put, $"/Category/{category.Id}")
        {
            Content = JsonContent.Create(category)
        };

        var response = await factory.Client.SendAsync(request);

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Theory]
    [InlineData("User")]
    [InlineData("Student")]
    [InlineData("SLB")]
    public async Task PutCategory_RespondsWithForbidden_WhenRolesAreNotCorrect(string role)
    {
        using var factory = new TestAppFactory();

        var category = new Category
        {
            Id = Guid.NewGuid(),
            AccentColor = "#aaa",
            PrimaryColor = "#fff",
            Position = 1,
            Value = "Test category updated"
        };

        var request = new HttpRequestMessage(HttpMethod.Put, $"/Category/{category.Id}")
        {
            Content = JsonContent.Create(category)
        };
        request.Headers.Add("X-Test-Auth", "true");
        request.Headers.Add("X-Test-Role", role);

        var response = await factory.Client.SendAsync(request);

        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }
}
