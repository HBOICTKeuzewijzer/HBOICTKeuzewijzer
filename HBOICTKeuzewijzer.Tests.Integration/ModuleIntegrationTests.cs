using FluentAssertions;
using HBOICTKeuzewijzer.Api;
using Microsoft.AspNetCore.Mvc.Testing;
using System.Net;
using System.Text;
using HBOICTKeuzewijzer.Api.Models;
using HBOICTKeuzewijzer.Tests.Integration.Shared;
using Microsoft.Identity.Client;
using System.Text.Json;
using Azure;
using Microsoft.EntityFrameworkCore;

namespace HBOICTKeuzewijzer.Tests.Integration
{
    public class ModuleIntegrationTests
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
}
