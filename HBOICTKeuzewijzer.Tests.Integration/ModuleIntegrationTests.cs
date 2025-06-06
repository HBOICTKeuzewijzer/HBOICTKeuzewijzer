using FluentAssertions;
using HBOICTKeuzewijzer.Api.Models;
using HBOICTKeuzewijzer.Tests.Integration.Shared;
using Microsoft.EntityFrameworkCore;
using System.Net;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;

namespace HBOICTKeuzewijzer.Tests.Integration
{
    public class ModuleIntegrationTests
    {
        public class Get : ModuleIntegrationTests
        {
            [InlineData("SystemAdmin")]
            [InlineData("ModuleAdmin")]
            [Fact]
            public async Task GET_retrieves_Modules()
            {
                using var application = new TestAppFactory();
                using var client = application.Client;

                var response = await client.GetAsync("/Module");
                response.StatusCode.Should().Be(HttpStatusCode.OK);
            }

            [Fact]
            public async Task GET_ById_Retrieves_A_Module()
            {

                using var application = new TestAppFactory();
                using var client = application.Client;

                var testModule = new Module
                {
                    Id = Guid.NewGuid(),
                    Name = "test",
                    Code = "ICT-001",
                    Description = "testmodule",
                    ECs = 5,
                    Level = 1,
                    Required = false,
                    IsPropaedeutic = true,
                    Oer = new Oer { Id = Guid.NewGuid(), AcademicYear = "24/25" }
                };

                await SeedHelper.SeedAsync(application.Services, testModule);

                var response = await client.GetAsync($"/Module/{testModule.Id}");
                response.StatusCode.Should().Be(HttpStatusCode.OK);
            }
        }

        public class Post : ModuleIntegrationTests
        {

            [Fact]
            public async Task POST_Module_AddsModule()
            {
                using var application = new TestAppFactory();
                using var client = application.Client;

                var oerId = Guid.NewGuid();

                await SeedHelper.SeedAsync(application.Services, new Oer
                {
                    Id = oerId,
                    AcademicYear = "24/25",
                });

                // Dit is wat je POST body moet zijn
                var dto = new
                {
                    name = "test1",
                    code = "ICT-002",
                    description = "testmodule1",
                    ecs = 5,
                    level = 1,
                    required = false,
                    isPropaedeutic = true,
                    categoryId = (Guid?)null,
                    requiredSemester = 1
                };

                var content = new StringContent(
                    JsonSerializer.Serialize(dto),
                    Encoding.UTF8,
                    "application/json");

                var response = await client.PostAsync("/Module", content);

                response.StatusCode.Should().Be(HttpStatusCode.Created);
            }
        }

        public class Delete : ModuleIntegrationTests
        {
            [InlineData("SystemAdmin")]
            [InlineData("ModuleAdmin")]
            [Fact]
            public async Task Delete_Module_ById()
            {
                using var application = new TestAppFactory();
                using var client = application.Client;

                var testModule = new Module
                {
                    Id = Guid.NewGuid(),
                    Name = "test4",
                    Code = "ICT-003",
                    Description = "testmodule2",
                    ECs = 10,
                    Level = 1,
                    Required = false,
                    IsPropaedeutic = true,
                    CategoryId = (Guid?)null
                };

                await SeedHelper.SeedAsync(application.Services, new Oer
                {
                    Id = testModule.OerId,
                    AcademicYear = "25/26"
                });


                await SeedHelper.SeedAsync(application.Services, testModule);

                var request = new HttpRequestMessage(HttpMethod.Delete, $"/Module/{testModule.Id}");
                request.Headers.Add("X-Test-Auth", "true");
                request.Headers.Add("X-Test-Role", "SystemAdmin");

                var Response = await client.SendAsync(request);

                Response.StatusCode.Should().Be(HttpStatusCode.NoContent);

                // checken of DB leeg is
                List<Module> currentModules;
                await using (var context = application.CreateDbContext())
                {
                    currentModules = await context.Modules.ToListAsync();
                }

                currentModules.Should().NotBeNull();
                currentModules.Should().NotContain(m => m.Id == testModule.Id);
            }
        }

        public class Update : ModuleIntegrationTests
        {
            [Theory]
            [InlineData("SystemAdmin")]
            [InlineData("ModuleAdmin")]
            public async Task Update_Module_Succes(string role)
            {
                using var application = new TestAppFactory();
                using var client = application.Client;

                var moduleId = Guid.NewGuid();

                var oerId = Guid.NewGuid();

                await SeedHelper.SeedAsync(application.Services, new Oer
                {
                    Id = oerId,
                    Filepath = "Test OER",
                    AcademicYear = "25/26"
                });


                var oldModule = new Module
                {
                    Id = moduleId,
                    Name = "test5",
                    Code = "ICT-005",
                    Description = "testmodule3",
                    ECs = 10,
                    Level = 1,
                    Required = false,
                    IsPropaedeutic = true,
                    CategoryId = (Guid?)null,
                    OerId = oerId
                };

                await SeedHelper.SeedAsync(application.Services, oldModule);

                var updatedModule = new Module
                {
                    Id = moduleId,
                    Name = "test6",
                    Code = "ICT-005",
                    Description = "testmodule3",
                    ECs = 10,
                    Level = 1,
                    Required = false,
                    IsPropaedeutic = true,
                    CategoryId = (Guid?)null,
                    OerId = oerId
                };

                var request = new HttpRequestMessage(HttpMethod.Put, $"/Module/{updatedModule.Id}")
                {
                    Content = JsonContent.Create(updatedModule)
                };

                request.Headers.Add("X-Test-Auth", "true");
                request.Headers.Add("X-Test-Role", role);

                var response = await application.Client.SendAsync(request);

                response.StatusCode.Should().Be(HttpStatusCode.NoContent);

                Module? saved;
                await using (var context = application.CreateDbContext())
                {
                    saved = await context.Modules.FindAsync(moduleId);
                    saved.Should().NotBeNull();
                }

                saved!.Name.Should().Be("test6");
            }


            [Fact]
            public async Task PutModule_RespondsWithUnauthorized_WhenNotAuthenticated()
            {
                using var application = new TestAppFactory();
                using var client = application.Client;

                var moduleId = Guid.NewGuid();

                var module = new Module
                {
                    Id = moduleId,
                    Name = "Unauthorized Test",
                    Code = "ICT-888",
                    Description = "Unauthorized",
                    ECs = 5,
                    Level = 1,
                    Required = false,
                    IsPropaedeutic = true,
                    OerId = Guid.NewGuid()
                };

                var request = new HttpRequestMessage(HttpMethod.Put, $"/Module/{module.Id}")
                {
                    Content = JsonContent.Create(module)
                };

                // GEEN auth headers → dus verwacht 401
                var response = await client.SendAsync(request);

                response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
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
        }
    }
}
