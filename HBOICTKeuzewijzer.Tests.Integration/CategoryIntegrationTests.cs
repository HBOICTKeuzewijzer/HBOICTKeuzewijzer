using HBOICTKeuzewijzer.Api.DAL;
using HBOICTKeuzewijzer.Tests.Integration.Fixtures;
using Microsoft.Extensions.DependencyInjection;
using System.Net.Http.Json;
using Microsoft.EntityFrameworkCore;

namespace HBOICTKeuzewijzer.Tests.Integration;

[Collection("Shared Test Collection")]
public class CategoryIntegrationTests(SharedDatabaseFixture fixture)
{
    private readonly HttpClient _client = fixture.Client;

    [Fact]
    public async Task PostCategory_CreatesModuleInDatabase()
    {
        // Arrange
        await fixture.ResetDatabaseAsync();

        var newCategory = new
        {
            value = "Software engineering",
            primaryColor = "#fff",
            accentColor = "#000",
            position = 1
        };

        // Act
        var response = await _client.PostAsJsonAsync("/category", newCategory);

        // Assert
        response.EnsureSuccessStatusCode();

        // Optionally: verify DB contents
        using var scope = new ServiceCollection()
            .AddDbContext<AppDbContext>(options =>
                options.UseSqlServer(fixture.DbContainer.GetConnectionString()))
            .BuildServiceProvider()
            .CreateScope();

        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var categoryInDb = await db.Categories.FirstOrDefaultAsync(m => m.Value == newCategory.value);

        Assert.NotNull(categoryInDb);
        Assert.Equal(newCategory.value, categoryInDb.Value);
        Assert.Equal(newCategory.primaryColor, categoryInDb.PrimaryColor);
        Assert.Equal(newCategory.accentColor, categoryInDb.AccentColor);
        Assert.Equal(newCategory.position, categoryInDb.Position);
    }
}