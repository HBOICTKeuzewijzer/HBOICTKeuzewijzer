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

        [Theory]
        [InlineData("SystemAdmin")]
        public async Task Put_ChangeStudents_ReplacesExistingRelationsCorrectly(string role)
        {
            using var factory = new TestAppFactory();

            var slbId = Guid.NewGuid();
            var student1Id = Guid.NewGuid();
            var student2Id = Guid.NewGuid();
            var student3Id = Guid.NewGuid();

            var slb = new ApplicationUser
            {
                Id = slbId
            };

            var student1 = new ApplicationUser
            {
                Id = student1Id
            };

            var student2 = new ApplicationUser
            {
                Id = student2Id
            };

            var student3 = new ApplicationUser
            {
                Id = student3Id
            };

            var relation1 = new Slb
            {
                Id = Guid.NewGuid(),
                SlbApplicationUserId = slbId,
                StudentApplicationUserId = student1Id
            };

            var relation2 = new Slb
            {
                Id = Guid.NewGuid(),
                SlbApplicationUserId = slbId,
                StudentApplicationUserId = student2Id
            };

            await SeedHelper.SeedAsync(factory.Services, slb);
            await SeedHelper.SeedAsync(factory.Services, student1);
            await SeedHelper.SeedAsync(factory.Services, student2);
            await SeedHelper.SeedAsync(factory.Services, student3);
            await SeedHelper.SeedAsync(factory.Services, relation1);
            await SeedHelper.SeedAsync(factory.Services, relation2);

            var newStudents = new List<Guid> { student2Id, student3Id }; // student1 will be removed, student2 and student3 will remain

            var request = new HttpRequestMessage(HttpMethod.Put, $"/Slb/ChangeStudents/{slbId}")
            {
                Content = JsonContent.Create(newStudents)
            };
            request.Headers.Add("X-Test-Auth", "true");
            request.Headers.Add("X-Test-Role", role);

            var response = await factory.Client.SendAsync(request);

            response.StatusCode.Should().Be(HttpStatusCode.NoContent);

            await using var db = factory.CreateDbContext();

            var hasStudent1 = await db.Slb
                .AnyAsync(r => r.SlbApplicationUserId == slbId && r.StudentApplicationUserId == student1Id);
            var hasStudent2 = await db.Slb
                .AnyAsync(r => r.SlbApplicationUserId == slbId && r.StudentApplicationUserId == student2Id);
            var hasStudent3 = await db.Slb
                .AnyAsync(r => r.SlbApplicationUserId == slbId && r.StudentApplicationUserId == student3Id);

            hasStudent1.Should().BeFalse(); // student1 should be removed
            hasStudent2.Should().BeTrue(); // student2 should be kept
            hasStudent3.Should().BeTrue(); // student3 should be added
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
        public async Task GetStudentsForUser_ReturnsStudentsForCurrentSlb(string role)
        {
            using var factory = new TestAppFactory();

            var slbId = Guid.NewGuid();
            var student1Id = Guid.NewGuid();

            var slbUserId = Guid.NewGuid().ToString();

            var slbUser = new ApplicationUser
            {
                Id = slbId,
                ExternalId = slbUserId,
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
                Email = "student@example.com",
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

            var relation = new Slb
            {
                Id = Guid.NewGuid(),
                SlbApplicationUserId = slbId,
                StudentApplicationUserId = student1Id
            };

            await SeedHelper.SeedAsync(factory.Services, slbUser);
            await SeedHelper.SeedAsync(factory.Services, student1);
            await SeedHelper.SeedAsync(factory.Services, relation);

            var request = new HttpRequestMessage(HttpMethod.Get, "/Slb/myStudents?pageNumber=1&pageSize=10");
            request.Headers.Add("X-Test-Auth", "true");
            request.Headers.Add("X-Test-Role", role);
            request.Headers.Add("X-User-Id", slbUserId);

            var response = await factory.Client.SendAsync(request);

            response.StatusCode.Should().Be(HttpStatusCode.OK);

            var pagedResult = await response.Content.ReadFromJsonAsync<PaginatedResult<StudentDto>>();
            pagedResult.Should().NotBeNull();
            pagedResult.Items.Should().Contain(s => s.Id == student1Id);
        }
    }
    public class Delete : SlbIntegrationTests
    {
        [Theory]
        [InlineData("SystemAdmin")]
        public async Task Delete_RemoveStudentFromSlb_DeletesRelationFromDatabase(string role)
        {
            using var factory = new TestAppFactory();

            var slbId = Guid.NewGuid();
            var studentId = Guid.NewGuid();

            var slb = new ApplicationUser
            {
                Id = slbId,
            };

            var student = new ApplicationUser
            {
                Id = studentId,
            };

            var relation = new Slb
            {
                Id = Guid.NewGuid(),
                SlbApplicationUserId = slbId,
                StudentApplicationUserId = studentId
            };

            await SeedHelper.SeedAsync(factory.Services, slb);
            await SeedHelper.SeedAsync(factory.Services, student);
            await SeedHelper.SeedAsync(factory.Services, relation);

            var request = new HttpRequestMessage(HttpMethod.Delete, $"/Slb/{slbId}/{studentId}");
            request.Headers.Add("X-Test-Auth", "true");
            request.Headers.Add("X-Test-Role", role);

            var response = await factory.Client.SendAsync(request);

            response.StatusCode.Should().Be(HttpStatusCode.NoContent);

            await using var db = factory.CreateDbContext();
            var stillExists = await db.Slb
                .AnyAsync(r =>
                    r.SlbApplicationUserId == slbId &&
                    r.StudentApplicationUserId == studentId);
            stillExists.Should().BeFalse();
        }
    }
}
