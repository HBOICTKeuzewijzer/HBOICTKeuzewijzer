using FluentAssertions;
using HBOICTKeuzewijzer.Api.Dtos;
using HBOICTKeuzewijzer.Api.Models;
using HBOICTKeuzewijzer.Api.Repositories;
using HBOICTKeuzewijzer.Tests.Integration.Shared;
using Microsoft.EntityFrameworkCore;
using System.Net;
using System.Net.Http.Json;

namespace HBOICTKeuzewijzer.Tests.Integration;

public class SlbIntegrationTests
{
    public class Put : SlbIntegrationTests
    {
        [Theory]
        [InlineData("SystemAdmin")]
        public async Task Put_AddStudentToSlb_CreatesRelationInDatabase(string role)
        {
            using var factory = new TestAppFactory();

            var slbId = Guid.NewGuid();
            var studentId = Guid.NewGuid();

            var slb = new ApplicationUser
            {
                Id = slbId,
                ExternalId = "slb-external-id",
                Email = "slb@example.com",
                DisplayName = "SLB’er"
            };

            var student = new ApplicationUser
            {
                Id = studentId,
                ExternalId = "student-external-id",
                Email = "student@example.com",
                DisplayName = "Student"
            };

            await SeedHelper.SeedAsync(factory.Services, slb);
            await SeedHelper.SeedAsync(factory.Services, student);

            var request = new HttpRequestMessage(HttpMethod.Put, $"/Slb/{slbId}/{studentId}");
            request.Headers.Add("X-Test-Auth", "true");
            request.Headers.Add("X-Test-Role", role);

            var response = await factory.Client.SendAsync(request);
            var responseBody = await response.Content.ReadAsStringAsync();
            Console.WriteLine("Server response:");
            Console.WriteLine(responseBody);


            response.StatusCode.Should().Be(HttpStatusCode.NoContent);

            await using var db = factory.CreateDbContext();
            var relationExists = await db.Slb
                .AnyAsync(r =>
                    r.SlbApplicationUserId == slbId &&
                    r.StudentApplicationUserId == studentId);

            relationExists.Should().BeTrue();
        }
    }

    public class Get : SlbIntegrationTests
    {
        [Theory]
        [InlineData("SystemAdmin")]
        public async Task GetStudentsForSlb_ReturnsStudentsForGivenSlb(string role)
        {
            using var factory = new TestAppFactory();

            var slbId = Guid.NewGuid();
            var student1Id = Guid.NewGuid();
            var student2Id = Guid.NewGuid();

            var slbUser = new ApplicationUser
            {
                Id = slbId,
                ExternalId = "slb-external-id",
                Email = "slb@example.com",
                DisplayName = "SLB'er",
                ApplicationUserRoles = new List<ApplicationUserRole>
        {
            new ApplicationUserRole
            {
                Id = Guid.NewGuid(),
                Role = Role.SLB,
                ApplicationUserId = slbId
            }
        }
            };

            var student1 = new ApplicationUser
            {
                Id = student1Id,
                ExternalId = "student1-external-id",
                Email = "student1@example.com",
                DisplayName = "Student One",
                ApplicationUserRoles = new List<ApplicationUserRole>
        {
            new ApplicationUserRole
            {
                Id = Guid.NewGuid(),
                Role = Role.Student,
                ApplicationUserId = student1Id
            }
        }
            };

            var student2 = new ApplicationUser
            {
                Id = student2Id,
                ExternalId = "student2-external-id",
                Email = "student2@example.com",
                DisplayName = "Student Two",
                ApplicationUserRoles = new List<ApplicationUserRole>
        {
            new ApplicationUserRole
            {
                Id = Guid.NewGuid(),
                Role = Role.Student,
                ApplicationUserId = student2Id
            }
        }
            };

            var slbRelation1 = new Slb
            {
                Id = Guid.NewGuid(),
                SlbApplicationUserId = slbId,
                StudentApplicationUserId = student1Id
            };

            var slbRelation2 = new Slb
            {
                Id = Guid.NewGuid(),
                SlbApplicationUserId = slbId,
                StudentApplicationUserId = student2Id
            };

            await SeedHelper.SeedAsync(factory.Services, slbUser);
            await SeedHelper.SeedAsync(factory.Services, student1);
            await SeedHelper.SeedAsync(factory.Services, student2);
            await SeedHelper.SeedAsync(factory.Services, slbRelation1);
            await SeedHelper.SeedAsync(factory.Services, slbRelation2);

            var request = new HttpRequestMessage(HttpMethod.Get, $"/Slb/{slbUser.Id}/students?pageNumber=1&pageSize=10");
            request.Headers.Add("X-Test-Auth", "true");
            request.Headers.Add("X-Test-Role", role);

            var response = await factory.Client.SendAsync(request);

            response.StatusCode.Should().Be(HttpStatusCode.OK);

            var pagedResult = await response.Content.ReadFromJsonAsync<PaginatedResult<StudentDto>>();

            pagedResult.Should().NotBeNull();
            pagedResult.Items.Should().HaveCountGreaterThan(0);
            pagedResult.Items.Should().Contain(s => s.Id == student1Id);
            pagedResult.Items.Should().Contain(s => s.Id == student2Id);
            pagedResult.Page.Should().Be(1);
            pagedResult.PageSize.Should().Be(10);
        }

        [Theory]
        [InlineData("SLB")]
        public async Task GetStudentsForUser_ReturnsStudentsForGivenSlb(string role)
        {

        }
    }

}
