using HBOICTKeuzewijzer.Api.DAL;
using HBOICTKeuzewijzer.Tests.Integration.Fixtures;
using Microsoft.Extensions.DependencyInjection;
using System.Net.Http.Json;
using Microsoft.EntityFrameworkCore;
using HBOICTKeuzewijzer.Api.Models;

namespace HBOICTKeuzewijzer.Tests.Integration;

[Collection("Shared Test Collection")]
public class SlbIntegrationTests(SharedDatabaseFixture fixture)
{
    private readonly HttpClient _client = fixture.Client;

    [Fact]
    public async Task Put_AddStudentToSlb_CreatesRelationInDatabase()
    {
        // Arrange
        await fixture.ResetDatabaseAsync();

        var slbUserId = Guid.NewGuid();
        var studentUserId = Guid.NewGuid();

        await fixture.ExecuteDbContextAsync(async db =>
        {
            var slbUser = new ApplicationUser
            {
                Id = slbUserId,
                DisplayName = "SLB'er",
                Email = "slb@example.com",
                ExternalId = "slb-external-id"
            };

            var studentUser = new ApplicationUser
            {
                Id = studentUserId,
                DisplayName = "Student",
                Email = "student@example.com",
                ExternalId = "student-external-id"
            };

            db.ApplicationUsers.AddRange(slbUser, studentUser);

            // Assign roles to test
            db.ApplicationUserRoles.AddRange(
                new ApplicationUserRole { ApplicationUserId = slbUserId, Role = Role.SLB },
                new ApplicationUserRole { ApplicationUserId = studentUserId, Role = Role.Student }
            );

            await db.SaveChangesAsync();
        });


        // Act
        var response = await _client.PutAsync($"/slb/{slbUserId}/{studentUserId}", null);

        // Assert
        response.EnsureSuccessStatusCode();

        // Optionally: verify DB contents
        await fixture.ExecuteDbContextAsync(async db =>
        {
            var relationExists = await db.Slb.AnyAsync(r => 
                    r.SlbApplicationUserId == slbUserId && 
                    r.StudentApplicationUserId == studentUserId);

            Assert.True(relationExists);
        });
    }
}