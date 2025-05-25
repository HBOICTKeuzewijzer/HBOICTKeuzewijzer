using HBOICTKeuzewijzer.Api.Models;
using System.Net;
using System.Net.Http.Json;
using FluentAssertions;

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
            Position = 1
        };

        var request = new HttpRequestMessage(HttpMethod.Post, "/Category")
        {
            Content = JsonContent.Create(newCategory)
        };
        request.Headers.Add("X-Test-Role", role);

        var response = await factory.Client.SendAsync(request);

        response.StatusCode.Should().Be(HttpStatusCode.Created);
        response.Headers.Location!.AbsolutePath.Should().Be($"/Category/{newCategory.Id}");

        var saved = await factory.DbContext.Categories.FindAsync(newCategory.Id);
        saved.Should().NotBeNull();
        saved!.Value.Should().Be("Test Category");
    }
}
